using System;
using System.Collections.Generic;

using MoonSharp.Interpreter;

using PPR.Main;
using PPR.Main.Managers;

using PRR;

using SFML.Graphics;
using SFML.System;

using Alignment = PRR.Renderer.Alignment;

namespace PPR.GUI.Elements {
    public class Slider : UIElement {
        public enum State { Idle, Hovered, Clicked }

        public int minValue { get; }
        public int maxValue { get; }
        public int width { get; }
        public override Vector2i size {
            get => new Vector2i(width, 1);
            set => throw new InvalidOperationException("Tried to change the size of a slider.");
        }
        public int step { get; }

        public int value {
            get => _value;
            set {
                _value = value;
                _onValueChangeArgs[1] = DynValue.NewNumber(value);
            }
        }
        
        public string leftText { get; set; }
        public string rightText { get; set; }
        public List<Closure> onValueChange { get; set; }
        private Color idleColor => GetColor("idle");
        private Color hoverColor => GetColor("hover");
        private Color clickColor => GetColor("click");
        public Alignment align { get; set; }
        public bool swapTexts { get; set; }
        public State currentState { get; private set; } = State.Clicked;

        private readonly DynValue[] _onValueChangeArgs;
        private int _value;
        private readonly float[] _animTimes;
        private readonly float[] _animRateOffsets;
        private State _prevState = State.Hovered;
        private Color _currentColor;
        private Color _prevColor;
        private int _posX;

        public Slider(string id, List<string> tags, Vector2i? position, int width, Vector2f? anchor, UIElement parent,
            int minValue, int maxValue, int defaultValue, string leftText, string rightText,
            Alignment align = Alignment.Left, bool swapTexts = false) :
            base(id, tags, position, new Vector2i(width, 1), anchor, parent) {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.width = width;
            step = (maxValue - minValue) / (width - 1);
            value = defaultValue;
            this.leftText = leftText;
            this.rightText = rightText;
            this.align = align;
            this.swapTexts = swapTexts;
            _animTimes = new float[width];
            _animRateOffsets = new float[width];
            _currentColor = hoverColor;
            _onValueChangeArgs = new DynValue[] { DynValue.NewString(id), DynValue.NewNumber(0) };
        }
        private State DrawBase() {
            string leftText = $"{(swapTexts ? this.rightText : this.leftText).Replace("[value]", value.ToString())} ";
            string rightText = (swapTexts ? this.leftText : this.rightText).Replace("[value]", value.ToString());
            _posX = globalPosition.X - align switch {
                Alignment.Right => width + rightText.Length + 1,
                Alignment.Center => (int)MathF.Ceiling(width / 2f),
                _ => -leftText.Length
            };
            if(leftText != "")
                Core.renderer.DrawText(new Vector2i(_posX - leftText.Length, globalPosition.Y), leftText, hoverColor,
                    idleColor, Alignment.Left, false, false, animationModifier);
            if(rightText != "")
                Core.renderer.DrawText(new Vector2i(_posX + width + 1, globalPosition.Y), rightText, hoverColor,
                    idleColor, Alignment.Left, false, false, animationModifier);

            bool onSlider = Core.renderer.mousePosition.InBounds(_posX, globalPosition.Y, _posX + width - 1, globalPosition.Y);
            bool wasOnSlider = UI.LineSegmentIntersection(UI.prevMousePosition, Core.renderer.mousePosition,
                new Vector2i(_posX, globalPosition.Y), new Vector2i(_posX + width - 1, globalPosition.Y));
            return wasOnSlider ? Core.renderer.leftButtonPressed && onSlider ? State.Clicked :
                State.Hovered : State.Idle;
        }
        public override void Draw() {
            base.Draw();
            
            currentState = DrawBase();

            UpdateState();

            if(Core.renderer.window.HasFocus() && currentState == State.Clicked) {
                int previousValue = value;
                value = Math.Clamp((Core.renderer.mousePosition.X - _posX) * step + minValue, minValue, maxValue);
                bool valueChanged = value != previousValue;
                if(valueChanged) {
                    SoundManager.PlaySound(SoundType.Slider); // ReSharper disable once HeapView.ObjectAllocation
                    foreach(Closure closure in onValueChange)
                        closure?.Call(_onValueChangeArgs[0], DynValue.NewNumber(previousValue), _onValueChangeArgs[1]);
                }
            }

            // TODO: transition implementation
            for(int x = 0; x < width; x++) {
                Vector2i pos = new Vector2i(_posX + x, globalPosition.Y);
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
        }

        private void UpdateState() {
            if(_prevState != currentState) {
                Color color = currentState switch {
                    State.Hovered => hoverColor,
                    State.Clicked => clickColor,
                    _ => idleColor
                };
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
