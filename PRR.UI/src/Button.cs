using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

public class Button : Element {
    public enum State { None, Inactive, Idle, FakeHovered, Hovered, Clicked, Hotkey }
    public const string ClickSoundId = "buttonClick";

    public IInput input { get; set; }
    public IAudio? audio { get; set; }

    public override Vector2Int size {
        get => _size;
        set {
            _size = value;
            _animSpeeds = new float[_size.y, _size.x];
        }
    }

    public KeyCode? hotkey { get; set; }

    public string? text { get; set; }
    public RenderStyle style { get; set; } = RenderStyle.None;

    public bool active { get; set; } = true;
    public bool toggled { get; set; }

    public Color inactiveColor { get; set; } = new(0.1f, 0.1f, 0.1f, 1f);
    public Color idleColor { get; set; } = Color.black;
    public Color hoverColor { get; set; } = Color.white;
    public Color clickColor { get; set; } = new(0.4f, 0.4f, 0.4f, 1f);

    public IPlayable? clickSound { get; set; }
    public event EventHandler? onClick;
    public event EventHandler? onHover;

    public State currentState { get; private set; } = State.None;

    private const float MinSpeed = 3f;
    private const float MaxSpeed = 5f;

    private Vector2Int _size;

    private float[,] _animSpeeds = new float[0, 0];
    private TimeSpan _animStartTime;
    private Color _animBackgroundColorStart;
    private Color _animBackgroundColorEnd;
    private Color _animForegroundColorStart;
    private Color _animForegroundColorEnd;

    public Button(IRenderer renderer, IInput input, IAudio? audio = null) : base(renderer) {
        this.input = input;
        this.audio = audio;
    }

    private void UpdateState(IReadOnlyStopwatch clock) {
        bool mouseWasOver = bounds.IntersectsLine(input.previousMousePosition, input.mousePosition);
        bool mouseOver = input.mousePosition.InBounds(bounds);
        bool mouseClicked = input.MouseButtonPressed(MouseButton.Left);
        bool hotkeyPressed = hotkey.HasValue && input.KeyPressed(hotkey.Value);
        State prevState = currentState;
        currentState = active ? hotkeyPressed ? State.Hotkey :
            mouseWasOver ? mouseOver ? mouseClicked ?
                State.Clicked : State.Hovered : State.FakeHovered : State.Idle : State.Inactive;
        if(currentState != prevState) StateChanged(clock, prevState, currentState);
    }

    private void StateChanged(IReadOnlyStopwatch clock, State from, State to) {
        bool instant = from == State.None;

        switch(to) {
            case State.Inactive:
                StartAnimation(clock, toggled ? inactiveColor : idleColor,
                    toggled ? idleColor : inactiveColor, instant);
                break;
            case State.Idle:
                if(from == State.Hotkey) Click();
                StartAnimation(clock, toggled ? clickColor : idleColor, toggled ? idleColor : hoverColor, instant);
                break;
            case State.FakeHovered:
                StartAnimation(clock, hoverColor, idleColor, instant);
                break;
            case State.Hovered:
                if(from is State.Clicked or State.Hotkey) Click();
                else onHover?.Invoke(this, EventArgs.Empty);
                StartAnimation(clock, hoverColor, idleColor, instant);
                break;
            case State.Clicked:
                StartAnimation(clock, clickColor, idleColor, instant);
                break;
        }
    }

    private void Click() {
        if(clickSound is not null) clickSound.status = PlaybackStatus.Playing;
        else if(audio is not null && audio.TryGetPlayable(ClickSoundId, out IPlayable? playable))
            playable.status = PlaybackStatus.Playing;
        onClick?.Invoke(this, EventArgs.Empty);
    }

    private void StartAnimation(IReadOnlyStopwatch clock, Color background, Color foreground, bool instant) {
        for(int y = 0; y < size.y; y++)
            for(int x = 0; x < size.x; x++)
                _animSpeeds[y, x] = Random.Shared.NextSingle(MinSpeed, MaxSpeed);
        _animStartTime = clock.time;
        _animBackgroundColorStart = instant ? background : _animBackgroundColorEnd;
        _animBackgroundColorEnd = background;
        _animForegroundColorStart = instant ? foreground : _animForegroundColorEnd;
        _animForegroundColorEnd = foreground;
    }

    public override void Update(IReadOnlyStopwatch clock) {
        if(!enabled) {
            currentState = State.None;
            return;
        }

        UpdateState(clock);

        if(text is not null)
            renderer.DrawText(center, text,
                _ => new Formatting(Color.white, Color.transparent, style, RenderOptions.Default, effect),
                HorizontalAlignment.Middle);

        float animTime = (float)(clock.time - _animStartTime).TotalSeconds;
        for(int y = 0; y < size.y; y++){
            for(int x = 0; x < size.x; x++) {
                Vector2Int position = new(this.position.x + x, this.position.y + y);
                RenderCharacter character = renderer.GetCharacter(position);

                float t = animTime * _animSpeeds[y, x];
                Color backgroundColor = Color.LerpColors(_animBackgroundColorStart, _animBackgroundColorEnd, t);
                Color foregroundColor = Color.LerpColors(_animForegroundColorStart, _animForegroundColorEnd, t);

                character = new RenderCharacter(character.character, backgroundColor, foregroundColor, style);
                renderer.DrawCharacter(position, character, RenderOptions.Default, effect);
            }
        }
    }
}
