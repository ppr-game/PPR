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
    public class Button : UIElement {
        public enum State { Idle, Hovered, Clicked, Selected }

        public override string type => "button";

        public string text { get; set; }

        public int width {
            get => _width;
            set {
                _width = value;
                _animTimes = new float[value];
                _animRateOffsets = new float[value];
            }
        }

        public override Vector2i size {
            get => new Vector2i(width, 1);
            set => width = value.X;
        }

        public List<Closure> onClick { get; set; }
        public List<Closure> onHover { get; set; }
        private Color idleColor => ColorScheme.TryGetColor($"button_{id}_idle") ??
            (tags != null && tags.Count > 0 ? ColorScheme.GetColor($"button_@{tags[0]}_idle") : Color.Transparent);
        private Color hoverColor => ColorScheme.TryGetColor($"button_{id}_hover") ??
            (tags != null && tags.Count > 0 ? ColorScheme.GetColor($"button_@{tags[0]}_hover") : Color.Transparent);
        private Color clickColor => ColorScheme.TryGetColor($"button_{id}_click") ??
            (tags != null && tags.Count > 0 ? ColorScheme.GetColor($"button_@{tags[0]}_click") : Color.Transparent);
        public State currentState { get; private set; } = State.Hovered;
        public State prevFrameState { get; private set; } = State.Hovered;
        public bool selected = false;

        private readonly DynValue[] _onClickArgs;
        private int _width;
        private readonly Alignment _align;
        private float[] _animTimes;
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
            _animTimes = new float[width];
            _animRateOffsets = new float[width];
            _currentColor = hoverColor;
            _onClickArgs = new DynValue[] { DynValue.NewString(id) };
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
            bool onButton = Core.renderer.mousePosition.InBounds(_posX, globalPosition.Y, _posX + width - 1, globalPosition.Y);
            bool wasOnButton = UI.LineSegmentIntersection(UI.prevMousePosition, Core.renderer.mousePosition,
                new Vector2i(_posX, globalPosition.Y), new Vector2i(_posX + width - 1, globalPosition.Y));
            return wasOnButton || _prevFrameHotkeyPressed ?
                Core.renderer.leftButtonPressed && onButton || _hotkeyPressed ? State.Clicked : State.Hovered :
                selected ? State.Selected : State.Idle;
        }
        
        public override void Draw() {
            base.Draw();
            
            if(text != null)
                Core.renderer.DrawText(globalPosition, text.Substring(0, Math.Min(text.Length, width)), _align, false,
                false, animationModifier);

            UpdateState();

            if(Core.renderer.window.HasFocus()) {
                if(currentState == State.Hovered) {
                    switch(prevFrameState) {
                        case State.Clicked:
                            SoundManager.PlaySound(SoundType.Click);
                            foreach(Closure closure in onClick) closure?.Call(_onClickArgs);
                            break;
                        case State.Idle:
                        case State.Selected:
                            foreach(Closure closure in onHover) closure?.Call(_onClickArgs);
                            break;
                    }
                }
            }

            // TODO: transition implementation
            for(int x = 0; x < width; x++) {
                Vector2i pos = new Vector2i(_posX + x, globalPosition.Y);
                Core.renderer.SetCellColor(pos,
                    Renderer.AnimateColor(_animTimes[x], _currentColor,
                        currentState == State.Idle ? hoverColor : idleColor, 4f + _animRateOffsets[x]),
                    Renderer.AnimateColor(_animTimes[x], _prevColor, _currentColor, 4f + _animRateOffsets[x]));
                _animTimes[x] += Core.deltaTime;
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
                    for(int x = 0; x < width; x++) {
                        _animTimes[x] = 0f;
                        _animRateOffsets[x] = new Random().NextFloat(-1f, 1f);
                    }
                }

                _currentColor = color;
            }

            _prevState = currentState;
        }
    }
}
