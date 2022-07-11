using PER;
using PER.Audio.Sfml;
using PER.Common.Resources;
using PER.Util;

using PRR.Sfml;
using PRR.UI;

namespace PPR;

public static class Core {
    public static readonly string version = Helper.GetVersion();
    public static readonly string engineVersion = Engine.version;
    public static readonly string abstractionsVersion = Engine.abstractionsVersion;
    public static readonly string utilVersion = Helper.version;
    public static readonly string commonVersion = Helper.GetVersion(typeof(ResourcesManager));
    public static readonly string audioVersion = Helper.GetVersion(typeof(AudioManager));
    public static readonly string rendererVersion = Helper.GetVersion(typeof(Renderer));
    public static readonly string uiVersion = Helper.GetVersion(typeof(Button));

    private static readonly Renderer renderer = new();

    public static Engine engine { get; } =
        new(new ResourcesManager(), new Game(), renderer, new InputManager(renderer), new AudioManager()) {
            tickInterval = 0.02d
        };

    private static void Main() => engine.Reload();
}
