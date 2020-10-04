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

        public static bool pauseDrawing;

        // bruh Rider wth
        // ReSharper disable once UnusedMember.Local
        static void Main() {
            try {
                renderer.UpdateFramerateSetting();

                static void SubscribeEvents() {
                    renderer.window.KeyPressed += Game.KeyPressed;
                    renderer.window.MouseWheelScrolled += Game.MouseWheelScrolled;
                    renderer.window.LostFocus += Game.LostFocus;
                    renderer.window.GainedFocus += Game.GainedFocus;
                    renderer.window.Closed += (___, ____) => Game.End();
                }

                SubscribeEvents();
                renderer.onWindowRecreated += (_, __) => SubscribeEvents();
                Bindings.Reload();
                ColorScheme.Reload();
                Game.ReloadSounds();

                Game.Start(); // Start the game

                logger.Info("Loading finished");

                Clock fpsClock = new Clock();
                while(renderer.window.IsOpen) { // Executes every frame
                    renderer.window.DispatchEvents();

                    game.Update();

                    if(!pauseDrawing) {
                        renderer.Clear();
                        Map.Draw();
                        UI.Draw();
                    }
                    UI.UpdateAnims();
                    renderer.Draw(ColorScheme.GetColor("background"), Settings.GetBool("bloom"));

                    renderer.window.Display();

                    deltaTime = fpsClock.Restart().AsSeconds();
                    UI.fps = (int)MathF.Round(1f / deltaTime);
                    UI.tempAvgFPS += UI.fps;
                    UI.tempAvgFPSCounter++;
                    if(UI.tempAvgFPSCounter >= 100) {
                        UI.avgFPS = UI.tempAvgFPS / UI.tempAvgFPSCounter;
                        UI.tempAvgFPS = 0;
                        UI.tempAvgFPSCounter = 0;
                    }
                    
                    if(Game.exiting && UI.fadeOutFinished) renderer.window.Close();
                }
            }
            catch(Exception ex) {
                logger.Fatal(ex);
            }
        }
    }
}
