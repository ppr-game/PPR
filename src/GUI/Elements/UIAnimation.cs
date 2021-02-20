using System;

using PRR;

using SFML.System;

namespace PPR.GUI.Elements {
    public struct UIAnimation {
        public Func<float, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>> animation;
        public float delay;
        public float time;
        public bool? endState;
        
        public UIAnimation(Func<float, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>> animation,
            float delay, float time, bool? endState) {
            this.animation = animation;
            this.delay = delay;
            this.time = time;
            this.endState = endState;
        }
    }
}
