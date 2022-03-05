using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

public class Slider : Element {
    public enum State { None, Inactive, Idle, Hovered, Clicked }
    public const string ValueChangedSoundId = "slider";

    public IInputManager input { get; set; }
    public IAudio? audio { get; set; }

    public override Vector2Int size {
        get => new(width, 1);
        set => width = value.x;
    }

    public int width {
        get => _width;
        set {
            _width = value;
            _animSpeeds = new float[_width];
        }
    }

    public float value {
        get => _value;
        set {
            if(_value == value) return;
            _value = value;
            float tempValue = (value - minValue) / (maxValue - minValue);
            _relativeValue = (int)(tempValue * (width - 1));
            onValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public float minValue { get; set; }
    public float maxValue { get; set; }

    public bool active { get; set; } = true;

    public Color inactiveColor { get; set; } = new(0.1f, 0.1f, 0.1f, 1f);
    public Color idleColor { get; set; } = Color.black;
    public Color hoverColor { get; set; } = Color.white;
    public Color clickColor { get; set; } = new(0.4f, 0.4f, 0.4f, 1f);

    public IPlayable? valueChangedSound { get; set; }
    public event EventHandler? onValueChanged;

    public State currentState { get; private set; } = State.None;

    private const float MinSpeed = 3f;
    private const float MaxSpeed = 5f;

    private int _width;

    private int _relativeValue;
    private float _value = float.NaN; // make first value set always work

    private float[] _animSpeeds = Array.Empty<float>();
    private TimeSpan _animStartTime;
    private Color _animBackgroundColorStart;
    private Color _animBackgroundColorEnd;
    private Color _animForegroundColorStart;
    private Color _animForegroundColorEnd;

    public Slider(IRenderer renderer, IInputManager input, IAudio? audio = null) : base(renderer) {
        this.input = input;
        this.audio = audio;
    }

    private void UpdateState(IReadOnlyStopwatch clock) {
        bool mouseWasOver = bounds.IntersectsLine(input.previousMousePosition, input.mousePosition);
        bool mouseOver = input.mousePosition.InBounds(bounds);
        bool mouseClicked = input.MouseButtonPressed(MouseButton.Left);
        State prevState = currentState;
        currentState = active ? mouseWasOver ? mouseOver && mouseClicked ?
            State.Clicked : State.Hovered : State.Idle : State.Inactive;
        if(currentState != prevState) StateChanged(clock, prevState, currentState);
        if(currentState == State.Clicked) UpdateValue();
    }

    private void StateChanged(IReadOnlyStopwatch clock, State from, State to) {
        bool instant = from == State.None;

        switch(to) {
            case State.Inactive:
                StartAnimation(clock, idleColor, inactiveColor, instant);
                break;
            case State.Idle:
                StartAnimation(clock, idleColor, hoverColor, instant);
                break;
            case State.Hovered:
                StartAnimation(clock, hoverColor, idleColor, instant);
                break;
            case State.Clicked:
                StartAnimation(clock, clickColor, idleColor, instant);
                break;
        }
    }

    private void UpdateValue() {
        int prevRelativeValue = _relativeValue;
        _relativeValue = input.mousePosition.x - position.x;
        if(prevRelativeValue == _relativeValue) return;
        float tempValue = (float)_relativeValue / (width - 1);
        tempValue = minValue + tempValue * (maxValue - minValue);
        value = Math.Min(Math.Max(tempValue, minValue), maxValue);

        if(valueChangedSound is not null) valueChangedSound.status = PlaybackStatus.Playing;
        else if(audio is not null && audio.TryGetPlayable(ValueChangedSoundId, out IPlayable? playable))
            playable.status = PlaybackStatus.Playing;
    }

    private void StartAnimation(IReadOnlyStopwatch clock, Color background, Color foreground, bool instant) {
        for(int x = 0; x < width; x++)
            _animSpeeds[x] = Random.Shared.NextSingle(MinSpeed, MaxSpeed);
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

        float animTime = (float)(clock.time - _animStartTime).TotalSeconds;
        for(int x = 0; x < width; x++) {
            Vector2Int position = new(this.position.x + x, this.position.y);

            float t = animTime * _animSpeeds[x];
            Color backgroundColor = Color.LerpColors(_animBackgroundColorStart, _animBackgroundColorEnd, t);
            Color foregroundColor = Color.LerpColors(_animForegroundColorStart, _animForegroundColorEnd, t);

            char character = x < _relativeValue ? '─' : x == _relativeValue ? '█' : '-';
            renderer.DrawCharacter(position, new RenderCharacter(character, backgroundColor, foregroundColor),
                RenderOptions.Default, effect);
        }
    }
}
