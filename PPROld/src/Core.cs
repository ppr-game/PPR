using System;
using System.Reflection;

using NLog;

using PER;
using PER.Audio.Sfml;

using PPROld.Main;
using PPROld.Resources;

using PRR.Sfml;

namespace PPROld;

public static class Core {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static readonly string version = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion;
    public static readonly string prrVersion = Assembly.GetAssembly(typeof(Renderer))?
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion;

    public static Engine engine { get; } =
        new(new ResourcesManager(), new Game(), new Renderer(), new AudioManager()) { tickInterval = 0.02d };

    private static void Main() {
#if !DEBUG
        try {
#endif
            engine.Reload();
#if !DEBUG
        }
        catch(Exception ex) {
            logger.Fatal(ex);
            throw;
        }
#endif
    }
}
