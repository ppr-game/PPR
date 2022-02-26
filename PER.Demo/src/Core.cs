using PER.Audio.Sfml;

using PRR.Sfml;

namespace PER.Demo;

public static class Core {
    public static Engine engine { get; } =
        new(new Game(), new Renderer(), new AudioManager(), new Resources()) { tickInterval = 0.02d };

    private static void Main() => engine.Load();
}
