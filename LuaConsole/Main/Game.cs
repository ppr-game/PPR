using System.IO;

using PPR.GUI;
using PPR.LuaConsole.Rendering;
using PPR.Main;
using PPR.Main.Levels;

namespace PPR.LuaConsole.Main {
    public class Game : Scripts.Main.Game {
        public static string statsState => PPR.Main.Game.statsState switch {
            StatsState.Fail => "fail",
            StatsState.Pass => "pass",
            _ => "pause"
        };

        public static bool editing {
            get => PPR.Main.Game.editing;
            set => PPR.Main.Game.editing = value;
        }
        
        public static bool auto {
            get => PPR.Main.Game.auto;
            set => PPR.Main.Game.auto = value;
        }
        
        public static void Exit() => PPR.Main.Game.Exit();

        public static void LoadLevel(string levelName, string diffName) {
            string path = Path.Join("levels", levelName);
            Map.LoadLevelFromPath(path, levelName, diffName);
            PPR.Main.Game.RecalculatePosition();
        }
    }
}
