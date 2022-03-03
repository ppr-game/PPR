using PER.Abstractions.Renderer;
using PER.Util;

namespace PER.Common.Effects;

public class FadeEffect : IEffect {
    private enum State { None, Out, In }

    public IEnumerable<PipelineStep>? pipeline => null;
    public bool hasModifiers => true;
    public bool drawable => false;

    public bool fading => _state != State.None;

    private State _state;
    private float _t;
    private float _outTime;
    private float _inTime;
    private Action? _callback;
    private readonly Stopwatch _stopwatch = new();

    public void Start(float outTime, float inTime, Action middleCallback) {
        _outTime = outTime;
        _inTime = inTime;
        _callback = middleCallback;
        _state = State.Out;
        _t = 0f;
        _stopwatch.Reset();
    }

    public (Vector2, RenderCharacter) ApplyModifiers(Vector2 position, RenderCharacter character) {
        float t = _t;
        if(_state == State.Out) t = 1f - t;
        character = new RenderCharacter(character.character,
            new Color(character.background.r, character.background.g, character.background.b,
                Lerp(0f, character.background.a, t)),
            new Color(character.foreground.r, character.foreground.g, character.foreground.b,
                Lerp(0f, character.foreground.a, t)),
            character.style);
        return (position, character);
    }

    public void Update(bool fullscreen) {
        if(_t >= 1f) {
            switch(_state) {
                case State.Out:
                    _callback?.Invoke();
                    _state = State.In;
                    _stopwatch.Reset();
                    break;
                case State.In:
                    _state = State.None;
                    break;
            }
        }

        float time = _state switch {
            State.Out => _outTime,
            State.In => _inTime,
            _ => 0f
        };
        _t = (float)_stopwatch.time.TotalSeconds / time;
    }

    public void Draw(Vector2Int position) { }

    private static float Lerp(float a, float b, float t) => MathF.Min(MathF.Max(a + (b - a) * t, 0f), 1f);
}
