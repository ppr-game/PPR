using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

public class Slider : Element {

    public enum State { None, Inactive, Idle, Hovered, Clicked }

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
            onValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public float minValue { get; set; }
    public float maxValue { get; set; }

    public bool active { get; set; } = true;

    public Color inactiveColor { get; set; } = Color.black;
    public Color inactiveToggledColor { get; set; } = new(0.1f, 0.1f, 0.1f, 1f);
    public Color idleColor { get; set; } = Color.black;
    public Color hoverColor { get; set; } = Color.white;
    public Color clickColor { get; set; } = new(0.4f, 0.4f, 0.4f, 1f);

    public event EventHandler? onValueChanged;

    public State currentState { get; private set; } = State.None;

    private const float MinSpeed = 3f;
    private const float MaxSpeed = 5f;

    private int _width;

    private int _relativeValue;
    private float _value;

    private float[] _animSpeeds = Array.Empty<float>();
    private TimeSpan _animStartTime;
    private Color _animBackgroundColorStart;
    private Color _animBackgroundColorEnd;
    private Color _animForegroundColorStart;
    private Color _animForegroundColorEnd;

    public Slider(IRenderer renderer) : base(renderer) { }

    private void UpdateState(IReadOnlyStopwatch clock) {
        bool mouseOver = renderer.input?.mousePosition.InBounds(bounds) ?? false;
        bool mouseClicked = renderer.input?.MouseButtonPressed(MouseButton.Left) ?? false;
        State prevState = currentState;
        currentState = active ? mouseOver ? mouseClicked ? State.Clicked : State.Hovered : State.Idle : State.Inactive;
        if(currentState != prevState) StateChanged(clock, prevState, currentState);
        if(currentState == State.Clicked) UpdateValue();
    }

    private void StateChanged(IReadOnlyStopwatch clock, State from, State to) {
        if(from == State.None) {
            _animBackgroundColorEnd = Color.transparent;
            _animForegroundColorEnd = Color.transparent;
        }

        switch(to) {
            case State.Inactive:
                StartAnimation(clock, inactiveColor, inactiveToggledColor);
                break;
            case State.Idle:
                StartAnimation(clock, idleColor, hoverColor);
                break;
            case State.Hovered:
                StartAnimation(clock, hoverColor, idleColor);
                break;
            case State.Clicked:
                StartAnimation(clock, clickColor, idleColor);
                break;
        }
    }

    private void UpdateValue() {
        int prevRelativeValue = _relativeValue;
        _relativeValue = renderer.input?.mousePosition.x - position.x ?? 0;
        if(prevRelativeValue == _relativeValue) return;
        float tempValue = (float)_relativeValue / (width - 1);
        tempValue = minValue + tempValue * (maxValue - minValue);
        value = Math.Min(Math.Max(tempValue, minValue), maxValue);
    }

    private void StartAnimation(IReadOnlyStopwatch clock, Color background, Color foreground) {
        for(int x = 0; x < width; x++)
            _animSpeeds[x] = Random.Shared.NextSingle(MinSpeed, MaxSpeed);
        _animStartTime = clock.time;
        _animBackgroundColorStart = _animBackgroundColorEnd;
        _animBackgroundColorEnd = background;
        _animForegroundColorStart = _animForegroundColorEnd;
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
