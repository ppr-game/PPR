using System;
using System.IO;
using System.Reflection;

using NLog;

using PPR.GUI;
using PPR.Main;
using PPR.Main.Levels;
using PPR.Properties;

using PRR;

using SFML.System;

namespace PPR {
    public static class Core {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly string version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        public static readonly string prrVersion = Assembly.GetAssembly(typeof(Renderer))?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        public static float deltaTime;
        public static readonly Game game = new Game();
        public static readonly Renderer renderer = new Renderer("Press Press Revolution", 80, 60, 0,
            Settings.GetBool("fullscreen"), Path.Join("resources", "fonts", Settings.GetPath("font")));

        // bruh Rider wth
        // ReSharper disable once UnusedMember.Local
        static void Main() {
            renderer.UpdateFramerateSetting();
            //renderer.onWindowRecreated += (_, __) => {
                renderer.window.KeyPressed += Game.KeyPressed;
                renderer.window.MouseWheelScrolled += Game.MouseWheelScrolled;
                renderer.window.LostFocus += Game.LostFocus;
                renderer.window.GainedFocus += Game.GainedFocus;
                renderer.window.Closed += (___, ____) => Game.End();
            //};
            Bindings.Reload();
            ColorScheme.Reload();
            Game.ReloadSounds();
            
            Game.Start(); // Start the game

            logger.Info("Loading finished");

            Clock fpsClock = new Clock();
            while(renderer.window.IsOpen) { // Executes every frame
                renderer.window.DispatchEvents();

                game.Update();

                renderer.Clear();
                Map.Draw();
                UI.Draw();
                renderer.Draw(ColorScheme.GetColor("background"), Settings.GetBool("bloom"));

                renderer.window.Display();

                deltaTime = fpsClock.Restart().AsSeconds();
                UI.fps = (int)MathF.Round(1f / deltaTime);
                //if(UI.fps < 30 && renderer.window.HasFocus()) logger.Warn("Lag detected: too low fps ({0})", UI.fps);
            }
        }
    }
}
