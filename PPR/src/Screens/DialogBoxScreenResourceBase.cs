using PER.Abstractions;
using PER.Abstractions.Rendering;
using PER.Abstractions.UI;
using PER.Common.Resources;
using PER.Util;

using PPR.Resources;

using PRR.UI.Resources;

namespace PPR.Screens;

public abstract class DialogBoxScreenResourceBase : ScreenResourceBase {
    protected Vector2Int size { get; set; }

    protected virtual string foregroundColorId => "dialogBox_fg";
    protected virtual string backgroundColorId => "dialogBox_bg";
    protected virtual IEffect? frameEffect => null;

    private ColorsResource? _colors;
    private DialogBoxPaletteResource? _palette;

    protected DialogBoxScreenResourceBase(Vector2Int size) => this.size = size;

    public override void Open() {
        if(!Core.engine.resources.TryGetResource(ColorsResource.GlobalId, out _colors) ||
            !Core.engine.resources.TryGetResource(DialogBoxPaletteResource.GlobalId, out _palette))
            throw new InvalidOperationException("Missing dependency.");
    }

    public override void Close() {
        _colors = null;
        _palette = null;
    }

    public override void Update() {
        if(_colors is null || _palette is null ||
            !_colors.colors.TryGetValue(backgroundColorId, out Color backgroundColor) ||
            !_colors.colors.TryGetValue(foregroundColorId, out Color foregroundColor))
            return;

        Vector2Int offset = new((renderer.width - size.x) / 2, (renderer.height - size.y) / 2);
        for(int y = 0; y < size.y; y++)
            for(int x = 0; x < size.x; x++)
                renderer.DrawCharacter(new Vector2Int(offset.x + x, offset.y + y),
                    new RenderCharacter(_palette.Get(x, y, size), backgroundColor, foregroundColor),
                    RenderOptions.Default, frameEffect);

        foreach((string _, Element element) in elements)
            element.Update(Core.engine.clock);
    }
}
