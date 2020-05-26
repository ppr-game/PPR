using System;

using PPR.Main;
using PPR.Rendering;

using SFML.Graphics;

namespace PPR.GUI.Elements {
    public class Button {
        public Vector2 position;
        public string text;
        int _width;
        public int width {
            get => _width;
            set {
                _width = value;
                animTimes = new float[value];
                animRateOffsets = new float[value];
            }
        }
        public Color idleColor;
        public Color hoverColor;
        public Color clickColor;
        public Renderer.TextAlignment align;
        Color currentColor;
        Color prevColor;
        public State currentState = State.Clicked;
        State prevState = State.Hovered;
        public State prevFrameState = State.Hovered;
        float[] animTimes;
        float[] animRateOffsets;
        public bool selected = false;
        int posX;
        public enum State { Idle, Hovered, Clicked, Selected };
        public Button(Vector2 position, string text, int width, Color idleColor, Color hoverColor, Color clickColor, Renderer.TextAlignment align = Renderer.TextAlignment.Left) {
            this.position = position;
            this.text = text;
            this.width = width;
            this.idleColor = idleColor;
            this.hoverColor = hoverColor;
            this.clickColor = clickColor;
            this.align = align;
            animTimes = new float[width];
            animRateOffsets = new float[width];
            currentColor = hoverColor;
        }

        State DrawWithState() {
            Renderer.instance.DrawText(position, text.Substring(0, Math.Min(text.Length, width)), Color.White, Color.Transparent, align);
            posX = position.x - align switch
            {
                Renderer.TextAlignment.Right => text.Length - 1,
                Renderer.TextAlignment.Center => (int)MathF.Ceiling(text.Length / 2f),
                _ => 0
            };
            return Renderer.instance.mousePosition.InBounds(posX, position.y, posX + width - 1, position.y)
                              ? Core.renderer.leftButtonPressed ? State.Clicked : State.Hovered
                               : selected ? State.Selected : State.Idle;
        }
        public bool Draw() {
            prevFrameState = currentState;
            currentState = DrawWithState();
            if(prevState != currentState) {
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
                if(currentColor != color) {
                    prevColor = currentColor;
                    for(int x = 0; x < width; x++) {
                        animTimes[x] = 0f;
                        animRateOffsets[x] = new Random().NextFloat(-1f, 1f);
                    }
                }
                currentColor = color;
            }
            prevState = currentState;

            for(int x = 0; x < width; x++) {
                Vector2 pos = new Vector2(posX + x, position.y);
                Renderer.instance.SetCellColor(pos,
                                                                    Renderer.AnimateColor(animTimes[x], currentColor, currentState == State.Idle ? hoverColor : idleColor, 4f + animRateOffsets[x]),
                                                                    Renderer.AnimateColor(animTimes[x], prevColor, currentColor, 4f + animRateOffsets[x]));
                animTimes[x] += Core.deltaTime;
            }
            return Renderer.instance.window.HasFocus() && currentState == State.Clicked && prevFrameState != State.Clicked;
        }
    }
}
