using System;
using System.IO;
using System.Reflection;

using NLog;

using PPR.Main;
using PPR.Main.Levels;
using PPR.Main.Managers;
using PPR.Properties;

using PRR;

using SFML.System;

namespace PPR {
    public static class Core {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
        private static void Main() {
#if !DEBUG
            try {
#endif
                renderer.UpdateFramerateSetting();

                static void SubscribeEvents() {
                    renderer.window.KeyPressed += Game.KeyPressed;
                    renderer.window.MouseWheelScrolled += Game.MouseWheelScrolled;
                    renderer.window.LostFocus += Game.LostFocus;
                    renderer.window.GainedFocus += Game.GainedFocus;
                    renderer.window.Closed += (_, __) => Game.Exit();
                    UI.ColorScheme.Reload();
                }

                Lua.Manager.ScriptSetup();
                Bindings.Reload();
                SubscribeEvents();
                renderer.onWindowRecreated += (_, __) => SubscribeEvents();
                SoundManager.ReloadSounds();

                Game.Start(); // Start the game

                logger.Info("Loading finished");

                Clock fpsClock = new Clock();
                while(renderer.window.IsOpen) { // Executes every frame
                    renderer.window.DispatchEvents();

                    game.Update();

                    renderer.Clear();
                    Map.Draw();
                    UI.Manager.Draw();
                    UI.Manager.UpdateAnims();
                    renderer.Draw(Settings.GetBool("bloom"));

                    renderer.window.Display();

                    deltaTime = fpsClock.Restart().AsSeconds();
                    UI.Manager.fps = (int)MathF.Round(1f / deltaTime);
                    UI.Manager.tempAvgFPS += UI.Manager.fps;
                    UI.Manager.tempAvgFPSCounter++;
                    if(UI.Manager.tempAvgFPSCounter >= 100) {
                        UI.Manager.avgFPS = UI.Manager.tempAvgFPS / UI.Manager.tempAvgFPSCounter;
                        UI.Manager.tempAvgFPS = 0;
                        UI.Manager.tempAvgFPSCounter = 0;
                    }
                    
                    if(Game.exiting && Game.exitTime <= 0f) renderer.window.Close();
                }
#if !DEBUG
            }
            catch(Exception ex) {
                logger.Fatal(ex);
                throw;
            }
#endif
        }
    }
}
