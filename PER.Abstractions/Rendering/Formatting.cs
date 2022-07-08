using PER.Util;

namespace PER.Abstractions.Rendering;

public readonly struct Formatting {
    public Color foregroundColor { get; }
    public Color backgroundColor { get; }
    public RenderStyle style { get; }
    public RenderOptions options { get; }
    public IEffect? effect { get; }

    public Formatting(Color foregroundColor, Color backgroundColor, RenderStyle style = RenderStyle.None,
        RenderOptions options = RenderOptions.Default, IEffect? effect = null) {
        this.foregroundColor = foregroundColor;
        this.backgroundColor = backgroundColor;
        this.style = style;
        this.options = options;
        this.effect = effect;
    }
}
