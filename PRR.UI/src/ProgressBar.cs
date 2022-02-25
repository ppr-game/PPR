using PER.Abstractions.Renderer;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

public class ProgressBar : Element {
    private struct AnimatedCharacter {
        private float speed { get; set; }
        private TimeSpan startTime { get; set; }
        private Color colorStart { get; set; }
        private Color colorEnd { get; set; }

        private const float MinSpeed = 3f;
        private const float MaxSpeed = 5f;

        public void Start(TimeSpan startTime, Color color) {
            if(colorEnd == color) return;
            speed = Random.Shared.NextSingle(MinSpeed, MaxSpeed);
            this.startTime = startTime;
            colorStart = colorEnd;
            colorEnd = color;
        }

        public Color Get(TimeSpan time) {
            float t = (float)(time - startTime).TotalSeconds * speed;
            return Color.LerpColors(colorStart, colorEnd, t);
        }
    }

    public override Vector2Int size {
        get => _size;
        set {
            _size = value;
            _anim = new AnimatedCharacter[_size.x, _size.y];
            _resized = true;
        }
    }

    public float value { get; set; }

    public Color lowColor { get; set; } = Color.black;
    public Color highColor { get; set; } = Color.white;

    private Vector2Int _size;
    private bool _resized;
    private AnimatedCharacter[,] _anim = new AnimatedCharacter[0, 0];
    private float _prevValue;

    public ProgressBar(IRenderer renderer) : base(renderer) { }

    private void Animate(IReadOnlyStopwatch clock, float from, float to, Color lowColor, Color highColor) {
        int fromX = (int)MathF.Floor(size.x * MathF.Min(MathF.Max(from, 0f), 1f));
        int toX = (int)MathF.Floor(size.x * MathF.Min(MathF.Max(to, 0f), 1f));
        if(fromX == toX) return;
        Color color = fromX < toX ? highColor : lowColor;
        int min = Math.Min(fromX, toX);
        int max = Math.Max(fromX, toX);
        for(int x = min; x < max; x++)
            for(int y = 0; y < size.y; y++)
                _anim[x, y].Start(clock.time, color);
    }

    public override void Update(IReadOnlyStopwatch clock) {
        if(!enabled) return;

        if(value != _prevValue) Animate(clock, _prevValue, value, lowColor, highColor);
        else if(_resized) {
            Animate(clock, value, value, lowColor, highColor);
            _resized = false;
        }
        _prevValue = value;

        for(int x = 0; x < size.x; x++)
            for(int y = 0; y < size.y; y++)
                renderer.DrawCharacter(new Vector2Int(position.x + x, position.y + y),
                    new RenderCharacter('\0', _anim[x, y].Get(clock.time), Color.transparent), RenderOptions.Default,
                    effect);
    }
}
