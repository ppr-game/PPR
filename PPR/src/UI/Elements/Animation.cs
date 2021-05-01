using System;
using System.Collections.Generic;

using MoonSharp.Interpreter;

using PRR;

using SFML.System;

namespace PPR.UI.Elements {
    public class Animation {
        public DynValue id { get; }
        public float time { get; }
        public bool endState { get; }
        public Closure endCallback { get; }
        public Dictionary<string, double> args { get; }
        
        [MoonSharpHidden]
        public Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> animationModifier =>
            playing ? _animation(_currentTime / time, args) : null;

        public float currentTime => _currentTime;

        public bool isFinite => time > 0f;
        public bool playing => (!isFinite || _currentTime < time) && _animation != null;
        public bool stopped => isFinite && _currentTime >= time;
        
        private Func<float, Dictionary<string, double>, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>>
            _animation;

        private DateTime _startTime;
        private float _currentTime = -1f;
        
        public Animation(string id,
            Func<float, Dictionary<string, double>, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>>
                animation, float time, bool endState, Closure endCallback, Dictionary<string, double> args) {
            this.id = DynValue.NewString(id);
            _animation = animation;
            this.time = time;
            this.endState = endState;
            this.endCallback = endCallback;
            this.args = args;
        }

        public void Start() {
            _currentTime = 0f;
            _startTime = DateTime.UtcNow;
        }

        public void Stop() => _animation = null;

        public void Update() {
            if(_currentTime >= 0f) _currentTime = (float)(DateTime.UtcNow - _startTime).TotalSeconds;
        }
    }
}
