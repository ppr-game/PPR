using SFML.System;

namespace PPR.Scripts.Main.Levels {
    public class Map {
        public static Vector2i gameLinePos => PPR.Main.Levels.Map.gameLinePos;
        public static Vector2i editorLinePos => PPR.Main.Levels.Map.editorLinePos;
        public static Vector2i linePos => PPR.Main.Levels.Map.linePos;
        public static int flashLine { set => PPR.Main.Levels.Map.flashLine = value; }
    }
}
