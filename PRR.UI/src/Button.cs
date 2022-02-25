﻿using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

public class Button : Element {
    public enum State { None, Inactive, Idle, Hovered, Clicked, Hotkey }

    public override Vector2Int size {
        get => _size;
        set {
            _size = value;
            _animSpeeds = new float[_size.y, _size.x];
        }
    }

    public KeyCode? hotkey { get; set; }

    public string? text {
        set => lines = value?.Split('\n');
    }
    public string[]? lines { get; set; }
    public RenderStyle style { get; set; } = RenderStyle.None;

    public bool active { get; set; } = true;
    public bool toggled { get; set; }

    public Color inactiveColor { get; set; } = Color.black;
    public Color inactiveToggledColor { get; set; } = new(0.1f, 0.1f, 0.1f, 1f);
    public Color idleColor { get; set; } = Color.black;
    public Color hoverColor { get; set; } = Color.white;
    public Color clickColor { get; set; } = new(0.4f, 0.4f, 0.4f, 1f);

    public event EventHandler? onClick;

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

    public Button(IRenderer renderer) : base(renderer) { }

    private void UpdateState(IReadOnlyStopwatch clock) {
        bool mouseOver = renderer.input?.mousePosition.InBounds(bounds) ?? false;
        bool mouseClicked = renderer.input?.MouseButtonPressed(MouseButton.Left) ?? false;
        bool hotkeyPressed = hotkey.HasValue && (renderer.input?.KeyPressed(hotkey.Value) ?? false);
        State prevState = currentState;
        currentState = active ? hotkeyPressed ? State.Hotkey :
            mouseOver ? mouseClicked ? State.Clicked : State.Hovered : State.Idle : State.Inactive;
        if(currentState != prevState) StateChanged(clock, prevState, currentState);
    }

    private void StateChanged(IReadOnlyStopwatch clock, State from, State to) {
        if(from == State.None) {
            _animBackgroundColorEnd = Color.transparent;
            _animForegroundColorEnd = Color.transparent;
        }

        switch(to) {
            case State.Inactive:
                StartAnimation(clock, toggled ? inactiveToggledColor : inactiveColor,
                    toggled ? inactiveColor : inactiveToggledColor);
                break;
            case State.Idle:
                if(from == State.Hotkey) onClick?.Invoke(this, EventArgs.Empty);
                StartAnimation(clock, toggled ? clickColor : idleColor, toggled ? idleColor : hoverColor);
                break;
            case State.Hovered:
                if(from is State.Clicked or State.Hotkey) onClick?.Invoke(this, EventArgs.Empty);
                StartAnimation(clock, hoverColor, idleColor);
                break;
            case State.Clicked:
                StartAnimation(clock, clickColor, idleColor);
                break;
        }
    }

    private void StartAnimation(IReadOnlyStopwatch clock, Color background, Color foreground) {
        for(int y = 0; y < size.y; y++)
            for(int x = 0; x < size.x; x++)
                _animSpeeds[y, x] = Random.Shared.NextSingle(MinSpeed, MaxSpeed);
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

        if(lines is not null)
            renderer.DrawText(center, lines, Color.white, Color.transparent,
                HorizontalAlignment.Middle, style, RenderOptions.Default, effect);

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
