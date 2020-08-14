using System;
using System.Reflection;

using NLog;

using PPR.GUI;
using PPR.Main;
using PPR.Rendering;

using SFML.System;

namespace PPR {
    public static class Core {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly string version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        public static float deltaTime;
        public static readonly Game game = new Game();
        public static readonly Renderer renderer = new Renderer(80, 60, 0);

        // bruh Rider wth
        // ReSharper disable once UnusedMember.Local
        static void Main() {
            game.Start(); // Start the game

            logger.Info("Loading finished");

            Clock fpsClock = new Clock();
            while(renderer.window.IsOpen) { // Executes every frame
                renderer.window.DispatchEvents();

                game.Update();

                renderer.Update();
                renderer.Draw();

                renderer.window.Display();

                deltaTime = fpsClock.Restart().AsSeconds();
                UI.fps = (int)MathF.Round(1f / deltaTime);
                //if(UI.fps < 30 && renderer.window.HasFocus()) logger.Warn("Lag detected: too low fps ({0})", UI.fps);
            }
        }
    }
}
