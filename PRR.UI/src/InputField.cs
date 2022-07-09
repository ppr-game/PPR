using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Util;

namespace PRR.UI;

public class InputField : ClickableElementBase {
    public const string TypeSoundId = "inputFieldType";
    public const string EraseSoundId = "inputFieldErase";
    public const string SubmitSoundId = "inputFieldSubmit";

    public override bool enabled {
        get => base.enabled;
        set {
            base.enabled = value;
            if(typing)
                StartTyping();
            else
                StopTyping();
        }
    }

    public override bool active {
        get => base.active;
        set {
            base.active = value;
            if(typing)
                StartTyping();
            else
                StopTyping();
        }
    }

    public string? value {
        get => _value;
        set {
            _value = value;
            onTextChange?.Invoke(this, EventArgs.Empty);
        }
    }

    public string? placeholder { get; set; }
    public RenderStyle style { get; set; } = RenderStyle.None;

    public int cursor {
        get => _cursor;
        set {
            _cursor = Math.Clamp(value, 0, this.value?.Length ?? 0);
            int sizeX = size.x;
            while(_cursor - _textOffset > sizeX)
                _textOffset++;
            while(_cursor - _textOffset < 0)
                _textOffset--;
        }
    }

    public IPlayable? typeSound { get; set; }
    public IPlayable? eraseSound { get; set; }
    public IPlayable? submitSound { get; set; }

    public event EventHandler? onStartTyping;
    public event EventHandler? onTextChange;
    public event EventHandler? onSubmit;

    protected override bool hotkeyPressed => false;

    private bool typing => enabled && active && toggledSelf;
    private bool usePlaceholder => string.IsNullOrEmpty(value) && !typing;

    private string? _value;

    private IReadOnlyStopwatch _lastClock = new Stopwatch();
    private TimeSpan _lastTypeTime;

    private int _cursor;
    private int _textOffset;

    public InputField(IRenderer renderer, IInput input, IAudio? audio = null) : base(renderer, input, audio) { }

    protected override void UpdateState(IReadOnlyStopwatch clock) {
        base.UpdateState(clock);
        if(!typing)
            return;

        if(!input.KeyPressed(KeyCode.Enter) && !input.KeyPressed(KeyCode.Escape) &&
            (currentState == State.Clicked || !input.MouseButtonPressed(MouseButton.Left)))
            return;
        StopTyping();
    }

    private void StartTyping() {
        toggledSelf = true;
        input.keyDown += KeyDown;
        input.textEntered += Type;
        input.keyRepeat = true;
        cursor = value?.Length ?? 0;
        onStartTyping?.Invoke(this, EventArgs.Empty);
    }

    private void StopTyping() {
        toggledSelf = false;
        input.keyDown -= KeyDown;
        input.textEntered -= Type;
        input.keyRepeat = false;
        _textOffset = 0;
        onSubmit?.Invoke(this, EventArgs.Empty);
        PlaySound(audio, submitSound, SubmitSoundId);
    }

    protected override void CustomUpdate(IReadOnlyStopwatch clock) {
        _lastClock = clock;
        string? drawText = usePlaceholder ? placeholder : value;
        if(drawText is null)
            return;
        ReadOnlySpan<char> drawTextSpan = drawText.AsSpan();
        int textMin = Math.Clamp(_textOffset, 0, drawText.Length);
        int textMax = Math.Clamp(_textOffset + size.x, 0, drawText.Length);
        renderer.DrawText(position, drawTextSpan[textMin..textMax],
            _ => new Formatting(Color.white, Color.transparent, style, RenderOptions.Default, effect));
    }

