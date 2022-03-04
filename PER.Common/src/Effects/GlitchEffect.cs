using PER.Abstractions.Renderer;
using PER.Util;

namespace PER.Common.Effects;

public class GlitchEffect : IEffect {
    public IEnumerable<PipelineStep>? pipeline => null;
    public bool hasModifiers => true;
    public bool drawable => true;

    private bool _draw = true;

    private readonly IRenderer _renderer;

    public GlitchEffect(IRenderer renderer) => _renderer = renderer;

    public (Vector2, RenderCharacter) ApplyModifiers(Vector2Int at, Vector2 position, RenderCharacter character) {
        position = new Vector2(position.x + RandomFloat() * 0.1f, position.y);
        string mappings = _renderer.font?.mappings ?? "";
        character = new RenderCharacter(
            RandomNonNegativeFloat() <= 0.98f ? character.character : mappings[Random.Shared.Next(0, mappings.Length)],
            RandomizeColor(character.background), RandomizeColor(character.foreground),
            RandomNonNegativeFloat() <= 0.95f ? character.style :
                (RenderStyle)Random.Shared.Next((int)RenderStyle.None, (int)RenderStyle.All));
        return (position, character);
    }

    public void Update(bool fullscreen) { }

    public void Draw(Vector2Int position) {
        if(position.x % Random.Shared.Next(3, 10) == 0 || position.y % Random.Shared.Next(3, 10) == 0)
            _draw = RandomNonNegativeFloat() > 0.95f;
        if(!_draw || !_renderer.IsCharacterEmpty(position)) return;
        string mappings = _renderer.font?.mappings ?? "";
        _renderer.DrawCharacter(position, new RenderCharacter(
            mappings[Random.Shared.Next(0, mappings.Length)],
            RandomizeColor(Color.transparent), RandomizeColor(Color.white),
            (RenderStyle)Random.Shared.Next((int)RenderStyle.None, (int)RenderStyle.All + 1)));
    }

    private static float RandomFloat() => Random.Shared.NextSingle(-1f, 1f);
    private static float RandomNonNegativeFloat() => Random.Shared.NextSingle(0f, 1f);
    private static float RandomColorComponent(float current) => current + RandomFloat() * 0.3f;

    private static Color RandomizeColor(Color current) => RandomNonNegativeFloat() <= 0.98f ?
        new Color(RandomColorComponent(current.r), RandomColorComponent(current.g),
            RandomColorComponent(current.b), RandomColorComponent(current.a)) : RandomColor();

    private static Color RandomColor() => Random.Shared.Next(0, 2) == 0 ? new Color(1f, 0f, 0f, 1f) :
        Random.Shared.Next(0, 2) == 0 ? new Color(0f, 1f, 0f, 1f) : new Color(0f, 0f, 1f, 1f);
}
