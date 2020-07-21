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
        readonly Vertex[] backgroundQuads;
        readonly Vertex[] foregroundQuads;
        public RenderTexture renderTexture { get; }
        public void RebuildRenderTexture() {
            renderTexture.Clear();

            uint index = 0;
            foreach(KeyValuePair<Vector2, Color> curColor in backgroundColors) {
                int xChar = curColor.Key.x * charWidth;
                int yChar = curColor.Key.y * charHeight;
                Vector2f position = new Vector2f(xChar, yChar);
                Color color = curColor.Value;

                backgroundQuads[index].Position = position; backgroundQuads[index].Color = color;
                backgroundQuads[index + 1].Position = position + new Vector2f(charWidth, 0f); backgroundQuads[index + 1].Color = color;
                backgroundQuads[index + 2].Position = position + new Vector2f(charWidth, charHeight); backgroundQuads[index + 2].Color = color;
                backgroundQuads[index + 3].Position = position + new Vector2f(0f, charHeight); backgroundQuads[index + 3].Color = color;

                index += 4;
            }
            renderTexture.Draw(backgroundQuads, 0, (uint)(backgroundColors.Count * 4), PrimitiveType.Quads);

            index = 0;
            foreach(KeyValuePair<Vector2, char> curChar in text) {
                int xChar = curChar.Key.x * charWidth;
                int yChar = curChar.Key.y * charHeight;
                if(font.characters.TryGetValue(curChar.Value, out Vector2f[] texCoords)) {
                    Vector2f position = new Vector2f(xChar, yChar);
                    Color foregroundColor = foregroundColors.TryGetValue(curChar.Key, out Color color) ? color : Color.White;

                    foregroundQuads[index].Position = position; foregroundQuads[index].TexCoords = texCoords[0];
                    foregroundQuads[index + 1].Position = position + new Vector2f(charWidth, 0f); foregroundQuads[index + 1].TexCoords = texCoords[1];
                    foregroundQuads[index + 2].Position = position + new Vector2f(charWidth, charHeight); foregroundQuads[index + 2].TexCoords = texCoords[2];
                    foregroundQuads[index + 3].Position = position + new Vector2f(0f, charHeight); foregroundQuads[index + 3].TexCoords = texCoords[3];

                    foregroundQuads[index].Color = foregroundColor; foregroundQuads[index + 1].Color = foregroundColor;
                    foregroundQuads[index + 2].Color = foregroundColor; foregroundQuads[index + 3].Color = foregroundColor;

                    index += 4;
                }
            }
            renderTexture.Draw(foregroundQuads, 0, (uint)(text.Count * 4), PrimitiveType.Quads, new RenderStates(font.texture));

            renderTexture.Display();
        }

        readonly byte charWidth = 0;
        readonly byte charHeight = 0;
        public readonly uint imageWidth = 0;
        public readonly uint imageHeight = 0;
        readonly uint textWidth = 0;
        readonly uint textHeight = 0;
        public BitmapFont font;
        public BitmapText(BitmapFont font, Vector2 size) {
            this.font = font;
            charWidth = (byte)font.characterSize.x;
            charHeight = (byte)font.characterSize.y;
            textWidth = (uint)size.x;
            textHeight = (uint)size.y;
            imageWidth = textWidth * charWidth;
            imageHeight = textHeight * charHeight;
            renderTexture = new RenderTexture(imageWidth, imageHeight);
            backgroundQuads = new Vertex[4 * textWidth * textHeight];
            foregroundQuads = new Vertex[4 * textWidth * textHeight];
        }
    }
}
