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
            get => new Vector2i(width, 0);
            set => throw new InvalidOperationException("Tried to change the size of a slider.");
        }
        public int step { get; }

        public int value {
            get => _value;
            set {
                if(_value == value) return;
                Lua.InvokeEvent(this, "sliderValueChange", this, DynValue.NewNumber(value),
                    DynValue.NewNumber(_value));
                _value = value;
                SoundManager.PlaySound(SoundType.Slider);
            }
        }
        
        public string leftText { get; set; }
        public string rightText { get; set; }
        private Color idleColor => GetColor("idle");
        private Color hoverColor => GetColor("hover");
        private Color clickColor => GetColor("click");
        public Alignment align { get; set; }
        public bool swapTexts { get; set; }
        public State currentState { get; private set; } = State.Clicked;

        private int _value;
        private DateTime _animStartTime;
        private float _animTime;
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
            _animRateOffsets = new float[width];
            _currentColor = hoverColor;
        }
        private State DrawBase(Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> modifier) {
            string leftText = $"{(swapTexts ? this.rightText : this.leftText).Replace("[value]", value.ToString())} ";
            string rightText = (swapTexts ? this.leftText : this.rightText).Replace("[value]", value.ToString());
            Vector2i globalPos = globalPosition;
            _posX = globalPos.X - align switch {
                Alignment.Right => width + rightText.Length + 1,
                Alignment.Center => (int)MathF.Ceiling(width / 2f),
                _ => -leftText.Length
            };
            if(leftText != "") {
                if(mask == null) {
                    Core.renderer.DrawText(new Vector2i(_posX - leftText.Length, globalPos.Y), leftText, hoverColor,
                        idleColor, Alignment.Left, false, false, modifier);
                }
                else {
                    Bounds maskBounds = mask.bounds;
                    Vector2i textPos = new Vector2i(_posX - leftText.Length, globalPos.Y);
                    if(textPos.Y >= maskBounds.min.Y && textPos.Y <= maskBounds.max.Y) {
                        int minX = Math.Max(0, maskBounds.min.X - textPos.X);
                        int maxX = Math.Min(leftText.Length, maskBounds.max.X - textPos.X - minX);
                        Core.renderer.DrawText(textPos + new Vector2i(minX, 0), leftText.Substring(minX, maxX),
                            hoverColor, idleColor, Alignment.Left, false, false, modifier);
                    }
                }
            }
            if(rightText != "") {
                if(mask == null) {
                    Core.renderer.DrawText(new Vector2i(_posX + width + 1, globalPos.Y), rightText, hoverColor,
                        idleColor, Alignment.Left, false, false, modifier);
                }
                else {
                    Bounds maskBounds = mask.bounds;
                    Vector2i textPos = new Vector2i(_posX + width + 1, globalPos.Y);
                    if(textPos.Y >= maskBounds.min.Y && textPos.Y <= maskBounds.max.Y) {
                        int minX = Math.Max(0, maskBounds.min.X - textPos.X);
                        int maxX = Math.Min(rightText.Length, maskBounds.max.X - textPos.X - minX);
                        Core.renderer.DrawText(textPos + new Vector2i(minX, 0), rightText.Substring(minX, maxX),
                            hoverColor, idleColor, Alignment.Left, false, false, modifier);
                    }
                }
            }

            bool onSlider;
            bool wasOnSlider;
            if(mask == null) {
                onSlider =
                    Core.renderer.mousePosition.InBounds(_posX, globalPos.Y, _posX + width - 1, globalPos.Y);
                wasOnSlider = UI.LineSegmentIntersection(UI.prevMousePosition, Core.renderer.mousePosition,
                    new Vector2i(_posX, globalPos.Y), new Vector2i(_posX + width - 1, globalPos.Y));
            }
            else {
                Bounds maskBounds = mask.bounds;
                Vector2i minBound = new Vector2i(Math.Max(_posX, maskBounds.min.X),
                    Math.Max(globalPos.Y, maskBounds.min.Y));
                Vector2i maxBound = new Vector2i(Math.Min(_posX + width - 1, maskBounds.max.X),
                    Math.Min(globalPos.Y, maskBounds.max.Y));
                onSlider = Core.renderer.mousePosition.InBounds(minBound, maxBound);
                wasOnSlider = UI.LineSegmentIntersection(UI.prevMousePosition, Core.renderer.mousePosition,
                    minBound, maxBound);
            }
            return wasOnSlider ? Core.renderer.leftButtonPressed && onSlider ? State.Clicked :
                State.Hovered : State.Idle;
        }
        public override void Draw() {
            base.Draw();

            Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> useAnimationModifier = ApplyAnimations;
            
            currentState = DrawBase(useAnimationModifier);

            UpdateState();
            _animTime = (float)(DateTime.UtcNow - _animStartTime).TotalSeconds;

            if(Core.renderer.window.HasFocus() && currentState == State.Clicked)
                value = Math.Clamp((Core.renderer.mousePosition.X - _posX) * step + minValue, minValue, maxValue);

            for(int x = 0; x < width; x++) {
                Vector2i pos = new Vector2i(_posX + x, globalPosition.Y);
                if(mask != null) {
                    Bounds maskBounds = mask.bounds;
                    if(pos.X < maskBounds.min.X || pos.X > maskBounds.max.X ||
                       pos.Y < maskBounds.min.Y || pos.Y > maskBounds.max.Y) break;
                }
                int drawValue = (value - minValue) / step;
                char curChar = '█';
                if(x < drawValue) curChar = '─';
                else if(x > drawValue) curChar = '-';
                Color background =
                    Renderer.AnimateColor(_animTime, _prevColor, _currentColor, 4f + _animRateOffsets[x]);
                Color foreground = Renderer.AnimateColor(_animTime, _currentColor,
                    currentState == State.Idle ? hoverColor : idleColor, 4f + _animRateOffsets[x]);
                if(useAnimationModifier == null)
                    Core.renderer.SetCharacter(pos, new RenderCharacter(curChar, background, foreground));
                else {
                    (Vector2i newPos, RenderCharacter newCharacter) =
                        useAnimationModifier(pos, new RenderCharacter(curChar, background, foreground));
                    Core.renderer.SetCharacter(newPos, newCharacter);
                }
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
                    for(int x = 0; x < width; x++) _animRateOffsets[x] = new Random().NextFloat(-1f, 1f);
                    _animStartTime = DateTime.UtcNow;
                }

                _currentColor = color;
            }

            _prevState = currentState;
        }
    }
}
