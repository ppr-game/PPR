using System.Collections.Generic;

using PER.Abstractions.Renderer;
using PER.Util;

using SFML.Graphics;
using SFML.System;

using Color = SFML.Graphics.Color;

namespace PRR {
    public class Text {
        public Dictionary<Vector2Int, RenderCharacter> text;
        public readonly Font font;
        private readonly uint _charWidth;
        private readonly uint _charHeight;
        public readonly uint imageWidth;
        public readonly uint imageHeight;
        private readonly Vertex[] _quads;

        public Text(Font font, Vector2Int size) {
            this.font = font;
            _charWidth = (uint)font.characterSize.x;
            _charHeight = (uint)font.characterSize.y;
            uint textWidth = (uint)size.x;
            uint textHeight = (uint)size.y;
            imageWidth = textWidth * _charWidth;
            imageHeight = textHeight * _charHeight;
            _quads = new Vertex[8 * textWidth * textHeight];
        }

        public void RebuildQuads(Vector2f offset) {
            Vector2f[] backgroundCharacter = font.backgroundCharacter;
            
            uint index = 0;
            foreach((Vector2Int pos, RenderCharacter character) in text) {
                Vector2f position = new(pos.x * _charWidth + offset.X, pos.y * _charHeight + offset.Y);

                _quads[index].Position = position;
                _quads[index + 1].Position = position + new Vector2f(_charWidth, 0f);
                _quads[index + 2].Position = position + new Vector2f(_charWidth, _charHeight);
                _quads[index + 3].Position = position + new Vector2f(0f, _charHeight);

                _quads[index].TexCoords = backgroundCharacter[0];
                _quads[index + 1].TexCoords = backgroundCharacter[1];
                _quads[index + 2].TexCoords = backgroundCharacter[2];
                _quads[index + 3].TexCoords = backgroundCharacter[3];

                Color background = SfmlConverters.ToSfmlColor(character.background);
                _quads[index].Color = background;
                _quads[index + 1].Color = background;
                _quads[index + 2].Color = background;
                _quads[index + 3].Color = background;

                if(font.characters.TryGetValue(character.character, out Vector2f[] texCoords)) {
                    _quads[index + 4].Position = _quads[index].Position;
                    _quads[index + 5].Position = _quads[index + 1].Position;
                    _quads[index + 6].Position = _quads[index + 2].Position;
                    _quads[index + 7].Position = _quads[index + 3].Position;

                    _quads[index + 4].TexCoords = texCoords[0];
                    _quads[index + 5].TexCoords = texCoords[1];
                    _quads[index + 6].TexCoords = texCoords[2];
                    _quads[index + 7].TexCoords = texCoords[3];

                    Color foreground = SfmlConverters.ToSfmlColor(character.foreground);
                    _quads[index + 4].Color = foreground;
                    _quads[index + 5].Color = foreground;
                    _quads[index + 6].Color = foreground;
                    _quads[index + 7].Color = foreground;
                }
                else {
                    _quads[index + 4].TexCoords = new Vector2f();
                    _quads[index + 5].TexCoords = new Vector2f();
                    _quads[index + 6].TexCoords = new Vector2f();
                    _quads[index + 7].TexCoords = new Vector2f();
                }

                index += 8;
            }
        }

        public void DrawQuads(RenderTarget target) => target.Draw(_quads, 0, (uint)(text.Count * 8),
            PrimitiveType.Quads,
            new RenderStates(font.texture));
    }
}
