using System.Collections.Generic;
using System.Globalization;
using System.IO;

using PER.Abstractions.Renderer;
using PER.Util;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Image = SixLabors.ImageSharp.Image;

namespace PRR {
    public class Font : IFont {
        public IReadOnlyDictionary<(char, RenderStyle), Vector2[]> characters { get; private set; }
        public Vector2[] backgroundCharacter { get; private set; } = { new(), new(), new(), new() };
        public Vector2Int size { get; }
        public Image<Rgba32> image { get; private set; }
        public string mappings { get; }

        public Font(string folderPath) {
            image = Image.Load<Rgba32>(Path.Join(folderPath, "font.png"));
            
            string[] fontMappingsLines = File.ReadAllLines(Path.Join(folderPath, "mappings.txt"));
            string[] fontSizeStr = fontMappingsLines[0].Split(',');
            mappings = fontMappingsLines[1];
            size = new Vector2Int(int.Parse(fontSizeStr[0], CultureInfo.InvariantCulture),
                int.Parse(fontSizeStr[1], CultureInfo.InvariantCulture));

            Setup(fontSizeStr[2][0]);
        }

        public Font(Image<Rgba32> image, string mappings, Vector2Int size, char backgroundCharacter) {
            this.image = image;
            this.size = size;
            this.mappings = mappings;
            Setup(backgroundCharacter);
        }

        private void Setup(char backgroundCharacter) {
            int originalHeight = image.Height;
            image = GenerateFontStyles(image, size);

            Dictionary<(char, RenderStyle), Vector2[]> characters = new();

            int index = 0;
            for(int y = 0; y < image.Height; y += size.y) {
                for(int x = 0; x < image.Width; x += size.x) {
                    if(mappings.Length <= index) index = 0;
                    if(IsCharacterEmpty(image, x, y, size)) {
                        index++;
                        continue;
                    }

                    RenderStyle style = (RenderStyle)(y / originalHeight);
                    char character = mappings[index];

                    Vector2[] texCoords = new Vector2[4];
                    // Clockwise
                    texCoords[0] = new Vector2(x, y); // top left
                    texCoords[1] = new Vector2(x + size.x, y); // top right
                    texCoords[2] = new Vector2(x + size.x, y + size.y); // bottom right
                    texCoords[3] = new Vector2(x, y + size.y); // bottom left
                    characters.Add((character, style), texCoords);

                    if(character == backgroundCharacter && style == RenderStyle.None) this.backgroundCharacter = texCoords;

                    index++;
                }
            }

            this.characters = characters;
        }

        private static Image<Rgba32> GenerateFontStyles(Image image, Vector2Int characterSize) {
            Image<Rgba32> newImage = new(image.Width, image.Height * ((int)RenderStyle.AllPerFont + 1));
            
            for(RenderStyle style = RenderStyle.None; style <= RenderStyle.AllPerFont; style++) {
                int imageOffset = image.Height * (int)style;
                GenerateStyle(image, newImage, style, characterSize, imageOffset);
            }

            return newImage;
        }

        private static void GenerateStyle(Image sourceImage, Image<Rgba32> stylesImage, RenderStyle style,
            Vector2Int characterSize, int imageOffset) {
            stylesImage.Mutate(ipc =>
                ipc.DrawImage(sourceImage, new Point(0, imageOffset), new GraphicsOptions { Antialias = false }));
                
            bool bold = style.HasFlag(RenderStyle.Bold);
            bool underline = style.HasFlag(RenderStyle.Underline);
            bool strikethrough = style.HasFlag(RenderStyle.Strikethrough);

            if(bold)
                stylesImage.Mutate(ipc =>
                    ipc.DrawImage(sourceImage, new Point(1, imageOffset), new GraphicsOptions { Antialias = false }));

            if(!underline && !strikethrough) return;
            int underlineThickness = characterSize.y / 10;
            int strikethroughThickness = characterSize.y / 10;
            for(int y = 0; y < sourceImage.Height; y += characterSize.y) {
                if(underline)
                    AddUnderline(stylesImage, y + imageOffset, characterSize.y, underlineThickness);
                    
                if(strikethrough)
                    AddStrikethrough(stylesImage, y + imageOffset, characterSize.y,
                        strikethroughThickness);
            }
        }

        private static void AddUnderline(Image<Rgba32> image, int y, int characterHeight, int thickness) {
            for(int x = 0; x < image.Width; x++)
                AddUnderline(image, x, y, characterHeight, thickness);
        }

        private static void AddUnderline(Image<Rgba32> image, int x, int y, int characterHeight, int thickness) {
            for(int i = 0; i < thickness; i++)
                image[x, y + characterHeight - 1 - i] = new Rgba32(255, 255, 255);
        }

        private static void AddStrikethrough(Image<Rgba32> image, int y, int characterHeight, int thickness) {
            for(int x = 0; x < image.Width; x++)
                AddStrikethrough(image, x, y, characterHeight, thickness);
        }

        private static void AddStrikethrough(Image<Rgba32> image, int x, int y, int characterHeight, int thickness) {
            for(int i = 0; i < thickness; i++)
                image[x, y + (characterHeight - thickness) / 2 + i] = new Rgba32(255, 255, 255);
        }

        private static bool IsCharacterEmpty(Image<Rgba32> image, int startX, int startY, Vector2Int characterSize) {
            for(int y = startY; y < startY + characterSize.y; y++)
                for(int x = startX; x < startX + characterSize.x; x++)
                    if(image[x, y].A != 0)
                        return false;

            return true;
        }
    }
}
