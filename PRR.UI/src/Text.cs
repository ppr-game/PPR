using JetBrains.Annotations;

using PER.Abstractions.Rendering;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

[PublicAPI]
public class Text : Element {
    public string? text { get; set; }
    public Dictionary<char, Formatting> formatting { get; set; } = new();
    public HorizontalAlignment align { get; set; } = HorizontalAlignment.Left;
    public bool wrap { get; set; }

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

    public override Element Clone() => Clone(this);

    public override void Update(TimeSpan time) {
        if(!enabled || text is null)
            return;
        if(formatting.Count == 0)
            formatting.Add('\0',
                new Formatting(Color.white, Color.transparent, RenderStyle.None, RenderOptions.Default, effect));
        renderer.DrawText(position, text, flag => formatting[flag], align, wrap ? size.x : 0);
    }

    public override void UpdateColors(Dictionary<string, Color> colors, string layoutName, string id, string? special) {
        Color foregroundColor = Color.white;
        Color backgroundColor = Color.transparent;
        if(TryGetColor(colors, "text", layoutName, id, "fg", special, out Color color))
            foregroundColor = color;
        if(TryGetColor(colors, "text", layoutName, id, "bg", special, out color))
            backgroundColor = color;
        formatting['\0'] = formatting.TryGetValue('\0', out Formatting oldFormatting) ?
            new Formatting(foregroundColor, backgroundColor, oldFormatting.style, oldFormatting.options,
                oldFormatting.effect) :
            new Formatting(foregroundColor, backgroundColor);
    }
}
