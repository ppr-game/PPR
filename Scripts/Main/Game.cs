using System.Collections.Generic;

using PPR.Main.Levels;

using SFML.Graphics;
using SFML.Window;

namespace PPR.Scripts.Main {
    public class Game {
        public static float timeFromStart => PPR.Main.Game.timeFromStart.AsSeconds();
        public static int roundedOffset => PPR.Main.Game.roundedOffset;
        public static float steps => PPR.Main.Game.steps;
        public static int roundedSteps => PPR.Main.Game.roundedSteps;
        public static int currentDirectionLayer => PPR.Main.Game.currentDirectionLayer;
        public static int currentBPM => PPR.Main.Game.currentBPM;
        public static int score => PPR.Main.Game.score;
        public static int[] scores => PPR.Main.Game.scores;
        public static int health => PPR.Main.Game.health;
        public static int accuracy => PPR.Main.Game.accuracy;
        public static int combo => PPR.Main.Game.combo;
        public static int maxCombo => PPR.Main.Game.maxCombo;

        public static char GetNoteBinding(Keyboard.Key key) => PPR.Main.Game.GetNoteBinding(key);
        public static float StepsToMilliseconds(float steps) => PPR.Main.Game.StepsToMilliseconds(steps);
        public static float MillisecondsToSteps(float time) => PPR.Main.Game.MillisecondsToSteps(time);
        public static float StepsToOffset(float steps) => PPR.Main.Game.StepsToOffset(steps);
        public static int StepsToDirectionLayer(float steps) => PPR.Main.Game.StepsToDirectionLayer(steps);
        public static bool StepPassedLine(int step, int lineOffset = 0) =>
            PPR.Main.Game.StepPassedLine(step, lineOffset);
        public static int GetBPMAtStep(int step) => PPR.Main.Game.GetBPMAtStep(step, Map.currentLevel.speeds);
        public static IEnumerable<int> GetBPMBetweenSteps(int start, int end) =>
            PPR.Main.Game.GetBPMBetweenSteps(start, end, Map.currentLevel.speeds);
        public static List<LevelSpeed> GetSpeedsBetweenSteps(int start, int end) =>
            PPR.Main.Game.GetSpeedsBetweenSteps(start, end, Map.currentLevel.speeds);
        public static Color GetAccuracyColor(int accuracy) => PPR.Main.Game.GetAccuracyColor(accuracy);
        public static Color GetComboColor(int accuracy, int misses) => PPR.Main.Game.GetComboColor(accuracy, misses);
    }
}
