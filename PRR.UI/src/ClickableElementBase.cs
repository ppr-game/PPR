using JetBrains.Annotations;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

[PublicAPI]
public abstract class ClickableElementBase : Element {
    public enum State { None, Inactive, Idle, FakeHovered, Hovered, Clicked, Hotkey }
    public const string ClickSoundId = "buttonClick";

    public IInput input { get; set; }
    public IAudio? audio { get; set; }

    public override Vector2Int size {
        get => base.size;
        set {
            base.size = value;
            _animSpeeds = new float[value.y, value.x];
        }
    }

    public virtual bool active { get; set; } = true;

    public Color inactiveColor { get; set; } = new(0.1f, 0.1f, 0.1f, 1f);
    public Color idleColor { get; set; } = Color.black;
    public Color hoverColor { get; set; } = Color.white;
    public Color clickColor { get; set; } = new(0.4f, 0.4f, 0.4f, 1f);

    public IPlayable? clickSound { get; set; }
    public event EventHandler? onClick;
    public event EventHandler? onHover;

    public State currentState { get; private set; } = State.None;

    protected bool toggledSelf {
        get => _toggled;
        set {
            if(_toggled == value)
                return;
            _toggled = value;
            _toggledChanged = true;
        }
    }

    protected abstract bool hotkeyPressed { get; }

    private const float MinSpeed = 3f;
    private const float MaxSpeed = 5f;

    private float[,] _animSpeeds = new float[0, 0];
    private TimeSpan _animStartTime;
    private Color _animBackgroundColorStart;
    private Color _animBackgroundColorEnd;
    private Color _animForegroundColorStart;
    private Color _animForegroundColorEnd;
    private bool _toggled;
    private bool _toggledChanged;

    protected ClickableElementBase(IRenderer renderer, IInput input, IAudio? audio = null) : base(renderer) {
        this.input = input;
        this.audio = audio;
    }

    protected virtual void UpdateState(IReadOnlyStopwatch clock) {
        bool mouseWasOver = bounds.IntersectsLine(input.previousMousePosition, input.mousePosition);
        bool mouseOver = input.mousePosition.InBounds(bounds);
        bool mouseClicked = input.MouseButtonPressed(MouseButton.Left);
        State prevState = currentState;
        currentState = active ? hotkeyPressed ? State.Hotkey :
            mouseWasOver ? mouseOver ? mouseClicked ?
                State.Clicked : State.Hovered : State.FakeHovered : State.Idle : State.Inactive;
        if(currentState != prevState || _toggledChanged)
            StateChanged(clock, prevState, currentState);
        _toggledChanged = false;
    }

    private void StateChanged(IReadOnlyStopwatch clock, State from, State to) {
        bool instant = from == State.None;

        switch(to) {
            case State.Inactive:
                StartAnimation(clock, _toggled ? inactiveColor : idleColor,
                    _toggled ? idleColor : inactiveColor, instant);
                break;
            case State.Idle:
                if(from == State.Hotkey) Click();
                StartAnimation(clock, _toggled ? clickColor : idleColor, _toggled ? idleColor : hoverColor, instant);
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

    protected virtual void Click() {
        PlaySound(audio, clickSound, ClickSoundId);
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
        CustomUpdate(clock);

        float animTime = (float)(clock.time - _animStartTime).TotalSeconds;
        for(int y = 0; y < size.y; y++) {
            for(int x = 0; x < size.x; x++) {
                float t = animTime * _animSpeeds[y, x];
                Color backgroundColor = Color.LerpColors(_animBackgroundColorStart, _animBackgroundColorEnd, t);
                Color foregroundColor = Color.LerpColors(_animForegroundColorStart, _animForegroundColorEnd, t);

                DrawCharacter(x, y, backgroundColor, foregroundColor);
            }
        }
    }

    protected abstract void DrawCharacter(int x, int y, Color backgroundColor, Color foregroundColor);

    protected abstract void CustomUpdate(IReadOnlyStopwatch clock);
}
