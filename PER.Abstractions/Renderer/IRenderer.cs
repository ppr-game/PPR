using System.Collections.Generic;

using PER.Abstractions.Input;
using PER.Util;

namespace PER.Abstractions.Renderer;

public interface IRenderer {
    public string title { get; }
    public int width { get; }
    public int height { get; }
    public int framerate { get; set; }
    public bool fullscreen { get; set; }
    public IFont? font { get; set; }
    public string? icon { get; set; }

    public bool open { get; }
    public bool focused { get; }

    public Color background { get; set; }

    public IInputManager? input { get; }

    public Dictionary<string, IEffect?> formattingEffects { get; }

    public void Setup(RendererSettings settings);
    public void Update();
    public void Finish();
    public void Reset();
    public void Reset(RendererSettings settings);

    public void Clear();
    public void Draw();
    public void DrawCharacter(Vector2Int position, RenderCharacter character,
        RenderOptions options = RenderOptions.Default, IEffect? effect = null);
    public void DrawText(Vector2Int position, string text, Color foregroundColor, Color backgroundColor,
        HorizontalAlignment align = HorizontalAlignment.Left, RenderStyle style = RenderStyle.None,
        RenderOptions options = RenderOptions.Default, IEffect? effect = null);
    public void DrawText(Vector2Int position, string[] lines, Color foregroundColor, Color backgroundColor,
        HorizontalAlignment align = HorizontalAlignment.Left, RenderStyle style = RenderStyle.None,
        RenderOptions options = RenderOptions.Default, IEffect? effect = null);

    public RenderCharacter GetCharacter(Vector2Int position);

    public void AddEffect(IEffect effect);
    public void AddEffect(Vector2Int position, IEffect? effect);

    public bool IsCharacterEmpty(Vector2Int position);
    public bool IsCharacterEmpty(RenderCharacter renderCharacter);
    public bool IsCharacterDrawable(char character, RenderStyle style);
}
