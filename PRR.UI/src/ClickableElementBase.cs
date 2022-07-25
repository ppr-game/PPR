using JetBrains.Annotations;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

[PublicAPI]
public abstract class ClickableElementBase : Element {
    public enum State { None, Inactive, Idle, FakeHovered, Hovered, FakeClicked, Clicked, Hotkey }
    public const string ClickSoundId = "buttonClick";

    protected abstract string type { get; }

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

    protected const float MinSpeed = 3f;
    protected const float MaxSpeed = 5f;

    private bool _clickLocked;
    private Vector2Int _clickPosition;

    private float[,] _animSpeeds = new float[0, 0];
    private TimeSpan _animStartTime;
    private Color _animBackgroundColorStart;
    private Color _animBackgroundColorEnd;
    private Color _animForegroundColorStart;
    private Color _animForegroundColorEnd;
    private bool _toggled;
    private bool _toggledChanged;
    private TimeSpan _lastTime;

    protected ClickableElementBase(IRenderer renderer, IInput input, IAudio? audio = null) : base(renderer) {
        this.input = input;
        this.audio = audio;
    }

    protected virtual void UpdateState(TimeSpan time) {
        State prevState = currentState;

        bool mouseClicked = input.MouseButtonPressed(MouseButton.Left);

        if(mouseClicked && !_clickLocked)
            _clickPosition = input.mousePosition;
        _clickLocked = mouseClicked;

        bool mouseOver = (_clickLocked ? _clickPosition : input.mousePosition).InBounds(bounds);
        bool mouseWasOver = bounds.IntersectsLine(input.previousMousePosition, input.mousePosition) ||
            _clickLocked && mouseOver;

        State clickedState = mouseOver ? State.Clicked : State.FakeClicked;
        State hoveredState = mouseOver ? State.Hovered : State.FakeHovered;
        State overState = mouseClicked ? clickedState : hoveredState;

        currentState = active ? hotkeyPressed ? State.Hotkey : mouseWasOver ? overState : State.Idle : State.Inactive;

        if(currentState != prevState || _toggledChanged)
            StateChanged(time, prevState, currentState);

        _toggledChanged = false;
        _lastTime = time;
    }

    private void StateChanged(TimeSpan time, State from, State to) {
        bool instant = from == State.None;

        ExecuteStateChangeActions(from, to);

        switch(to) {
            case State.Inactive:
                StartAnimation(time, _toggled ? inactiveColor : idleColor,
                    _toggled ? idleColor : inactiveColor, instant);
                break;
            case State.Idle:
                StartAnimation(time, _toggled ? clickColor : idleColor, _toggled ? idleColor : hoverColor, instant);
                break;
            case State.FakeHovered:
            case State.Hovered:
                StartAnimation(time, hoverColor, idleColor, instant);
                break;
            case State.FakeClicked:
            case State.Clicked:
                StartAnimation(time, clickColor, idleColor, instant);
                break;
        }
    }

    private void ExecuteStateChangeActions(State from, State to) {
        switch(to) {
            case State.Idle:
                if(from == State.Hotkey)
                    Click();
                break;
            case State.Hovered:
                if(from is State.Clicked or State.Hotkey)
                    Click();
                else
                    onHover?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    protected virtual void Click() {
        PlaySound(audio, clickSound, ClickSoundId);
        onClick?.Invoke(this, EventArgs.Empty);
    }

    private void StartAnimation(TimeSpan time, Color background, Color foreground, bool instant) {
        for(int y = 0; y < size.y; y++)
            for(int x = 0; x < size.x; x++)
                _animSpeeds[y, x] = Random.Shared.NextSingle(MinSpeed, MaxSpeed);
        _animStartTime = time;
        _animBackgroundColorStart = instant ? background : _animBackgroundColorEnd;
        _animBackgroundColorEnd = background;
        _animForegroundColorStart = instant ? foreground : _animForegroundColorEnd;
        _animForegroundColorEnd = foreground;
    }

    public override void Update(TimeSpan time) {
        if(!enabled) {
            currentState = State.None;
            return;
        }

        UpdateState(time);
        CustomUpdate(time);

        float animTime = (float)(time - _animStartTime).TotalSeconds;
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

    protected abstract void CustomUpdate(TimeSpan time);

    public override void UpdateColors(Dictionary<string, Color> colors, string layoutName, string id, string? special) {
        if(TryGetColor(colors, type, layoutName, id, "inactive", special, out Color color) ||
            TryGetColor(colors, "clickable", layoutName, id, "inactive", special, out color))
            inactiveColor = color;
        if(TryGetColor(colors, type, layoutName, id, "idle", special, out color) ||
            TryGetColor(colors, "clickable", layoutName, id, "idle", special, out color))
            idleColor = color;
        if(TryGetColor(colors, type, layoutName, id, "hover", special, out color) ||
            TryGetColor(colors, "clickable", layoutName, id, "hover", special, out color))
            hoverColor = color;
        if(TryGetColor(colors, type, layoutName, id, "click", special, out color) ||
            TryGetColor(colors, "clickable", layoutName, id, "click", special, out color))
            clickColor = color;
        // if UpdateColors is called from some update function,
        // _lastTime will be time at the current frame (or at least the last frame)
        // otherwise, it doesn't really matter
        StateChanged(_lastTime, currentState, currentState);
    }
}
