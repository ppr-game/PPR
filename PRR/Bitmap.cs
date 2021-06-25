using System;
using System.Collections.Generic;

using PER.Abstractions.Renderer;
using PER.Util;

using SFML.Graphics;
using SFML.System;

using Color = SFML.Graphics.Color;

namespace PRR {
    public class BitmapFont {
        public readonly Dictionary<char, Vector2f[]> characters = new Dictionary<char, Vector2f[]>();
        public readonly Vector2i characterSize;
        public readonly Texture texture;

        public BitmapFont(Image fontImage, string mappings, Vector2i characterSize) {
            this.characterSize = characterSize;
            int index = 0;
            for(uint y = 0; y < fontImage.Size.Y; y++) {
                for(uint x = 0; x < fontImage.Size.X; x++)
                    fontImage.SetPixel(x, y, fontImage.GetPixel(x, y) == Color.Black ? Color.Transparent : Color.White);
            }

            texture = new Texture(fontImage);
            for(int y = 0; y < fontImage.Size.Y; y += characterSize.Y) {
                for(int x = 0; x < fontImage.Size.X; x += characterSize.X) {
                    if(mappings.Length <= index) break;
                    Vector2f[] texCoords = new Vector2f[4];
                    // Clockwise
                    texCoords[0] = new Vector2f(x, y); // top left
                    texCoords[1] = new Vector2f(x + characterSize.X, y); // top right
                    texCoords[2] = new Vector2f(x + characterSize.X, y + characterSize.Y); // bottom right
                    texCoords[3] = new Vector2f(x, y + characterSize.Y); // bottom left
                    characters.Add(mappings[index++], texCoords);
                }
            }
        }
    }

    public class BitmapText {
        private readonly byte _charHeight;

        private readonly byte _charWidth;
        private readonly BitmapFont _font;
        private readonly Vertex[] _quads;
        public readonly uint imageHeight;
        public readonly uint imageWidth;
        public Dictionary<Vector2Int, RenderCharacter> text;

        public BitmapText(BitmapFont font, Vector2i size) {
            _font = font;
            _charWidth = (byte)font.characterSize.X;
            _charHeight = (byte)font.characterSize.Y;
            uint textWidth = (uint)size.X;
            uint textHeight = (uint)size.Y;
            imageWidth = textWidth * _charWidth;
            imageHeight = textHeight * _charHeight;
            _quads = new Vertex[8 * textWidth * textHeight];
        }

        public void RebuildQuads(Vector2f offset,
            Func<Vector2i, RenderCharacter, (Vector2f position, RenderCharacter character)> charactersModifier = null) {
            bool backgroundChar = _font.characters.TryGetValue('█', out Vector2f[] bgTexCoords);

            uint index = 0;
            foreach((Vector2Int pos, RenderCharacter character) in text) {
                Vector2i sfmlPos = SfmlConverters.ToSfmlVector2Int(pos);
                (Vector2f modPos, RenderCharacter modChar) = ((Vector2f)sfmlPos, character);
                if(charactersModifier != null) (modPos, modChar) = charactersModifier.Invoke(sfmlPos, character);

                Vector2f position = new(modPos.X * _charWidth + offset.X, modPos.Y * _charHeight + offset.Y);

                _quads[index].Position = position;
                _quads[index + 1].Position = position + new Vector2f(_charWidth, 0f);
                _quads[index + 2].Position = position + new Vector2f(_charWidth, _charHeight);
                _quads[index + 3].Position = position + new Vector2f(0f, _charHeight);

                if(backgroundChar) {
                    _quads[index].TexCoords = bgTexCoords[0];
                    _quads[index + 1].TexCoords = bgTexCoords[1];
                    _quads[index + 2].TexCoords = bgTexCoords[2];
                    _quads[index + 3].TexCoords = bgTexCoords[3];

                    Color background = SfmlConverters.ToSfmlColor(modChar.background);
                    _quads[index].Color = background;
                    _quads[index + 1].Color = background;
                    _quads[index + 2].Color = background;
                    _quads[index + 3].Color = background;
                }

                if(_font.characters.TryGetValue(modChar.character, out Vector2f[] texCoords)) {
                    _quads[index + 4].Position = _quads[index].Position;
                    _quads[index + 5].Position = _quads[index + 1].Position;
                    _quads[index + 6].Position = _quads[index + 2].Position;
                    _quads[index + 7].Position = _quads[index + 3].Position;

                    _quads[index + 4].TexCoords = texCoords[0];
                    _quads[index + 5].TexCoords = texCoords[1];
                    _quads[index + 6].TexCoords = texCoords[2];
                    _quads[index + 7].TexCoords = texCoords[3];

                    Color foreground = SfmlConverters.ToSfmlColor(modChar.foreground);
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
            new RenderStates(_font.texture));
    }
}
