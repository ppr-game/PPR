using PER.Abstractions.Resources;
using PER.Util;

namespace PPR.Resources;

public class DialogBoxPaletteResource : Resource {
    public const string GlobalId = "layouts/dialogBoxPalette";

    protected override IEnumerable<KeyValuePair<string, string>> paths { get; } = new Dictionary<string, string> {
        { "palette", "layouts/dialogBox.txt" }
    };

    // ReSharper disable once MemberCanBePrivate.Global
    public string palette { get; private set; } = "                ";

    public override void Load(string id) {
        if(!TryGetPath("palette", out string? palettePath))
            return;
        string palette = File.ReadAllText(palettePath);
        if(palette.Length < 16)
            throw new InvalidDataException("Dialog box palette should be at least 16 characters long");
        this.palette = palette;
    }

    public override void Unload(string id) { }

    public char Get(int x, int y, Vector2Int size) =>
        Get(x == 0, x == size.x - 1, y == 0, y == size.y - 1);

    // ReSharper disable once MemberCanBePrivate.Global
    // haha micro optimization go brrrr
    public char Get(bool isStartX, bool isEndX, bool isStartY, bool isEndY) {
        unsafe {
            return BitConverter.IsLittleEndian ?
                palette[*(byte*)&isStartX << 3 | *(byte*)&isEndX << 2 | *(byte*)&isStartY << 1 | *(byte*)&isEndY] :
                palette[*(byte*)&isEndY << 3 | *(byte*)&isStartY << 2 | *(byte*)&isEndX << 1 | *(byte*)&isStartX];
        }
    }
}
