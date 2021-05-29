using System;
using System.Collections.Generic;

using MoonSharp.Interpreter;

using PPR.Main;

using PRR;

using SFML.System;

namespace PPR.UI.Animations {
    public class Animation {
        public string id { get; }
        public DynValue dynValueId { get; }
        public float time { get; }
        public bool endState { get; }
        public Closure endCallback { get; }
        public Dictionary<string, double> args { get; }
        
        [MoonSharpHidden]
        public Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> animationModifier =>
            playing ? _animation(_currentTime / time, args) : null;

        public IReadOnlyStopwatch stopwatch => _stopwatch;
        public float currentTime => _currentTime;

        public double speed {
            get => _stopwatch.speed;
            set => _stopwatch.speed = value;
        }
        
        public bool started { get; private set; }

        public bool isFinite => time > 0f;
        public bool playing => (!isFinite || _currentTime >= 0f && _currentTime < time) && _animation != null;

        private Func<float, Dictionary<string, double>, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>>
            _animation;

        private float _currentTime;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        [MoonSharpHidden]
        public Animation(string id,
            Func<float, Dictionary<string, double>, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>>
                animation, float time, bool endState, Closure endCallback, Dictionary<string, double> args) {
            this.id = id;
            dynValueId = DynValue.NewString(id);
            _animation = animation;
            this.time = time;
            this.endState = endState;
            this.endCallback = endCallback;
            this.args = args;
        }

        public void Restart(float startTime = 0f) {
            if(_animation == null) return;
            _stopwatch.Reset(TimeSpan.FromSeconds(startTime).Ticks);
            started = true;
        }

        [MoonSharpHidden]
        public void Stop() => _animation = null;

        [MoonSharpHidden]
        public void Update() => _currentTime = (float)_stopwatch.time.TotalSeconds;
    }
}
