using System;
using System.Collections.Generic;

using PER.Util;

namespace PER.Abstractions.Rendering;

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
    public event EventHandler closed;

    public Color background { get; set; }

    public Dictionary<string, IEffect?> formattingEffects { get; }

    public void Setup(RendererSettings settings);
    public void Update();
    public void Close();
    public void Finish();
    public void Reset();
    public void Reset(RendererSettings settings);

    public void Clear();
    public void Draw();
    public void DrawCharacter(Vector2Int position, RenderCharacter character,
        RenderOptions options = RenderOptions.Default, IEffect? effect = null);
    public void DrawText(Vector2Int position, string text, Func<char, Formatting> formatter,
        HorizontalAlignment align = HorizontalAlignment.Left, int maxWidth = 0);

    public RenderCharacter GetCharacter(Vector2Int position);

    public void AddEffect(IEffect effect);
    public void AddEffect(Vector2Int position, IEffect? effect);

    public bool IsCharacterEmpty(Vector2Int position);
    public bool IsCharacterEmpty(RenderCharacter renderCharacter);
    public bool IsCharacterDrawable(char character, RenderStyle style);
}
