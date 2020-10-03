using System.Collections.Generic;

using PPR.Main.Levels;

namespace PPR.Scripts.Main.Levels {
    public class LevelMetadata {
        public static string name => PPR.Main.Levels.Map.currentLevel.metadata.name;
        public static int hpDrain => PPR.Main.Levels.Map.currentLevel.metadata.hpDrain;
        public static int hpRestorage => PPR.Main.Levels.Map.currentLevel.metadata.hpRestorage;
        public static string difficulty => PPR.Main.Levels.Map.currentLevel.metadata.difficulty;
        public static string author => PPR.Main.Levels.Map.currentLevel.metadata.author;
        public static string length => PPR.Main.Levels.Map.currentLevel.metadata.length;
        public static int maxStep => PPR.Main.Levels.Map.currentLevel.metadata.maxStep;
        public static int linesFrequency => PPR.Main.Levels.Map.currentLevel.metadata.linesFrequency;
        public static int initialOffsetMs => PPR.Main.Levels.Map.currentLevel.metadata.initialOffsetMs;
        public static string bpm => PPR.Main.Levels.Map.currentLevel.metadata.bpm;
        public static bool skippable => PPR.Main.Levels.Map.currentLevel.metadata.skippable;
        public static int skipTime => PPR.Main.Levels.Map.currentLevel.metadata.skipTime;
    }
    public class Level {
        public static List<LevelObject> objects => PPR.Main.Levels.Map.currentLevel.objects;
        public static List<LevelSpeed> speeds => PPR.Main.Levels.Map.currentLevel.speeds;
    }
}
