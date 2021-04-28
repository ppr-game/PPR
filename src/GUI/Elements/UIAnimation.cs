using System;

using MoonSharp.Interpreter;

using PRR;

using SFML.System;

namespace PPR.GUI.Elements {
    public class UIAnimation {
        public DynValue id { get; }
        private float time { get; }
        public bool endState { get; }
        public Closure endCallback { get; }
        
        public Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> animationModifier =>
            playing ? _animation(_currentTime / time) : null;

        private bool isFinite => time > 0f;
        private bool playing => (!isFinite || _currentTime < time) && _animation != null;
        public bool stopped => isFinite && _currentTime >= time;
        
        private Func<float, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>> _animation;

        private DateTime _startTime;
        private float _currentTime = -1f;
        
        public UIAnimation(string id, Func<float, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>> animation,
            float time, bool endState, Closure endCallback) {
            this.id = DynValue.NewString(id);
            _animation = animation;
            this.time = time;
            this.endState = endState;
            this.endCallback = endCallback;
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
