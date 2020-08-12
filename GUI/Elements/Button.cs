using System;

using PPR.Main;
using PPR.Properties;
using PPR.Rendering;

using SFML.Graphics;

namespace PPR.GUI.Elements {
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    // ReSharper disable MemberCanBePrivate.Global
    public class Button {
        public Vector2 position;
        public string text;
        public string id;
        int _width;
        public int width {
            get => _width;
            set {
                _width = value;
                _animTimes = new float[value];
                _animRateOffsets = new float[value];
            }
        }
        public Color idleColor;
        public Color hoverColor;
        public Color clickColor;
        public Renderer.Alignment align;
        public InputKey hotkey;
        bool _hotkeyPressed;
        bool _prevFrameHotkeyPressed;
        Color _currentColor;
        Color _prevColor;
        public State currentState = State.Hovered;
        State _prevState = State.Hovered;
        public State prevFrameState = State.Hovered;
        float[] _animTimes;
        float[] _animRateOffsets;
        public bool selected = false;
        int _posX;
        public enum State { Idle, Hovered, Clicked, Selected };
        public Button(Vector2 position, string text, string id, int width, Color idleColor, Color hoverColor, Color clickColor,
                InputKey hotkey = null, Renderer.Alignment align = Renderer.Alignment.Left) {
            this.position = position;
            this.text = text;
            this.id = id;
            this.width = width;
            this.idleColor = idleColor;
            this.hoverColor = hoverColor;
            this.clickColor = clickColor;
            this.align = align;
            _animTimes = new float[width];
            _animRateOffsets = new float[width];
            _currentColor = hoverColor;
            this.hotkey = hotkey;
            Core.renderer.window.KeyPressed += (_, key) => {
                if(this.hotkey != null && this.hotkey.IsPressed(key)) _hotkeyPressed = true;
            };
            Core.renderer.window.KeyReleased += (_, key) => {
                if(this.hotkey != null && this.hotkey.IsPressed(key)) _hotkeyPressed = false;
            };
        }

        State DrawWithState() {
            Renderer.instance.DrawText(position, text.Substring(0, Math.Min(text.Length, width)), ColorScheme.white, Color.Transparent, align);
            _posX = position.x - align switch
            {
                Renderer.Alignment.Right => text.Length - 1,
                Renderer.Alignment.Center => (int)MathF.Ceiling(text.Length / 2f),
                _ => 0
            };
            return Renderer.instance.mousePosition.InBounds(_posX, position.y, _posX + width - 1, position.y) || _prevFrameHotkeyPressed
                              ? Core.renderer.leftButtonPressed || _hotkeyPressed ? State.Clicked : State.Hovered
                               : selected ? State.Selected : State.Idle;
        }
        public bool Draw() {
            prevFrameState = currentState;
            currentState = DrawWithState();
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

            for(int x = 0; x < width; x++) {
                Vector2 pos = new Vector2(_posX + x, position.y);
                Renderer.instance.SetCellColor(pos,
                    Renderer.AnimateColor(_animTimes[x], _currentColor, currentState == State.Idle ? hoverColor : idleColor, 4f + _animRateOffsets[x]),
                    Renderer.AnimateColor(_animTimes[x], _prevColor, _currentColor, 4f + _animRateOffsets[x]));
                _animTimes[x] += Core.deltaTime;
            }

            _prevFrameHotkeyPressed = Renderer.instance.window.HasFocus() && _hotkeyPressed;

            if(!Renderer.instance.window.HasFocus() || currentState != State.Hovered ||
               prevFrameState != State.Clicked) return false;
            Game.buttonClickSound.Play();
            return true;
        }
    }
}
