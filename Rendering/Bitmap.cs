using System.Collections.Generic;

using PPR.Main;

using SFML.Graphics;
using SFML.System;

namespace PPR.Rendering {
    public class BitmapFont {
        public readonly Texture texture;
        public readonly Vector2 characterSize;
        public readonly Dictionary<char, Vector2f[]> characters = new Dictionary<char, Vector2f[]>();
        public BitmapFont(Image fontImage, string mappings, Vector2 characterSize) {
            this.characterSize = characterSize;
            int index = 0;
            for(uint y = 0; y < fontImage.Size.Y; y++) {
                for(uint x = 0; x < fontImage.Size.X; x++)
                    fontImage.SetPixel(x, y, fontImage.GetPixel(x, y) == Color.Black ? Color.Transparent : Color.White);
            }
            texture = new Texture(fontImage);
            for(int y = 0; y < fontImage.Size.Y; y += characterSize.y) {
                for(int x = 0; x < fontImage.Size.X; x += characterSize.x) {
                    if(mappings.Length <= index + 1) break;
                    Vector2f[] texCoords = new Vector2f[4];
                    // Clockwise
                    texCoords[0] = new Vector2f(x, y); // top left
                    texCoords[1] = new Vector2f(x + characterSize.x, y); // top right
                    texCoords[2] = new Vector2f(x + characterSize.x, y + characterSize.y); // bottom right
                    texCoords[3] = new Vector2f(x, y + characterSize.y); // bottom left
                    characters.Add(mappings[index++], texCoords);
                }
            }
        }
    }
    public class BitmapText {
        public Dictionary<Vector2, Color> backgroundColors;
        public Dictionary<Vector2, Color> foregroundColors;
        public Dictionary<Vector2, char> text;
        readonly Vertex[] _backgroundQuads;
        readonly Vertex[] _foregroundQuads;
        public RenderTexture renderTexture { get; }
        public void RebuildRenderTexture() {
            renderTexture.Clear();

            uint index = 0;
            foreach((Vector2 key, Color color) in backgroundColors) {
                int xChar = key.x * _charWidth;
                int yChar = key.y * _charHeight;
                Vector2f position = new Vector2f(xChar, yChar);

                _backgroundQuads[index].Position = position;
                _backgroundQuads[index + 1].Position = position + new Vector2f(_charWidth, 0f);
                _backgroundQuads[index + 2].Position = position + new Vector2f(_charWidth, _charHeight);
                _backgroundQuads[index + 3].Position = position + new Vector2f(0f, _charHeight);
                
                _backgroundQuads[index].Color = color;
                _backgroundQuads[index + 1].Color = color;
                _backgroundQuads[index + 2].Color = color;
                _backgroundQuads[index + 3].Color = color;

                index += 4;
            }
            renderTexture.Draw(_backgroundQuads, 0, (uint)(backgroundColors.Count * 4), PrimitiveType.Quads);

            index = 0;
            foreach((Vector2 key, char value) in text) {
                int xChar = key.x * _charWidth;
                int yChar = key.y * _charHeight;
                if(!_font.characters.TryGetValue(value, out Vector2f[] texCoords)) continue;
                Vector2f position = new Vector2f(xChar, yChar);
                Color foregroundColor = foregroundColors.TryGetValue(key, out Color color) ? color : Color.White;

                _foregroundQuads[index].Position = position;
                _foregroundQuads[index + 1].Position = position + new Vector2f(_charWidth, 0f);
                _foregroundQuads[index + 2].Position = position + new Vector2f(_charWidth, _charHeight);
                _foregroundQuads[index + 3].Position = position + new Vector2f(0f, _charHeight);
                
                _foregroundQuads[index].TexCoords = texCoords[0];
                _foregroundQuads[index + 1].TexCoords = texCoords[1];
                _foregroundQuads[index + 2].TexCoords = texCoords[2];
                _foregroundQuads[index + 3].TexCoords = texCoords[3];

                _foregroundQuads[index].Color = foregroundColor;
                _foregroundQuads[index + 1].Color = foregroundColor;
                _foregroundQuads[index + 2].Color = foregroundColor;
                _foregroundQuads[index + 3].Color = foregroundColor;

                index += 4;
            }
            renderTexture.Draw(_foregroundQuads, 0, (uint)(text.Count * 4), PrimitiveType.Quads,
                new RenderStates(_font.texture));

            renderTexture.Display();
        }

        readonly byte _charWidth;
        readonly byte _charHeight;
        public readonly uint imageWidth;
        public readonly uint imageHeight; // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        readonly uint _textWidth; // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        readonly uint _textHeight;
        readonly BitmapFont _font;
        public BitmapText(BitmapFont font, Vector2 size) {
            _font = font;
            _charWidth = (byte)font.characterSize.x;
            _charHeight = (byte)font.characterSize.y;
            _textWidth = (uint)size.x;
            _textHeight = (uint)size.y;
            imageWidth = _textWidth * _charWidth;
            imageHeight = _textHeight * _charHeight;
            renderTexture = new RenderTexture(imageWidth, imageHeight);
            _backgroundQuads = new Vertex[4 * _textWidth * _textHeight];
            _foregroundQuads = new Vertex[4 * _textWidth * _textHeight];
        }
    }
}
