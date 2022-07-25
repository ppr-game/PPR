using JetBrains.Annotations;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

[PublicAPI]
public class Button : ClickableElement {
    protected override string type => "button";

    public KeyCode? hotkey { get; set; }

    public string? text { get; set; }
    public RenderStyle style { get; set; } = RenderStyle.None;

    public bool toggled {
        get => toggledSelf;
        set => toggledSelf = value;
    }

    protected override bool hotkeyPressed => hotkey.HasValue && input.KeyPressed(hotkey.Value);

    public Button(IRenderer renderer, IInput input, IAudio? audio = null) : base(renderer, input, audio) { }

    public static Button Clone(Button template) => new(template.renderer, template.input, template.audio) {
        enabled = template.enabled,
        position = template.position,
        size = template.size,
        effect = template.effect,
        hotkey = template.hotkey,
        text = template.text,
        style = template.style,
        active = template.active,
        toggled = template.toggled,
        inactiveColor = template.inactiveColor,
        idleColor = template.idleColor,
        hoverColor = template.hoverColor,
        clickColor = template.clickColor,
        clickSound = template.clickSound
    };

    public override Element Clone() => Clone(this);

    protected override void CustomUpdate(TimeSpan time) {
        if(text is null)
            return;
        renderer.DrawText(center, text,
            _ => new Formatting(Color.white, Color.transparent, style, RenderOptions.Default, effect),
            HorizontalAlignment.Middle);
    }

    protected override void DrawCharacter(int x, int y, Color backgroundColor, Color foregroundColor) {
        Vector2Int position = new(this.position.x + x, this.position.y + y);
        RenderCharacter character = renderer.GetCharacter(position);
        character = new RenderCharacter(character.character, backgroundColor, foregroundColor, style);
        renderer.DrawCharacter(position, character, RenderOptions.Default, effect);
    }
}
