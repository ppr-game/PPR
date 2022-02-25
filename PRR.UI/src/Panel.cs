using PER.Abstractions.Renderer;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

public class Panel : Element {
    public RenderCharacter character { get; set; }

    public Panel(IRenderer renderer) : base(renderer) { }

    public override void Update(IReadOnlyStopwatch clock) {
        if(!enabled) return;
        for(int y = bounds.min.y; y <= bounds.max.y; y++)
            for(int x = bounds.min.x; x <= bounds.max.x; x++)
                renderer.DrawCharacter(new Vector2Int(x, y), character, RenderOptions.Default, effect);
    }
}
