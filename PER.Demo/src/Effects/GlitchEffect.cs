using System;
using System.Collections.Generic;

using PER.Abstractions.Renderer;
using PER.Util;

namespace PER.Demo.Effects {
    public class GlitchEffect : IEffect {
        public IEnumerable<PipelineStep> pipeline => null;
        public bool hasModifiers => true;
        public bool ended => false;

        private Random _random = new();

        public (Vector2, RenderCharacter) ApplyModifiers(Vector2 position, RenderCharacter character) {
            position = new Vector2(position.x + _random.Next(-100, 100) / 1000f, position.y);
            return (position, character);
        }
    }
}
