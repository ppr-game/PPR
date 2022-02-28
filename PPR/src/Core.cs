using PER;
using PER.Audio.Sfml;

using PPR.Resources;

using PRR.Sfml;

namespace PPR;

public static class Core {
    public static Engine engine { get; } =
        new(new ResourcesManager(), new Game(), new Renderer(), new AudioManager()) { tickInterval = 0.02d };

    private static void Main() => engine.Reload();
}
