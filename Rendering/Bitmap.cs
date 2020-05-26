using System.Collections.Generic;

using PPR.Main;

using SFML.Graphics;
using SFML.System;

namespace PPR.Rendering {
    public class BitmapFont {
        public Texture texture;
        public Vector2 characterSize;
        public readonly Dictionary<char, Vector2f[]> characters = new Dictionary<char, Vector2f[]>();
        public BitmapFont(Image fontImage, string mappings, Vector2 characterSize) {
            this.characterSize = characterSize;
            int index = 0;
            for(uint y = 0; y < fontImage.Size.Y; y++) {
                for(uint x = 0; x < fontImage.Size.X; x++) {
                    if(fontImage.GetPixel(x, y) == Color.Black) fontImage.SetPixel(x, y, Color.Transparent);
                    else fontImage.SetPixel(x, y, Color.White);
                }
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
        readonly RenderTexture _renderTexture;
        readonly VertexArray backgroundQuads;
        readonly VertexArray foregroundQuads;
        public RenderTexture renderTexture {
            get {
                _renderTexture.Clear();

                backgroundQuads.Clear();
                foreach(KeyValuePair<Vector2, Color> curColor in backgroundColors) {
                    int xChar = curColor.Key.x * charWidth;
                    int yChar = curColor.Key.y * charHeight;
                    Vector2f position = new Vector2f(xChar, yChar);
                    Color color = curColor.Value;

                    backgroundQuads.Append(new Vertex(position, color)); // top left
                    backgroundQuads.Append(new Vertex(position + new Vector2f(charWidth, 0f), color)); // top right
                    backgroundQuads.Append(new Vertex(position + new Vector2f(charWidth, charHeight), color)); // bottom right
                    backgroundQuads.Append(new Vertex(position + new Vector2f(0f, charHeight), color)); // bottom left
                }
                _renderTexture.Draw(backgroundQuads);

                foregroundQuads.Clear();
                foreach(KeyValuePair<Vector2, char> curChar in text) {
                    int xChar = curChar.Key.x * charWidth;
                    int yChar = curChar.Key.y * charHeight;
                    if(font.characters.TryGetValue(curChar.Value, out Vector2f[] texCoords)) {
                        Vector2f position = new Vector2f(xChar, yChar);
                        Color foregroundColor = foregroundColors.TryGetValue(curChar.Key, out Color color) ? color : Color.White;

                        foregroundQuads.Append(new Vertex(position, foregroundColor, texCoords[0])); // top left
                        foregroundQuads.Append(new Vertex(position + new Vector2f(charWidth, 0f), foregroundColor, texCoords[1])); // top right
                        foregroundQuads.Append(new Vertex(position + new Vector2f(charWidth, charHeight), foregroundColor, texCoords[2])); // bottom right
                        foregroundQuads.Append(new Vertex(position + new Vector2f(0f, charHeight), foregroundColor, texCoords[3])); // bottom left
                    }
                }
                _renderTexture.Draw(foregroundQuads, new RenderStates(font.texture));

                _renderTexture.Display();
                return _renderTexture;
            }
        }

        readonly byte charWidth = 0;
        readonly byte charHeight = 0;
        readonly uint imageWidth = 0;
        readonly uint imageHeight = 0;
        readonly uint textWidth = 0;
        readonly uint textHeight = 0;
        public BitmapFont font;
        public BitmapText(BitmapFont font, Vector2 size) {
            this.font = font;
            charWidth = (byte)font.characterSize.x;
            charHeight = (byte)font.characterSize.y;
            textWidth = (uint)size.x;
            textHeight = (uint)size.y;
            imageWidth = (textWidth + 1) * charWidth;
            imageHeight = (textHeight + 1) * charHeight;
            _renderTexture = new RenderTexture(imageWidth, imageHeight);
            backgroundQuads = new VertexArray(PrimitiveType.Quads, 4u * imageWidth * imageHeight);
            foregroundQuads = new VertexArray(PrimitiveType.Quads, 4u * imageWidth * imageHeight);
        }
    }
}
