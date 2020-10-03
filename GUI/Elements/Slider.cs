using System;

using PPR.Main;

using PRR;

using SFML.Graphics;
using SFML.System;

namespace PPR.GUI.Elements {
    // Yes. Yes I just copy-pasted the code from the Button class and modified it a bit. Sorry
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    // ReSharper disable MemberCanBePrivate.Global
    public class Slider {
        public Vector2i position;
        public readonly int minValue;
        public readonly int maxValue;
        public readonly int size;
        public readonly int step;
        public int value;
        public string leftText;
        public string rightText;
        public string id;
        public Color idleColor;
        public Color hoverColor;
        public Color clickColor;
        public Renderer.Alignment align;
        public bool swapTexts;
        Color _currentColor;
        Color _prevColor;
        public State currentState { get; private set; } = State.Clicked;
        State _prevState = State.Hovered;
        // ReSharper disable once NotAccessedField.Global
        public State prevFrameState { get; private set; } = State.Hovered;
        readonly float[] _animTimes;
        readonly float[] _animRateOffsets;
        int _posX;
        public enum State { Idle, Hovered, Clicked };
        public Slider(Vector2i position, int minValue, int maxValue, int size, int defaultValue, string leftText,
            string rightText, string id, Renderer.Alignment align = Renderer.Alignment.Left, bool swapTexts = false) :
            this(position, minValue, maxValue, size, defaultValue, leftText, rightText, id,
            ColorScheme.GetColor($"slider_{id}_idle"),
            ColorScheme.GetColor($"slider_{id}_hover"),
            ColorScheme.GetColor($"slider_{id}_click"), align, swapTexts) { }
        public Slider(Vector2i position, int minValue, int maxValue, int size, int defaultValue, string leftText,
            string rightText, string id, Color idleColor, Color hoverColor, Color clickColor,
            Renderer.Alignment align = Renderer.Alignment.Left, bool swapTexts = false) {
            this.position = position;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.size = size;
            step = (maxValue - minValue) / (size - 1);
            value = defaultValue;
            this.leftText = leftText;
            this.rightText = rightText;
            this.id = id;
            this.idleColor = idleColor;
            this.hoverColor = hoverColor;
            this.clickColor = clickColor;
            this.align = align;
            this.swapTexts = swapTexts;
            _animTimes = new float[size];
            _animRateOffsets = new float[size];
            _currentColor = hoverColor;
        }
        State DrawWithState() {
            string leftText = $"{(swapTexts ? this.rightText : this.leftText).Replace("[value]", value.ToString())} ";
            string rightText = (swapTexts ? this.leftText : this.rightText).Replace("[value]", value.ToString());
            _posX = position.X - align switch
            {
                Renderer.Alignment.Right => size + rightText.Length + 1,
                Renderer.Alignment.Center => (int)MathF.Ceiling(size / 2f),
                _ => -leftText.Length
            };
            if(leftText != "") Core.renderer.DrawText(new Vector2i(_posX - leftText.Length, position.Y), leftText, hoverColor, idleColor);
            if(rightText != "") Core.renderer.DrawText(new Vector2i(_posX + size + 1, position.Y), rightText, hoverColor, idleColor);
            return Core.renderer.mousePosition.InBounds(_posX, position.Y, _posX + size - 1, position.Y)
                              ? Core.renderer.leftButtonPressed ? State.Clicked : State.Hovered : State.Idle;
        }
        public bool Draw() {
            prevFrameState = currentState;
            currentState = DrawWithState();
            if(_prevState != currentState) {
                Color color = currentState switch {
                    State.Hovered => hoverColor,
                    State.Clicked => clickColor,
                    _ => idleColor
                };
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

            bool valueChanged = false;
            if(Core.renderer.window.HasFocus() && currentState == State.Clicked) {
                int previousValue = value;
                value = Math.Clamp((Core.renderer.mousePosition.X - _posX) * step + minValue, minValue, maxValue);
                valueChanged = value != previousValue;
                if(valueChanged) Game.sliderSound.Play();
            }

            for(int x = 0; x < size; x++) {
                Vector2i pos = new Vector2i(_posX + x, position.Y);
                int drawValue = (value - minValue) / step;
                char curChar = '█';
                if(x < drawValue) curChar = '─';
                else if(x > drawValue) curChar = '-';
                Core.renderer.SetCharacter(pos, new RenderCharacter(curChar,
                    Renderer.AnimateColor(_animTimes[x], _prevColor, _currentColor, 4f + _animRateOffsets[x]),
                    Renderer.AnimateColor(_animTimes[x], _currentColor,
                        currentState == State.Idle ? hoverColor : idleColor, 4f + _animRateOffsets[x])));
                _animTimes[x] += Core.deltaTime;
            }
            return valueChanged;
        }
    }
}
