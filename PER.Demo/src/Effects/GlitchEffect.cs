using System;
using System.Collections.Generic;

using PER.Abstractions.Renderer;
using PER.Util;

namespace PER.Demo.Effects;

public class GlitchEffect : IEffect {
    public IEnumerable<PipelineStep>? pipeline => null;
    public bool hasModifiers => true;
    public bool drawable => true;

    private static readonly Random random = new();

    private bool _draw = true;

    public (Vector2, RenderCharacter) ApplyModifiers(Vector2 position, RenderCharacter character) {
        position = new Vector2(position.x + RandomFloat() * 0.1f, position.y);
        string mappings = Core.engine.renderer.font?.mappings ?? "";
        character = new RenderCharacter(
            RandomPositiveFloat() <= 0.98f ? character.character : mappings[random.Next(0, mappings.Length)],
            RandomizeColor(character.background), RandomizeColor(character.foreground),
            RandomPositiveFloat() <= 0.95f ? character.style :
                (RenderStyle)random.Next((int)RenderStyle.None, (int)RenderStyle.All));
        return (position, character);
    }

    public void Update(bool fullscreen) { }

    public void Draw(Vector2Int position) {
        if(position.x % random.Next(3, 10) == 0 || position.y % random.Next(3, 10) == 0)
            _draw = RandomPositiveFloat() > 0.95f;
        if(!_draw || !Core.engine.renderer.IsCharacterEmpty(position)) return;
        string mappings = Core.engine.renderer.font?.mappings ?? "";
        Core.engine.renderer.DrawCharacter(position, new RenderCharacter(
            mappings[random.Next(0, mappings.Length)],
            RandomizeColor(Color.transparent), RandomizeColor(Color.white),
            (RenderStyle)random.Next((int)RenderStyle.None, (int)RenderStyle.All)));
    }

    private static float RandomFloat() => random.Next(-100000, 100000) / 100000f;
    private static float RandomPositiveFloat() => random.Next(0, 100000) / 100000f;
    private static float RandomColorComponent(float current) => current + RandomFloat() * 0.3f;

    private static Color RandomizeColor(Color current) => RandomPositiveFloat() <= 0.98f ?
        new Color(RandomColorComponent(current.r), RandomColorComponent(current.g),
            RandomColorComponent(current.b), RandomColorComponent(current.a)) : RandomColor();

    private static Color RandomColor() => random.Next(0, 2) == 0 ? new Color(1f, 0f, 0f, 1f) :
        random.Next(0, 2) == 0 ? new Color(0f, 1f, 0f, 1f) : new Color(0f, 0f, 1f, 1f);
}
