using PER.Audio.Sfml;
using PER.Demo.Resources;

using PRR.Sfml;

namespace PER.Demo;

public static class Core {
    public static Engine engine { get; } =
        new(new ResourcesManager(), new Game(), new Renderer(), new AudioManager()) { tickInterval = 0.02d };

    private static void Main() => engine.Reload();
}
