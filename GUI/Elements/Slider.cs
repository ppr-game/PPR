using System;

using PPR.Main;
using PPR.Rendering;

using SFML.Graphics;

namespace PPR.GUI.Elements {
    // Yes. Yes I just copy-pasted the code from the Button class and modified it a bit. Sorry
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    // ReSharper disable MemberCanBePrivate.Global
    public class Slider {
        public Vector2 position;
        public readonly int minValue;
        public readonly int maxValue;
        public readonly int size;
        public readonly int step;
        public int value;
        public string text;
        public Color idleColor;
        public Color hoverColor;
        public Color clickColor;
        public bool showValue;
        public Renderer.Alignment align;
        public TextAlignment alignText;
        Color _currentColor;
        Color _prevColor;
        public State currentState = State.Clicked;
        State _prevState = State.Hovered;
        // ReSharper disable once NotAccessedField.Global
        public State prevFrameState = State.Hovered;
        readonly float[] _animTimes;
        readonly float[] _animRateOffsets;
        int _posX;
        public enum State { Idle, Hovered, Clicked };
        public enum TextAlignment { Left, Right };
        public Slider(Vector2 position, int minValue, int maxValue, int size, int defaultValue, string text, Color idleColor, Color hoverColor, Color clickColor, bool showValue = true, Renderer.Alignment align = Renderer.Alignment.Left, TextAlignment alignText = TextAlignment.Left) {
            this.position = position;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.size = size;
            step = (maxValue - minValue) / (size - 1);
            value = defaultValue;
            this.text = text;
            this.idleColor = idleColor;
            this.hoverColor = hoverColor;
            this.clickColor = clickColor;
            this.showValue = showValue;
            this.align = align;
            this.alignText = alignText;
            _animTimes = new float[size];
            _animRateOffsets = new float[size];
            _currentColor = hoverColor;
        }
        State DrawWithState() {
            bool left = alignText == TextAlignment.Left;
            string leftText = $"{(left ? text : showValue ? value.ToString() : "")} ";
            string rightText = left ? showValue ? value.ToString() : "" : text;
            _posX = position.x - align switch
            {
                Renderer.Alignment.Right => size + rightText.Length + 1,
                Renderer.Alignment.Center => (int)MathF.Ceiling(size / 2f),
                _ => -leftText.Length
            };
            if(leftText != "") Renderer.instance.DrawText(new Vector2(_posX - leftText.Length, position.y), leftText, hoverColor, idleColor);
            if(rightText != "") Renderer.instance.DrawText(new Vector2(_posX + size + 1, position.y), rightText, hoverColor, idleColor);
            return Renderer.instance.mousePosition.InBounds(_posX, position.y, _posX + size - 1, position.y)
                              ? Core.renderer.leftButtonPressed ? State.Clicked : State.Hovered : State.Idle;
        }
        public int Draw() {
            prevFrameState = currentState;
            currentState = DrawWithState();
            if(_prevState != currentState) {
                Color color = idleColor;
                switch(currentState) {
                    case State.Hovered:
                        color = hoverColor;
                        break;
                    case State.Clicked:
                        color = clickColor;
                        break;
                }
                if(_currentColor != color) {
                    _prevColor = _currentColor;
                    for(int x = 0; x < size; x++) {
                        _animTimes[x] = 0f;
                        _animRateOffsets[x] = new Random().NextFloat(-1f, 1f);
                    }
                }
                _currentColor = color;
            }
            _prevState = currentState;

            if(Renderer.instance.window.HasFocus() && currentState == State.Clicked) {
                int previousValue = value;
                value = Math.Clamp((Renderer.instance.mousePosition.x - _posX) * step + minValue, minValue, maxValue);
                if (value != previousValue) Game.sliderSound.Play();
            }

            for(int x = 0; x < size; x++) {
                Vector2 pos = new Vector2(_posX + x, position.y);
                int drawValue = (value - minValue) / step;
                char curChar = '█';
                if(x < drawValue) curChar = '─';
                else if(x > drawValue) curChar = '-';
                Renderer.instance.SetCharacter(pos, curChar,
                                                                            Renderer.AnimateColor(_animTimes[x], _currentColor, currentState == State.Idle ? hoverColor : idleColor, 4f + _animRateOffsets[x]),
                                                                            Renderer.AnimateColor(_animTimes[x], _prevColor, _currentColor, 4f + _animRateOffsets[x]));
                _animTimes[x] += Core.deltaTime;
            }
            return value;
        }
    }
}
