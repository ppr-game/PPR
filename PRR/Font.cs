using System.Collections.Generic;

using PER.Util;

using SFML.Graphics;
using SFML.System;

using Color = SFML.Graphics.Color;

namespace PRR {
    public class Font {
        public IReadOnlyDictionary<char, Vector2f[]> characters { get; }
        public Vector2Int characterSize { get; }
        public Texture texture { get; }

        public Font(Image image, string mappings, Vector2Int characterSize) {
            this.characterSize = characterSize;
            texture = new Texture(image);
            Dictionary<char, Vector2f[]> characters = new();
            
            int index = 0;
            for(uint y = 0; y < image.Size.Y; y += (uint)characterSize.y) {
                for(uint x = 0; x < image.Size.X; x += (uint)characterSize.x) {
                    if(mappings.Length <= index) break;
                    if(IsCharacterEmpty(image, x, y, characterSize)) {
                        index++;
                        continue;
                    }
                    
                    Vector2f[] texCoords = new Vector2f[4];
                    // Clockwise
                    texCoords[0] = new Vector2f(x, y); // top left
                    texCoords[1] = new Vector2f(x + characterSize.x, y); // top right
                    texCoords[2] = new Vector2f(x + characterSize.x, y + characterSize.y); // bottom right
                    texCoords[3] = new Vector2f(x, y + characterSize.y); // bottom left
                    characters.Add(mappings[index++], texCoords);
                }
            }

            this.characters = characters;
        }

        private bool IsCharacterEmpty(Image image, uint startX, uint startY, Vector2Int characterSize) {
            for(uint imageY = startY; imageY < startY + characterSize.y; imageY++)
                for(uint imageX = startX; imageX < startX + characterSize.x; imageX++)
                    if(image.GetPixel(imageX, imageY) != Color.Transparent)
                        return false;

            return true;
        }
    }
}
