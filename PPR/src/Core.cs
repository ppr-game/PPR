using System;
using System.IO;
using System.Reflection;

using NLog;

using PER.Abstractions.Renderer;

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
        public static readonly Game game = new();
        public static readonly Renderer renderer = new();

        private static void Main() {
#if !DEBUG
            try {
#endif
                renderer.onWindowCreated += (_, __) => {
                    renderer.window.KeyPressed += Game.KeyPressed;
                    renderer.window.MouseWheelScrolled += Game.MouseWheelScrolled;
                    renderer.window.LostFocus += Game.LostFocus;
                    renderer.window.GainedFocus += Game.GainedFocus;
                    renderer.window.Closed += (x, unknown) => Game.Exit();
                    UI.ColorScheme.Reload();
                };
                
                renderer.Setup(new RendererSettings {
                    title = "Press Press Revolution",
                    width = 80,
                    height = 60,
                    framerate = 0,
                    fullscreen = Settings.GetBool("fullscreen"),
                    font = Path.Join("resources", "fonts", Settings.GetPath("font"))
                });
                renderer.UpdateFramerateSetting();

                Lua.Manager.ScriptSetup();
                Bindings.Reload();
                SoundManager.ReloadSounds();

                Game.Start(); // Start the game

                logger.Info("Loading finished");

                Clock fpsClock = new();
                while(renderer.open) { // Executes every frame
                    renderer.Loop();

                    game.Update();

                    renderer.Clear();
                    Map.Draw();
                    UI.Manager.Draw();
                    UI.Manager.UpdateAnims();
                    renderer.Draw(Settings.GetBool("bloom"));

                    deltaTime = fpsClock.Restart().AsSeconds();
                    UI.Manager.fps = (int)MathF.Round(1f / deltaTime);
                    UI.Manager.tempAvgFPS += UI.Manager.fps;
                    UI.Manager.tempAvgFPSCounter++;
                    if(UI.Manager.tempAvgFPSCounter >= 100) {
                        UI.Manager.avgFPS = UI.Manager.tempAvgFPS / UI.Manager.tempAvgFPSCounter;
                        UI.Manager.tempAvgFPS = 0;
                        UI.Manager.tempAvgFPSCounter = 0;
                    }
                    
                    if(Game.exiting && Game.exitTime <= 0f) renderer.Stop();
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
