using System;
using System.Collections.Generic;

using PER.Util;

namespace PER.Abstractions.Renderer;

public abstract class RendererBase : IRenderer {
    public virtual string title { get; private set; } = "";
    public virtual int width { get; private set; }
    public virtual int height { get; private set; }

    public virtual int framerate {
        get => _framerate;
        set {
            _framerate = value;
            UpdateFramerate();
        }
    }

    public virtual bool fullscreen {
        get => _fullscreen;
        set {
            _fullscreen = value;
            Reset();
        }
    }

    public virtual IFont? font {
        get => _font;
        set {
            _font = value;
            Reset();
        }
    }

    public virtual string? icon { get; set; }

    public abstract bool open { get; }
    public abstract bool focused { get; }
    public abstract event EventHandler? closed;

    public virtual Color background { get; set; } = Color.black;

    public Dictionary<string, IEffect?> formattingEffects { get; } = new();

    protected List<IEffect> fullscreenEffects { get; private set; } = new();

    protected Dictionary<Vector2Int, RenderCharacter> display { get; private set; } = new();
    protected Dictionary<Vector2Int, IEffect> effects { get; private set; } = new();

    private int _framerate;
    private bool _fullscreen;
    private IFont? _font;

    public virtual void Setup(RendererSettings settings) {
        title = settings.title;
        width = settings.width;
        height = settings.height;
        _framerate = settings.framerate;
        _fullscreen = settings.fullscreen;
        _font = settings.font;
        icon = settings.icon;

        CreateWindow();
    }

    protected abstract void CreateWindow();
    protected abstract void UpdateFramerate();

    public abstract void Update();
    public abstract void Close();
    public abstract void Finish();

    public virtual void Reset(RendererSettings settings) {
        Finish();
        Setup(settings);
    }

    public virtual void Reset() => Reset(new RendererSettings(this));

    protected virtual void UpdateFont() {
        display = new Dictionary<Vector2Int, RenderCharacter>(width * height);
        effects = new Dictionary<Vector2Int, IEffect>(width * height);
        fullscreenEffects = new List<IEffect>();

        CreateText();
    }

    protected abstract void CreateText();

    public virtual void Clear() => display.Clear();
    public abstract void Draw();

    protected void DrawAllEffects() {
        DrawEffects();
        DrawFullscreenEffects();
    }

    private void DrawEffects() {
        foreach((Vector2Int position, IEffect effect) in effects) {
            effect.Update(false);
            if(!effect.drawable) continue;
            effect.Draw(position);
        }
    }

    private void DrawFullscreenEffects() {
        foreach(IEffect effect in fullscreenEffects) {
            effect.Update(true);
            if(!effect.drawable) continue;
            for(int y = 0; y < height; y++)
                for(int x = 0; x < width; x++)
                    effect.Draw(new Vector2Int(x, y));
        }
    }

    public virtual void DrawCharacter(Vector2Int position, RenderCharacter character,
        RenderOptions options = RenderOptions.Default, IEffect? effect = null) {
        if(position.x < 0 || position.y < 0 || position.x >= width || position.y >= height) return;

        if((options & RenderOptions.BackgroundAlphaBlending) != 0) {
            RenderCharacter currentCharacter = GetCharacter(position);
            Color background = currentCharacter.background.Blend(character.background);
            character = new RenderCharacter(character.character, background, character.foreground, character.style);
        }

        if((options & RenderOptions.InvertedBackgroundAsForegroundColor) != 0) {
            RenderCharacter currentCharacter = GetCharacter(position);
            character = new RenderCharacter(character.character, character.background,
                Color.white - currentCharacter.background, character.style);
        }

        if(IsCharacterEmpty(character)) display.Remove(position);
        else display[position] = character;
        AddEffect(position, effect);
    }

    public virtual void DrawText(Vector2Int position, string text, Func<char, Formatting> formatter,
        HorizontalAlignment align = HorizontalAlignment.Left) {
        if(text.Length == 0) return;

        char formattingFlag = '\0';
        int startIndex = 0;
        int width = 0;
        int y = 0;
        for(int i = 0; i <= text.Length; i++) {
            char currentCharacter = i >= text.Length ? '\n' : text[i];
            switch(currentCharacter) {
                case '\n':
                    int x = GetAlignOffset(align, width);
                    DrawTextCharacter(position, text, startIndex, x, y, width, formatter, ref formattingFlag);

                    startIndex = i + 1;
                    width = 0;
                    y++;
                    break;
                case '\f': i++; // skip 2 characters
                    break;
                case not '\r': width++;
                    break;
            }
        }
    }

    private void DrawTextCharacter(Vector2Int position, string text, int startIndex, int x, int y, int width,
        Func<char, Formatting> formatter, ref char formattingFlag) {
        for(int i = startIndex; i < startIndex + width; i++) {
            char toDraw = text[i];
            if(toDraw == '\f') {
                formattingFlag = text[++i];
                width += 2;
                continue;
            }

            Formatting formatting = formatter(formattingFlag);
            Vector2Int charPos = new(position.x + x, position.y + y);
            DrawCharacter(charPos,
                new RenderCharacter(toDraw, formatting.backgroundColor, formatting.foregroundColor,
                    formatting.style), formatting.options, formatting.effect);
            x++;
        }
    }

    private static int GetAlignOffset(HorizontalAlignment align, int width) => align switch {
        HorizontalAlignment.Left => 0,
        HorizontalAlignment.Middle => -width + width / 2 + 1,
        HorizontalAlignment.Right => -width + 1,
        _ => 0
    };

    public virtual RenderCharacter GetCharacter(Vector2Int position) => IsCharacterEmpty(position) ?
        new RenderCharacter('\0', Color.transparent, Color.transparent) : display[position];

    public virtual void AddEffect(IEffect effect) => fullscreenEffects.Add(effect);

    public virtual void AddEffect(Vector2Int position, IEffect? effect) {
        if(effect is null) {
            effects.Remove(position);
            return;
        }
        effects[position] = effect;
    }

    public virtual bool IsCharacterEmpty(Vector2Int position) => !display.ContainsKey(position);

    public virtual bool IsCharacterEmpty(RenderCharacter renderCharacter) =>
        renderCharacter.background.a == 0f &&
        (!IsCharacterDrawable(renderCharacter.character, renderCharacter.style) ||
         renderCharacter.foreground.a == 0f);

    public virtual bool IsCharacterDrawable(char character, RenderStyle style) =>
        font?.IsCharacterDrawable(character, style & RenderStyle.AllPerFont) ?? false;
}
