using PER.Abstractions.Renderer;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

public class Text : Element {
    public string? text { get; set; }
    public Color foregroundColor { get; set; } = Color.white;
    public Color backgroundColor { get; set; } = Color.transparent;
    public HorizontalAlignment align { get; set; } = HorizontalAlignment.Left;
    public RenderStyle style { get; set; } = RenderStyle.None;
    public RenderOptions options { get; set; } = RenderOptions.Default;

    public Text(IRenderer renderer) : base(renderer) { }

    public override void Update(IReadOnlyStopwatch clock) {
        if(!enabled || text is null) return;
        renderer.DrawText(position, text.Split('\n'), foregroundColor, backgroundColor, align, style, options, effect);
    }
}
