using System;
using System.Collections.Generic;

using PER.Abstractions.Renderer;
using PER.Util;

namespace PER.Demo.Effects {
    public class GlitchEffect : IEffect {
        public IEnumerable<PipelineStep> pipeline => null;
        public bool hasModifiers => true;
        public bool ended => false;

        private static readonly Random random = new();

        public (Vector2, RenderCharacter) ApplyModifiers(Vector2 position, RenderCharacter character) {
            position = new Vector2(position.x + RandomFloat() * 0.1f, position.y);
            string mappings = Core.engine.renderer.font.mappings;
            character = new RenderCharacter(
                RandomFloat() <= 0.95f ? character.character : mappings[random.Next(0, mappings.Length)], new Color(
                    character.background.r + RandomFloat() * 0.5f, character.background.g + RandomFloat() * 0.5f,
                    character.background.b + RandomFloat() * 0.5f, character.background.a + RandomFloat() * 0.5f),
                new Color(
                    character.foreground.r + RandomFloat() * 0.5f, character.foreground.g + RandomFloat() * 0.5f,
                    character.foreground.b + RandomFloat() * 0.5f, character.foreground.a + RandomFloat() * 0.5f),
                RandomFloat() <= 0.9f ? character.style :
                    (RenderStyle)random.Next((int)RenderStyle.None, (int)RenderStyle.All));
            return (position, character);
        }

        private static float RandomFloat() => random.Next(-100000, 100000) / 100000f;
    }
}
