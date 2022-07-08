using PER.Abstractions.Renderer;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

public class Text : Element {
    public string? text { get; set; }
    public Dictionary<char, Formatting> formatting { get; set; } = new();
    public HorizontalAlignment align { get; set; } = HorizontalAlignment.Left;

    public Text(IRenderer renderer) : base(renderer) { }

    public static Text Clone(Text template) => new(template.renderer) {
        enabled = template.enabled,
        position = template.position,
        size = template.size,
        effect = template.effect,
        text = template.text,
        formatting = new Dictionary<char, Formatting>(template.formatting),
        align = template.align
    };

    public override void Update(IReadOnlyStopwatch clock) {
        if(!enabled || text is null)
            return;
        if(formatting.Count == 0)
            formatting.Add('\0',
                new Formatting(Color.white, Color.transparent, RenderStyle.None, RenderOptions.Default, effect));
        renderer.DrawText(position, text, flag => formatting[flag], align);
    }
}
