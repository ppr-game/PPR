using System;
using System.Collections.Generic;

using PER.Util;

using SFML.System;

using Alignment = PRR.Renderer.Alignment;
using Color = SFML.Graphics.Color;

namespace PPR.UI.Elements {
    public class Text : Element {
        public override string type => "text";

        public override Vector2i size {
            get => new Vector2i(_maxLineLength, lines.Length);
            set { }
        }

        public string text {
            get => _text;
            set {
                _text = value;
                lines = value.Replace("\n\r", "\n").Replace("\r\n", "\n").Replace("\r", "\n").Split("\n");
                _maxLineLength = 0;
                foreach(string line in lines)
                    if(line.Length > _maxLineLength)
                        _maxLineLength = line.Length;
            }
        }

        public string[] lines { get; private set; }
        public Alignment align { get; set; }
        public bool replacingSpaces { get; set; }
        public bool invertOnDarkBackground { get; set; }
        private Color foregroundColor => GetColor("fg");
        private Color backgroundColor => GetColor("bg");
        private string _text;
        private int _maxLineLength;

        public Text(string id, List<string> tags, Vector2i? position, Vector2f? anchor, Element parent, string text,
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
                Core.renderer.DrawText(globalPosition, lines, (PER.Util.Color)foregroundColor, (PER.Util.Color)backgroundColor, (HorizontalAlignment)align, replacingSpaces,
                    invertOnDarkBackground, ApplyAnimations);
            else {
                Vector2i pos = globalPosition;
                Bounds maskBounds = mask.bounds;
                int minY = Math.Max(0, maskBounds.min.Y - pos.Y);
                int maxY = Math.Min(lines.Length, maskBounds.max.Y - pos.Y);
                for(int y = minY; y < maxY; ++y) {
                    string line = lines[y];
                    int minX = Math.Max(0, maskBounds.min.X - pos.X);
                    int maxX = Math.Min(line.Length, maskBounds.max.X - pos.X - minX + 1);
                    Core.renderer.DrawText((Vector2Int)(pos + new Vector2i(minX, y)), (string[])line.Substring(minX, maxX),
                        (PER.Util.Color)foregroundColor, (PER.Util.Color)backgroundColor, (HorizontalAlignment)align, replacingSpaces, invertOnDarkBackground,
                        ApplyAnimations);
                }
            }
        }
    }
}
