using System.Collections.Generic;
using System.Globalization;
using System.IO;

using JetBrains.Annotations;

using PER.Util;

namespace PER.Abstractions.Rendering;

[PublicAPI]
public abstract class BasicFont : IFont {
    public IReadOnlyDictionary<(char, RenderStyle), Vector2[]> characters => _characters;
    public Vector2[] backgroundCharacter { get; private set; } = { new(), new(), new(), new() };
    public Vector2Int size { get; }
    public Image image { get; private set; }
    public string mappings { get; }

    private readonly HashSet<(char, RenderStyle)> _drawable = new();
    private readonly Dictionary<(char, RenderStyle), Vector2[]> _characters = new();

    protected BasicFont(string imagePath, string mappingsPath) {
        string[] fontMappingsLines = File.ReadAllLines(mappingsPath);
        string[] fontSizeStr = fontMappingsLines[0].Split(',');
        mappings = fontMappingsLines[1];
        size = new Vector2Int(int.Parse(fontSizeStr[0], CultureInfo.InvariantCulture),
            int.Parse(fontSizeStr[1], CultureInfo.InvariantCulture));

        Setup(imagePath, fontSizeStr[2][0]);
    }

    protected BasicFont(Image image, string mappings, Vector2Int size, char backgroundCharacter) {
        this.image = image;
        this.size = size;
        this.mappings = mappings;
        Setup(backgroundCharacter);
    }

    public bool IsCharacterDrawable(char character, RenderStyle style) => _drawable.Contains((character, style));

    protected abstract Image ReadImage(string path);

    private void Setup(string imagePath, char backgroundCharacter) {
        image = ReadImage(imagePath);
        Setup(backgroundCharacter);
    }

    private void Setup(char backgroundCharacter) {
        int originalHeight = image.height;
        image = GenerateFontStyles(image, size);

        _characters.Clear();

        int index = 0;
        for(int y = 0; y < image.height; y += size.y)
            for(int x = 0; x < image.width; x += size.x)
                AddCharacter(x, y, ref index, originalHeight, backgroundCharacter);
    }

    private void AddCharacter(int x, int y, ref int index, int originalHeight, char backgroundCharacter) {
        if(mappings.Length <= index)
            index = 0;

        if(IsCharacterEmpty(image, x, y, size)) {
            index++;
            return;
        }

        RenderStyle style = (RenderStyle)(y / originalHeight);
        char character = mappings[index];
        _drawable.Add((character, style));

        Vector2[] texCoords = new Vector2[4];
        // Clockwise
        texCoords[0] = new Vector2(x, y); // top left
        texCoords[1] = new Vector2(x + size.x, y); // top right
        texCoords[2] = new Vector2(x + size.x, y + size.y); // bottom right
        texCoords[3] = new Vector2(x, y + size.y); // bottom left
        _characters.Add((character, style), texCoords);

        if(character == backgroundCharacter && style == RenderStyle.None)
            this.backgroundCharacter = texCoords;

        index++;
    }

    private static Image GenerateFontStyles(Image image, Vector2Int characterSize) {
        Image newImage = new(image.width, image.height * ((int)RenderStyle.AllPerFont + 1));

        for(RenderStyle style = RenderStyle.None; style <= RenderStyle.AllPerFont; style++) {
            int imageOffset = image.height * (int)style;
            GenerateStyle(image, newImage, style, characterSize, imageOffset);
        }

        return newImage;
    }

    private static void GenerateStyle(Image sourceImage, Image stylesImage, RenderStyle style,
        Vector2Int characterSize, int imageOffset) {
        stylesImage.DrawImage(new Vector2Int(0, imageOffset), sourceImage);

        bool bold = style.HasFlag(RenderStyle.Bold);
        bool underline = style.HasFlag(RenderStyle.Underline);
        bool strikethrough = style.HasFlag(RenderStyle.Strikethrough);

        if(bold)
            stylesImage.DrawImage(new Vector2Int(1, imageOffset), sourceImage);

        if(!underline && !strikethrough)
            return;

        int underlineThickness = characterSize.y / 10;
        int strikethroughThickness = characterSize.y / 10;
        for(int y = 0; y < sourceImage.height; y += characterSize.y) {
            if(underline)
                AddUnderline(stylesImage, y + imageOffset, characterSize.y, underlineThickness);

            if(strikethrough)
                AddStrikethrough(stylesImage, y + imageOffset, characterSize.y,
                    strikethroughThickness);
        }
    }

    private static void AddUnderline(Image image, int y, int characterHeight, int thickness) {
        for(int x = 0; x < image.width; x++)
            AddUnderline(image, x, y, characterHeight, thickness);
    }

    private static void AddUnderline(Image image, int x, int y, int characterHeight, int thickness) {
        for(int i = 0; i < thickness; i++)
            image[x, y + characterHeight - 1 - i] = Color.white;
    }

    private static void AddStrikethrough(Image image, int y, int characterHeight, int thickness) {
        for(int x = 0; x < image.width; x++)
            AddStrikethrough(image, x, y, characterHeight, thickness);
    }

    private static void AddStrikethrough(Image image, int x, int y, int characterHeight, int thickness) {
        for(int i = 0; i < thickness; i++)
            image[x, y + (characterHeight - thickness) / 2 + i] = Color.white;
    }

    private static bool IsCharacterEmpty(Image image, int startX, int startY, Vector2Int characterSize) {
        for(int y = startY; y < startY + characterSize.y; y++)
            for(int x = startX; x < startX + characterSize.x; x++)
                if(image[x, y].a != 0f)
                    return false;

        return true;
    }
}