    protected override void DrawCharacter(int x, int y, Color backgroundColor, Color foregroundColor) {
        Vector2Int position = new(this.position.x + x, this.position.y + y);
        RenderCharacter character = renderer.GetCharacter(position);
        char characterCharacter = character.character;

        RenderStyle style = this.style;
        if(typing && x == cursor - _textOffset && (int)(_lastClock.time - _lastTypeTime).TotalSeconds % 2 == 0) {
            style |= RenderStyle.Underline;
            if(!renderer.IsCharacterDrawable(characterCharacter, style))
                characterCharacter = ' ';
        }

        if(usePlaceholder)
            foregroundColor = new Color(foregroundColor.r, foregroundColor.g, foregroundColor.b,
                foregroundColor.a * 0.5f);

        character = new RenderCharacter(characterCharacter, backgroundColor, foregroundColor, style);
        renderer.DrawCharacter(position, character, RenderOptions.Default, effect);
    }

    protected override void Click() {
        base.Click();
        StartTyping();
    }

    private void KeyDown(object? called, IInput.KeyDownEventArgs args) {
        switch(args.key) {
            case KeyCode.Delete:
                EraseRight();
                _lastTypeTime = _lastClock.time;
                break;
            case KeyCode.Left:
                cursor = Math.Clamp(cursor - 1, 0, value?.Length ?? 0);
                _lastTypeTime = _lastClock.time;
                break;
            case KeyCode.Right:
                cursor = Math.Clamp(cursor + 1, 0, value?.Length ?? 0);
                _lastTypeTime = _lastClock.time;
                break;
        }
    }

    private void Type(object? caller, IInput.TextEnteredEventArgs args) {
        foreach(char character in args.text)
            switch(character) {
                case '\b':
                    EraseLeft();
                    break;
                // ^C
                case '\u0003':
                    Copy();
                    break;
                // ^V
                case '\u0016':
                    Paste();
                    break;
                // ^X
                case '\u0018':
                    Cut();
                    break;
                default:
                    TypeDrawable(character);
                    break;
            }
        _lastTypeTime = _lastClock.time;
    }

    private void Copy() {
        input.clipboard = value ?? string.Empty;
    }

    private void Paste() {
        foreach(char character in input.clipboard)
            TypeDrawable(character);
    }

    private void Cut() {
        Copy();
        EraseAll();
    }

    private void TypeDrawable(char character) {
        if(renderer.IsCharacterDrawable(character, RenderStyle.None) || character == ' ')
            Type(character);
    }

    private void Type(char character) {
        PlaySound(audio, typeSound, TypeSoundId);
        ReadOnlySpan<char> textSpan = value.AsSpan();
        ReadOnlySpan<char> textLeft = cursor <= 0 ? ReadOnlySpan<char>.Empty : textSpan[..cursor];
        ReadOnlySpan<char> textRight = cursor >= textSpan.Length ? ReadOnlySpan<char>.Empty : textSpan[cursor..];
        value = $"{textLeft}{character}{textRight}";
        cursor++;
    }

    private void EraseLeft() {
        PlaySound(audio, eraseSound, EraseSoundId);
        if(cursor <= 0)
            return;
        ReadOnlySpan<char> textSpan = value.AsSpan();
        ReadOnlySpan<char> textLeft = cursor <= 1 ? ReadOnlySpan<char>.Empty : textSpan[..(cursor - 1)];
        ReadOnlySpan<char> textRight = cursor >= textSpan.Length ? ReadOnlySpan<char>.Empty : textSpan[cursor..];
        value = $"{textLeft}{textRight}";
        cursor--;
    }

    private void EraseRight() {
        PlaySound(audio, eraseSound, EraseSoundId);
        if(cursor >= (value?.Length ?? 0))
            return;
        ReadOnlySpan<char> textSpan = value.AsSpan();
        ReadOnlySpan<char> textLeft = cursor <= 0 ? ReadOnlySpan<char>.Empty : textSpan[..cursor];
        ReadOnlySpan<char> textRight =
            cursor >= textSpan.Length - 1 ? ReadOnlySpan<char>.Empty : textSpan[(cursor + 1)..];
        value = $"{textLeft}{textRight}";
    }

    private void EraseAll() {
        PlaySound(audio, eraseSound, EraseSoundId);
        value = null;
        cursor = 0;
    }
}
