using JetBrains.Annotations;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

[PublicAPI]
public class InputField : ClickableElementBase {
    protected override string type => "inputField";

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

    public override Vector2Int size {
        get => base.size;
        set {
            base.size = value;
            _animSpeeds = new float[value.y, value.x];
            _animStartTimes = new TimeSpan[value.y, value.x];
        }
    }

    public string? value {
        get => _value;
        set {
            _value = value;
            onTextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string? placeholder { get; set; }
    public RenderStyle style { get; set; } = RenderStyle.None;

    public bool wrap { get; set; }

    public int cursor {
        get => _cursor;
        set {
            int textLength = this.value?.Length ?? 0;
            _cursor = Math.Clamp(value, 0, textLength);

            int useTextOffset = wrap ? _textOffset / size.x : _textOffset;
            int useCursor = wrap ? _cursor / size.x : _cursor;
            int useSize = wrap ? size.y - 1 : size.x;
            int useTextLength = wrap ? (textLength - 1) / size.x : textLength;

            useTextOffset = Math.Clamp(useTextOffset, useCursor - useSize, useCursor);
            useTextOffset = Math.Clamp(useTextOffset, 0, Math.Max(useTextLength - useSize, 0));

            _textOffset = wrap ? useTextOffset * size.x : useTextOffset;
        }
    }

    public float blinkRate { get; set; } = 1f;

    public IPlayable? typeSound { get; set; }
    public IPlayable? eraseSound { get; set; }
    public IPlayable? submitSound { get; set; }

    public event EventHandler? onStartTyping;
    public event EventHandler? onTextChanged;
    public event EventHandler? onSubmit;

    protected override bool hotkeyPressed => false;

    private bool typing => enabled && active && toggledSelf;
    private bool usePlaceholder => string.IsNullOrEmpty(value) && !typing;

    private Vector2Int cursorPos {
        get {
            int cursor = _cursor - _textOffset;
            return new Vector2Int(wrap ? cursor % size.x : cursor, wrap ? cursor / size.x : 0);
        }
    }

    private string? _value;
    private int _cursor;
    private int _textOffset;

    private TimeSpan _lastTime;
    private TimeSpan _lastTypeTime;

    private float[,] _animSpeeds = new float[0, 0];
    private TimeSpan[,] _animStartTimes = new TimeSpan[0, 0];

    public InputField(IRenderer renderer, IInput input, IAudio? audio = null) : base(renderer, input, audio) { }

    public override Element Clone() => throw new NotImplementedException();

    protected override void UpdateState(TimeSpan time) {
        base.UpdateState(time);
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

    protected override void CustomUpdate(TimeSpan time) {
        _lastTime = time;
        string? drawText = usePlaceholder ? placeholder : value;
        if(drawText is null)
            return;
        ReadOnlySpan<char> drawTextSpan = drawText.AsSpan();
        int textMin = Math.Clamp(_textOffset, 0, drawText.Length);
        int textMax = Math.Clamp(_textOffset + size.x * (wrap ? size.y : 1), 0, drawText.Length);
        renderer.DrawText(position, drawTextSpan[textMin..textMax],
            _ => new Formatting(Color.white, Color.transparent, style, RenderOptions.Default, effect),
            HorizontalAlignment.Left, wrap ? size.x : 0);
    }

    protected override void DrawCharacter(int x, int y, Color backgroundColor, Color foregroundColor) {
        Vector2Int position = new(this.position.x + x, this.position.y + y);
        RenderCharacter character = renderer.GetCharacter(position);
        char characterCharacter = character.character;

        Vector2Int cursor = cursorPos;

        bool isCursor = typing && x == cursor.x && y == cursor.y &&
            (_lastTime - _lastTypeTime).TotalSeconds * blinkRate % 1d <= 0.5d;

        RenderStyle style = this.style;
        if(isCursor) {
            style |= RenderStyle.Underline;
            if(!renderer.IsCharacterDrawable(characterCharacter, style))
                characterCharacter = ' ';
        }

        float speed = _animSpeeds[y, x];
        float t = speed == 0f ? 1f : (float)(_lastTime - _animStartTimes[y, x]).TotalSeconds * speed;
        foregroundColor = new Color(foregroundColor.r, foregroundColor.g, foregroundColor.b,
            MoreMath.Lerp(0f, foregroundColor.a, t) * (usePlaceholder ? 0.5f : 1f));

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
                _lastTypeTime = _lastTime;
                break;
            case KeyCode.Left:
                cursor = Math.Clamp(cursor - 1, 0, value?.Length ?? 0);
                _lastTypeTime = _lastTime;
                Animate();
                break;
            case KeyCode.Right:
                cursor = Math.Clamp(cursor + 1, 0, value?.Length ?? 0);
                _lastTypeTime = _lastTime;
                Animate();
                break;
            case KeyCode.Up:
                cursor = wrap ? Math.Clamp(cursor - size.x, 0, value?.Length ?? 0) : 0;
                _lastTypeTime = _lastTime;
                Animate();
                break;
            case KeyCode.Down:
                cursor = wrap ? Math.Clamp(cursor + size.x, 0, value?.Length ?? 0) : value?.Length ?? 0;
                _lastTypeTime = _lastTime;
                Animate();
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
                    PlaySound(audio, typeSound, TypeSoundId);
                    TypeDrawable(character);
                    break;
            }
        _lastTypeTime = _lastTime;
    }

    private void Copy() {
        input.clipboard = value ?? string.Empty;
    }

    private void Paste() {
        PlaySound(audio, typeSound, TypeSoundId);
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
        ReadOnlySpan<char> textSpan = value.AsSpan();
        ReadOnlySpan<char> textLeft = cursor <= 0 ? ReadOnlySpan<char>.Empty : textSpan[..cursor];
        ReadOnlySpan<char> textRight = cursor >= textSpan.Length ? ReadOnlySpan<char>.Empty : textSpan[cursor..];
        value = $"{textLeft}{character}{textRight}";
        Animate();
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
        Animate();
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
        Animate();
    }

    private void EraseAll() {
        PlaySound(audio, eraseSound, EraseSoundId);
        value = null;
        cursor = 0;
        Animate();
    }

    private void Animate() => Animate(_lastTime, cursorPos);
    private void Animate(TimeSpan time, Vector2Int position) {
        if(position.y < 0 || position.y >= _animSpeeds.GetLength(0) ||
            position.x < 0 || position.x >= _animSpeeds.GetLength(1))
            return;
        _animSpeeds[position.y, position.x] = Random.Shared.NextSingle(MinSpeed, MaxSpeed);
        _animStartTimes[position.y, position.x] = time;
    }
}
