using System;
using System.Collections.Generic;

using MoonSharp.Interpreter;

using PPR.Main;
using PPR.Main.Managers;
using PPR.Properties;

using PRR;

using SFML.Graphics;
using SFML.System;

using Alignment = PRR.Renderer.Alignment;

namespace PPR.GUI.Elements {
    public sealed class OnClickEventArgs : EventArgs {
        public string id { get; }

        public OnClickEventArgs(string id) => this.id = id;
    }
    
    public class Button : UIElement {
        public enum State { Idle, Hovered, Clicked, Selected }

        public override string type => "button";

        public string text { get; set; }

        public int width {
            get => _width;
            set {
                _width = value;
                _animRateOffsets = new float[value];
            }
        }

        public override Vector2i size {
            get => new Vector2i(width, 1);
            set => width = value.X;
        }

        public event EventHandler<OnClickEventArgs> onClick;
        public event EventHandler<OnClickEventArgs> onHover;
        private Color idleColor => GetColor("idle");
        private Color hoverColor => GetColor("hover");
        private Color clickColor => GetColor("click");
        public State currentState { get; private set; } = State.Hovered;
        public State prevFrameState { get; private set; } = State.Hovered;
        public bool selected = false;

        private readonly OnClickEventArgs _onClickArgs;
        private int _width;
        private readonly Alignment _align;
        private DateTime _animStartTime;
        private float _animTime;
        private float[] _animRateOffsets;
        private State _prevState = State.Hovered;
        private bool _hotkeyPressed;
        private bool _prevFrameHotkeyPressed;
        private Color _currentColor;
        private Color _prevColor;
        private int _posX;

        public Button(string id, List<string> tags, Vector2i? position, int width, Vector2f? anchor, UIElement parent,
            string text, InputKey hotkey = null, Alignment align = Alignment.Left) :
            base(id, tags, position, new Vector2i(width, 1), anchor, parent) {
            this.text = text;
            this.width = width;
            _align = align;
            _animRateOffsets = new float[width];
            _currentColor = hoverColor;
            _onClickArgs = new OnClickEventArgs(id);
            Core.renderer.window.KeyPressed += (_, key) => {
                if(hotkey != null && hotkey.IsPressed(key)) _hotkeyPressed = true;
            };
            Core.renderer.window.KeyReleased += (_, key) => {
                if(hotkey != null && hotkey.IsPressed(key)) _hotkeyPressed = false;
            };
        }

        private State GetState() {
            _posX = globalPosition.X - _align switch {
                Alignment.Right => text.Length - 1,
                Alignment.Center => (int)MathF.Floor(text.Length / 2f),
                _ => 0
            };
            bool onButton;
            bool wasOnButton;
            if(mask == null) {
                onButton =
                    Core.renderer.mousePosition.InBounds(_posX, globalPosition.Y, _posX + width - 1, globalPosition.Y);
                wasOnButton = UI.LineSegmentIntersection(UI.prevMousePosition, Core.renderer.mousePosition,
                    new Vector2i(_posX, globalPosition.Y), new Vector2i(_posX + width - 1, globalPosition.Y));
            }
            else {
                Bounds maskBounds = mask.bounds;
                Vector2i minBound = new Vector2i(Math.Max(_posX, maskBounds.min.X),
                    Math.Max(globalPosition.Y, maskBounds.min.Y));
                Vector2i maxBound = new Vector2i(Math.Min(_posX + width - 1, maskBounds.max.X),
                    Math.Min(globalPosition.Y, maskBounds.max.Y));
                onButton = Core.renderer.mousePosition.InBounds(minBound, maxBound);
                wasOnButton = UI.LineSegmentIntersection(UI.prevMousePosition, Core.renderer.mousePosition,
                    minBound, maxBound);
            }
            return wasOnButton || _prevFrameHotkeyPressed ?
                Core.renderer.leftButtonPressed && onButton || _hotkeyPressed ? State.Clicked : State.Hovered :
                selected ? State.Selected : State.Idle;
        }
        
        public override void Draw() {
            base.Draw();

            Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> useAnimationModifier = animationModifier;
            
            Vector2i globalPos = globalPosition;
            
            if(text != null) {
                if(mask == null) {
                    Core.renderer.DrawText(globalPos, text.Substring(0, Math.Min(text.Length, width)), _align,
                        false, false, useAnimationModifier);
                }
                else {
                    Bounds maskBounds = mask.bounds;
                    if(globalPos.Y >= maskBounds.min.Y && globalPos.Y <= maskBounds.max.Y) {
                        int minX = Math.Max(0, maskBounds.min.X - globalPos.X);
                        int maxX = Math.Min(text.Length, maskBounds.max.X - globalPos.X - minX);
                        Core.renderer.DrawText(globalPos + new Vector2i(minX, 0), text.Substring(minX, maxX),
                            _align, false, false, useAnimationModifier);
                    }
                }
            }

            UpdateState();
            _animTime = (float)(DateTime.UtcNow - _animStartTime).TotalSeconds;

            if(Core.renderer.window.HasFocus()) {
                if(currentState == State.Hovered) {
                    switch(prevFrameState) {
                        case State.Clicked:
                            SoundManager.PlaySound(SoundType.Click);
                            onClick?.Invoke(this, _onClickArgs);
                            break;
                        case State.Idle:
                        case State.Selected:
                            onHover?.Invoke(this, _onClickArgs);
                            break;
                    }
                }
            }

            for(int x = 0; x < width; x++) {
                Vector2i pos = new Vector2i(_posX + x, globalPos.Y);
                if(mask != null) {
                    Bounds maskBounds = mask.bounds;
                    if(pos.X < maskBounds.min.X || pos.X > maskBounds.max.X ||
                       pos.Y < maskBounds.min.Y || pos.Y > maskBounds.max.Y) break;
                }
                Color foreground = Renderer.AnimateColor(_animTime, _currentColor,
                    currentState == State.Idle ? hoverColor : idleColor, 4f + _animRateOffsets[x]);
                Color background =
                    Renderer.AnimateColor(_animTime, _prevColor, _currentColor, 4f + _animRateOffsets[x]);
                if(useAnimationModifier == null) Core.renderer.SetCellColor(pos, foreground, background);
                else {
                    RenderCharacter character =
                        new RenderCharacter(background, foreground, Core.renderer.GetCharacter(pos));
                    (Vector2i newPos, RenderCharacter newCharacter) = useAnimationModifier(pos, character);
                    Core.renderer.SetCharacter(newPos, newCharacter);
                }
            }

            _prevFrameHotkeyPressed = Core.renderer.window.HasFocus() && _hotkeyPressed;
        }

        private void UpdateState() {
            prevFrameState = currentState;
            currentState = GetState();
            if(_prevState != currentState) {
                Color color = idleColor;
                switch(currentState) {
                    case State.Hovered:
                        color = hoverColor;
                        break;
                    case State.Selected:
                    case State.Clicked:
                        color = clickColor;
                        break;
                }

                if(_currentColor != color) {
                    _prevColor = _currentColor;
                    for(int x = 0; x < width; x++) _animRateOffsets[x] = new Random().NextFloat(-1f, 1f);
                    _animStartTime = DateTime.UtcNow;
                }

                _currentColor = color;
            }

            _prevState = currentState;
        }
    }
}
