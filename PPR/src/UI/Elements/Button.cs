using System;
using System.Collections.Generic;

using PPR.Main;
using PPR.Main.Managers;
using PPR.Properties;
using PPR.UI.Animations;

using PRR;

using SFML.Graphics;
using SFML.System;

using Alignment = PRR.Renderer.Alignment;

namespace PPR.UI.Elements {
    public class Button : Element {
        public enum State { Idle, Hovered, Clicked, Selected }

        public override string type => "button";

        public string text { get; set; }

        public int width {
            get => _width;
            set {
                _width = value;
                UpdateMaxValues(value);
            }
        }

        public override Vector2i size {
            get => new Vector2i(width, 1);
            set => width = value.X;
        }

        public AnimationSettings idleAnimation { get; set; }
        public AnimationSettings hoverAnimation { get; set; }
        public AnimationSettings clickAnimation { get; set; }

        private Color idleColor => GetColor("idle");
        private Color hoverColor => GetColor("hover");
        private Color clickColor => GetColor("click");
        public State currentState { get; private set; } = State.Hovered;
        public State prevFrameState { get; private set; } = State.Hovered;
        public bool selected = false;

        private int _width;
        private readonly Alignment _align;
        private bool _hotkeyPressed;
        private bool _prevHotkeyPressed;
        private Color _currentColor;
        private int _posX;
        private bool _onButton;
        private bool _wasOnButton;
        private Animation[,] _currentAnimations;
        private Color[,] _currentBackgroundColors;
        private Color[,] _currentForegroundColors;

        public Button(string id, AnimationSettings idleAnimation, AnimationSettings hoverAnimation,
            AnimationSettings clickAnimation, List<string> tags, Vector2i? position, int width, Vector2f? anchor,
            Element parent, string text, InputKey hotkey = null, Alignment align = Alignment.Left) :
            base(id, tags, position, new Vector2i(width, 1), anchor, parent) {
            this.text = text;
            this.idleAnimation = idleAnimation;
            this.hoverAnimation = hoverAnimation;
            this.clickAnimation = clickAnimation;
            this.width = width;
            _align = align;
            _currentColor = hoverColor;
            Core.renderer.window.KeyPressed += (_, key) => {
                if(hotkey != null && hotkey.IsPressed(key)) _hotkeyPressed = true;
            };
            Core.renderer.window.KeyReleased += (_, key) => {
                if(hotkey != null && hotkey.IsPressed(key)) _hotkeyPressed = false;
            };
        }

        private State GetState() => _wasOnButton || _prevHotkeyPressed ?
            Core.renderer.leftButtonPressed && _onButton || _hotkeyPressed ? State.Clicked : State.Hovered :
            selected ? State.Selected : State.Idle;

        public override void Update() {
            base.Update();

            if(!enabled) {
                _onButton = false;
                _wasOnButton = false;
                currentState = State.Hovered;
                prevFrameState = State.Hovered;
                return;
            }

            _posX = globalPosition.X - _align switch {
                Alignment.Right => text.Length - 1,
                Alignment.Center => (int)MathF.Floor(text.Length / 2f),
                _ => 0
            };
            
            if(mask is null) {
                _onButton =
                    Core.renderer.mousePosition.InBounds(_posX, globalPosition.Y, _posX + width - 1, globalPosition.Y);
                _wasOnButton = UI.Manager.LineSegmentIntersection(UI.Manager.prevMousePosition, Core.renderer.mousePosition,
                    new Vector2i(_posX, globalPosition.Y), new Vector2i(_posX + width - 1, globalPosition.Y));
            }
            else {
                Bounds maskBounds = mask.bounds;
                Vector2i minBound = new Vector2i(Math.Max(_posX, maskBounds.min.X),
                    Math.Max(globalPosition.Y, maskBounds.min.Y));
                Vector2i maxBound = new Vector2i(Math.Min(_posX + width, maskBounds.max.X),
                    Math.Min(globalPosition.Y, maskBounds.max.Y));
                _onButton = Core.renderer.mousePosition.InBounds(minBound, maxBound);
                _wasOnButton = UI.Manager.LineSegmentIntersection(UI.Manager.prevMousePosition, Core.renderer.mousePosition,
                    minBound, maxBound);
            }
            
            UpdateState();
            
            if(!(_currentAnimations is null))
                foreach(Animation animation in _currentAnimations)
                    animation?.Update();
            
            if(!Core.renderer.window.HasFocus() || currentState != State.Hovered) {
                prevFrameState = currentState;
                return;
            }
            switch(prevFrameState) {
                case State.Clicked when currentState == State.Hovered:
                    SoundManager.PlaySound(SoundType.Click);
                    Lua.Manager.InvokeEvent(this, "buttonClicked", this);
                    break;
                case State.Idle:
                case State.Selected:
                    Lua.Manager.InvokeEvent(this, "buttonHovered", this);
                    break;
            }
            prevFrameState = currentState;
        }

