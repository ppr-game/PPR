using JetBrains.Annotations;

using PER.Abstractions.Rendering;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

[PublicAPI]
public class FilledPanel : Element {
    public RenderCharacter character { get; set; }

    public FilledPanel(IRenderer renderer) : base(renderer) { }

    public override Element Clone() => throw new NotImplementedException();

    public override void Update(TimeSpan time) {
        if(!enabled) return;
        for(int y = bounds.min.y; y <= bounds.max.y; y++)
            for(int x = bounds.min.x; x <= bounds.max.x; x++)
                renderer.DrawCharacter(new Vector2Int(x, y), character, RenderOptions.Default, effect);
    }

    public override void UpdateColors(Dictionary<string, Color> colors, string layoutName, string id,
        string? special) => throw new NotImplementedException();
}
