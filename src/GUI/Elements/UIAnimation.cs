using System;

using MoonSharp.Interpreter;

using PRR;

using SFML.System;

namespace PPR.GUI.Elements {
    public struct UIAnimation {
        public string id;
        public Func<float, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>> animation;
        public float time;
        public bool? endState;
        public Closure endCallback;
        
        public UIAnimation(string id, Func<float, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>> animation,
            float time, bool? endState, Closure endCallback) {
            this.id = id;
            this.animation = animation;
            this.time = time;
            this.endState = endState;
            this.endCallback = endCallback;
        }
    }
}
