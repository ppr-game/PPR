using System;
using System.Collections.Generic;

using PPR.Main;

using PRR;

using SFML.Graphics;
using SFML.System;

using Alignment = PRR.Renderer.Alignment;

namespace PPR.GUI.Elements {
    public class Text : UIElement {
        public override string type => "text";

        public string text {
            get => _text;
            set {
                _text = value;
                lines = value.Replace("\n\r", "\n").Replace("\r\n", "\n").Replace("\r", "\n").Split("\n");
            }
        }

        public string[] lines { get; private set; }
        public Alignment align { get; set; }
        public bool replacingSpaces { get; set; }
        public bool invertOnDarkBackground { get; set; }
        private Color foregroundColor => GetColor("fg");
        private Color backgroundColor => GetColor("bg");
        private string _text;

        public Text(string id, List<string> tags, Vector2i? position, Vector2f? anchor, UIElement parent, string text,
            Alignment align, bool replacingSpaces, bool invertOnDarkBackground) : base(id, tags, position,
            new Vector2i(), anchor, parent) {
            this.text = text;
            this.align = align;
            this.replacingSpaces = replacingSpaces;
            this.invertOnDarkBackground = invertOnDarkBackground;
        }

        public override void Draw() {
            base.Draw();

            if(mask == null)
                Core.renderer.DrawLines(globalPosition, lines, foregroundColor, backgroundColor, align, replacingSpaces,
                    invertOnDarkBackground, animationModifier);
            else {
                Vector2i pos = globalPosition;
                Bounds maskBounds = mask.bounds;
                int minY = Math.Max(0, maskBounds.min.Y - pos.Y);
                int maxY = Math.Min(lines.Length, maskBounds.max.Y - pos.Y);
                for(int y = minY; y < maxY; ++y) {
                    string line = lines[y];
                    int minX = Math.Max(0, maskBounds.min.X - pos.X);
                    int maxX = Math.Min(line.Length, maskBounds.max.X - pos.X - minX);
                    Core.renderer.DrawText(pos + new Vector2i(minX, y), line.Substring(minX, maxX),
                        foregroundColor, backgroundColor, align, replacingSpaces, invertOnDarkBackground,
                        animationModifier);
                }
            }
        }
    }
}
