using System.Collections.Generic;

using PER.Abstractions.Renderer;
using PER.Util;

using SFML.Graphics;
using SFML.System;

using Color = SFML.Graphics.Color;

namespace PRR {
    public class Font {
        public IReadOnlyDictionary<(char, RenderStyle), Vector2f[]> characters { get; }
        public Vector2f[] backgroundCharacter { get; } = { new(), new(), new(), new() };
        public Vector2Int characterSize { get; }
        public Texture texture { get; }
        public string mappings { get; }

        public Font(Image image, string mappings, Vector2Int characterSize, char backgroundCharacter) {
            this.mappings = mappings;
            
            uint originalHeight = image.Size.Y;
            image = GenerateFontStyles(image, characterSize);
            texture = new Texture(image);
            this.characterSize = characterSize;

            Dictionary<(char, RenderStyle), Vector2f[]> characters = new();
            
            int index = 0;
            for(uint y = 0; y < image.Size.Y; y += (uint)characterSize.y) {
                for(uint x = 0; x < image.Size.X; x += (uint)characterSize.x) {
                    if(mappings.Length <= index) index = 0;
                    if(IsCharacterEmpty(image, x, y, characterSize)) {
                        index++;
                        continue;
                    }
                    RenderStyle style = (RenderStyle)(y / originalHeight);
                    char character = mappings[index];
                    
                    Vector2f[] texCoords = new Vector2f[4];
                    // Clockwise
                    texCoords[0] = new Vector2f(x, y); // top left
                    texCoords[1] = new Vector2f(x + characterSize.x, y); // top right
                    texCoords[2] = new Vector2f(x + characterSize.x, y + characterSize.y); // bottom right
                    texCoords[3] = new Vector2f(x, y + characterSize.y); // bottom left
                    characters.Add((character, style), texCoords);
                    
                    if(character == backgroundCharacter && style == RenderStyle.None)
                        this.backgroundCharacter = texCoords;

                    index++;
                }
            }

            this.characters = characters;
        }

        private static Image GenerateFontStyles(Image image, Vector2Int characterSize) {
            Image newImage = new(image.Size.X, image.Size.Y * ((uint)RenderStyle.AllPerFont + 1));
            
            for(RenderStyle style = RenderStyle.None; style <= RenderStyle.AllPerFont; style++) {
                uint imageOffset = image.Size.Y * (uint)style;
                GenerateStyle(image, newImage, style, characterSize, imageOffset);
            }

            return newImage;
        }

        private static void GenerateStyle(Image sourceImage, Image stylesImage, RenderStyle style,
            Vector2Int characterSize, uint imageOffset) {
            stylesImage.Copy(sourceImage, 0, imageOffset);
                
            bool bold = style.HasFlag(RenderStyle.Bold);
            bool underline = style.HasFlag(RenderStyle.Underline);
            bool strikethrough = style.HasFlag(RenderStyle.Strikethrough);
                
            if(bold) stylesImage.Copy(sourceImage, 1, imageOffset, new IntRect(), true);

            if(!underline && !strikethrough) return;
            uint underlineThickness = (uint)characterSize.y / 10;
            uint strikethroughThickness = (uint)characterSize.y / 10;
            for(uint y = 0; y < sourceImage.Size.Y; y += (uint)characterSize.y) {
                if(underline)
                    AddUnderline(stylesImage, y + imageOffset, (uint)characterSize.y, underlineThickness);
                    
                if(strikethrough)
                    AddStrikethrough(stylesImage, y + imageOffset, (uint)characterSize.y,
                        strikethroughThickness);
            }
        }

        private static void AddUnderline(Image image, uint y, uint characterHeight, uint thickness) {
            for(uint x = 0; x < image.Size.X; x++)
                AddUnderline(image, x, y, characterHeight, thickness);
        }

        private static void AddUnderline(Image image, uint x, uint y, uint characterHeight, uint thickness) {
            for(uint i = 0; i < thickness; i++)
                image.SetPixel(x, y + characterHeight - 1 - i, Color.White);
        }

        private static void AddStrikethrough(Image image, uint y, uint characterHeight, uint thickness) {
            for(uint x = 0; x < image.Size.X; x++)
                AddStrikethrough(image, x, y, characterHeight, thickness);
        }

        private static void AddStrikethrough(Image image, uint x, uint y, uint characterHeight, uint thickness) {
            for(uint i = 0; i < thickness; i++)
                image.SetPixel(x, y + (characterHeight - thickness) / 2 + i, Color.White);
        }

        private static bool IsCharacterEmpty(Image image, uint startX, uint startY, Vector2Int characterSize) {
            for(uint imageY = startY; imageY < startY + characterSize.y; imageY++)
                for(uint imageX = startX; imageX < startX + characterSize.x; imageX++)
                    if(image.GetPixel(imageX, imageY) != Color.Transparent)
                        return false;

            return true;
        }
    }
}
