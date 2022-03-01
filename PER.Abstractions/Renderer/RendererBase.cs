using System;
using System.Collections.Generic;
using System.Text;

using PER.Abstractions.Input;
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
    private readonly IList<char> _formattingColorsRecord = new List<char>(8);
    private readonly StringBuilder _formattingEffectTextBuilder = new();

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

    public virtual void DrawText(Vector2Int position, string text, Color foregroundColor, Color backgroundColor,
        HorizontalAlignment align = HorizontalAlignment.Left, RenderStyle style = RenderStyle.None,
        RenderOptions options = RenderOptions.Default, IEffect? effect = null) {
        if(text.Length == 0) return;

        int actualTextLength = GetTextLengthWithoutFormatting(text);

        int x = align switch {
            HorizontalAlignment.Left => 0,
            HorizontalAlignment.Middle => -actualTextLength + actualTextLength / 2 + 1,
            HorizontalAlignment.Right => -actualTextLength + 1,
            _ => throw new ArgumentOutOfRangeException(nameof(align), align, "wtf")
        };

        bool formattingMode = false;
        bool colorSetMode = false;
        bool effectSetMode = false;
        bool foregroundSetMode = false;
        bool backgroundSetMode = false;
        _formattingColorsRecord.Clear();
        _formattingEffectTextBuilder.Clear();
        foreach(char curChar in text) {
            if(curChar == '\f') {
                formattingMode = !formattingMode;
                colorSetMode = false;
                effectSetMode = false;
                foregroundSetMode = false;
                backgroundSetMode = false;
                _formattingColorsRecord.Clear();
                _formattingEffectTextBuilder.Clear();
                continue;
            }

            if(formattingMode)
                ProcessFormatting(curChar, ref colorSetMode, ref foregroundSetMode, ref backgroundSetMode, ref effectSetMode,
                    _formattingColorsRecord, ref foregroundColor, ref backgroundColor, ref style, ref options,
                    _formattingEffectTextBuilder, ref effect, formattingEffects);
            else {
                Vector2Int charPos = new(position.x + x, position.y);
                DrawCharacter(charPos, new RenderCharacter(curChar, backgroundColor, foregroundColor, style),
                    options, effect);
                x++;
            }
        }
    }

    public virtual void DrawText(Vector2Int position, string[] lines, Color foregroundColor, Color backgroundColor,
        HorizontalAlignment align = HorizontalAlignment.Left, RenderStyle style = RenderStyle.None,
        RenderOptions options = RenderOptions.Default, IEffect? effect = null) {
        for(int i = 0; i < lines.Length; i++)
            DrawText(position + new Vector2Int(0, i), lines[i], foregroundColor, backgroundColor,
                align, style, options, effect);
    }

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

    private static void ProcessFormatting(char character, ref bool colorSetMode, ref bool foregroundSetMode,
        ref bool backgroundSetMode, ref bool effectSetMode, IList<char> colorsRecord, ref Color foregroundColor,
        ref Color backgroundColor, ref RenderStyle style, ref RenderOptions options,
        StringBuilder effectTextBuilder, ref IEffect? effect, IReadOnlyDictionary<string, IEffect?> effects) {
        if(colorSetMode)
            ProcessColorFormatting(character, ref colorSetMode, ref foregroundSetMode, ref backgroundSetMode,
                colorsRecord, ref foregroundColor, ref backgroundColor);
        else if(effectSetMode)
            ProcessEffectFormatting(character, ref effectSetMode, effectTextBuilder, ref effect, effects);
        else ProcessNormalFormatting(character, ref colorSetMode, ref effectSetMode, ref style, ref options);
    }

    private static void ProcessNormalFormatting(char character, ref bool colorSetMode, ref bool effectSetMode,
        ref RenderStyle style, ref RenderOptions options) {
        switch(character) {
            case 'c': colorSetMode = true;
                break;
            case 'e': effectSetMode = true;
                break;
            case 'b': style ^= RenderStyle.Bold;
                break;
            case 'u': style ^= RenderStyle.Underline;
                break;
            case 's': style ^= RenderStyle.Strikethrough;
                break;
            case 'i': style ^= RenderStyle.Italic;
                break;
            case 'a': options ^= RenderOptions.BackgroundAlphaBlending;
                break;
            case 'x': options ^= RenderOptions.InvertedBackgroundAsForegroundColor;
                break;
        }
    }

    private static void ProcessEffectFormatting(char character, ref bool effectSetMode,
        StringBuilder textBuilder, ref IEffect? effect, IReadOnlyDictionary<string, IEffect?> effects) {
        switch(character) {
            case 'e': effectSetMode = false;
                break;
            default:
                if(char.IsLower(character)) break;
                textBuilder.Append(character);
                break;
        }

        if(effectSetMode) return;
        string text = textBuilder.ToString();
        effect = effects[text];
    }

    private static void ProcessColorFormatting(char character, ref bool colorSetMode, ref bool foregroundSetMode,
        ref bool backgroundSetMode, IList<char> colorsRecord, ref Color foregroundColor,
        ref Color backgroundColor) {
        if(foregroundSetMode || backgroundSetMode) {
            ProcessColor(character, ref foregroundSetMode, ref backgroundSetMode, colorsRecord, ref foregroundColor,
                ref backgroundColor);
        }
        else {
            switch(character) {
                case 'c': colorSetMode = false;
                    break;
                case 'f': foregroundSetMode = true;
                    break;
                case 'b': backgroundSetMode = true;
                    break;
            }
        }
    }

    private static void ProcessColor(char character, ref bool foregroundSetMode, ref bool backgroundSetMode,
        IList<char> colorsRecord, ref Color foregroundColor, ref Color backgroundColor) {
        colorsRecord.Add(character);
        if(colorsRecord.Count < 8) return;
        byte[] colorArray = new byte[4];
        for(int i = 0; i < 4; i++)
            colorArray[i] =
                (byte)(GetHexVal(colorsRecord[i * 2]) * 0xF + GetHexVal(colorsRecord[i * 2 + 1]));

        if(foregroundSetMode) {
            foregroundColor = new Color(colorArray[0], colorArray[1], colorArray[2], colorArray[3]);
            foregroundSetMode = false;
        }
        else {
            backgroundColor = new Color(colorArray[0], colorArray[1], colorArray[2], colorArray[3]);
            backgroundSetMode = false;
        }

        colorsRecord.Clear();
    }

    private static int GetHexVal(char hex) {
        int val = hex;
        return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
    }

    private static int GetTextLengthWithoutFormatting(string text) {
        int actualTextLength = text.Length;
        bool formatting = false;
        foreach(char curChar in text) {
            if(curChar == '\f') {
                formatting = !formatting;
                actualTextLength--;
            }
            else if(formatting) actualTextLength--;
        }

        return actualTextLength;
    }
}