        private void UpdateState() {
            currentState = GetState();
            _prevHotkeyPressed = Core.renderer.window.HasFocus() && _hotkeyPressed;
            if(prevFrameState == currentState) return;

            Color color = idleColor;
            AnimationSettings animation = idleAnimation;
            switch(currentState) {
                case State.Hovered:
                    color = hoverColor;
                    animation = hoverAnimation;
                    break;
                case State.Selected:
                case State.Clicked:
                    color = clickColor;
                    animation = clickAnimation;
                    break;
            }

            _currentColor = color;
            RestartAnimations(animation);
        }

        public override void Draw() {
            base.Draw();

            Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> animationModifier = ApplyAnimations;
            Vector2i globalPos = globalPosition;
            
            DrawText(globalPos, animationModifier);
            DrawButton(globalPos, animationModifier);
        }

        private void DrawText(Vector2i globalPos,
            Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> animationModifier) {
            if(text is null) return;
            if(mask is null) {
                Core.renderer.DrawText(globalPos, text.Substring(0, Math.Min(text.Length, width)), _align,
                    true, false, animationModifier);
            }
            else {
                Bounds maskBounds = mask.bounds;
                if(globalPos.Y < maskBounds.min.Y || globalPos.Y > maskBounds.max.Y) return;
                int minX = Math.Max(0, maskBounds.min.X - globalPos.X);
                int maxX = Math.Min(text.Length, maskBounds.max.X - globalPos.X - minX + 1);
                Core.renderer.DrawText(globalPos + new Vector2i(minX, 0), text.Substring(minX, maxX),
                    _align, true, false, animationModifier);
            }
        }

        private void DrawButton(Vector2i globalPos,
            Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> animationModifier) {
            for(int x = 0; x < width; x++) {
                Vector2i pos = new Vector2i(_posX + x, globalPos.Y);
                if(!(mask is null)) {
                    Bounds maskBounds = mask.bounds;
                    if(pos.X < maskBounds.min.X || pos.X > maskBounds.max.X ||
                       pos.Y < maskBounds.min.Y || pos.Y > maskBounds.max.Y) break;
                }

                Color foreground = currentState == State.Idle ? hoverColor : idleColor;
                Color background = _currentColor;
                RenderCharacter character =
                    new RenderCharacter(background, foreground, Core.renderer.GetCharacter(pos));
                (Vector2i pos, RenderCharacter character) mod =
                    _currentAnimations[x, 0]?.animationModifier?.Invoke(pos, character) ?? (pos, character);
                mod = animationModifier(mod.pos, mod.character);
                Core.renderer.SetCharacter(mod.pos, mod.character);
                _currentBackgroundColors[x, 0] = mod.character.background;
                _currentForegroundColors[x, 0] = mod.character.foreground;
            }
        }

        private void UpdateMaxValues(int maxValue) {
            _currentAnimations = new Animation[maxValue, size.Y];
            _currentBackgroundColors = new Color[maxValue, size.Y];
            _currentForegroundColors = new Color[maxValue, size.Y];
        }
        
        private void RestartAnimations(AnimationSettings animationSettings) {
            for(int x = 0; x < _currentBackgroundColors.GetLength(0); x++) {
                for(int y = 0; y < _currentBackgroundColors.GetLength(1); y++) {
                    Animation animation = new Animation(animationSettings, null,
                        new Dictionary<string, double> {
                            { "startBgR", _currentBackgroundColors[x, y].R },
                            { "startBgG", _currentBackgroundColors[x, y].G },
                            { "startBgB", _currentBackgroundColors[x, y].B },
                            { "startBgA", _currentBackgroundColors[x, y].A },
                            { "startFgR", _currentForegroundColors[x, y].R },
                            { "startFgG", _currentForegroundColors[x, y].G },
                            { "startFgB", _currentForegroundColors[x, y].B },
                            { "startFgA", _currentForegroundColors[x, y].A }
                        });
                    animation.Restart();
                    _currentAnimations[x, y]?.Stop();
                    _currentAnimations[x, y] = animation;
                }
            }
        }
    }
}
