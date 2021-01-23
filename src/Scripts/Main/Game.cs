using System.Collections.Generic;

using PPR.Main.Levels;
using PPR.Main.Managers;

using SFML.Graphics;
using SFML.Window;

namespace PPR.Scripts.Main {
    public class Game {
        public static float levelTime => PPR.Main.Game.levelTime.AsSeconds();
        public static float timeFromStart => levelTime;
        public static int roundedOffset => PPR.Main.Game.roundedOffset;
        public static float steps => PPR.Main.Game.steps;
        public static int roundedSteps => PPR.Main.Game.roundedSteps;
        public static int currentDirectionLayer => PPR.Main.Game.currentDirectionLayer;
        public static int currentBPM => PPR.Main.Game.currentBPM;
        public static int score => ScoreManager.score;
        public static int[] scores => ScoreManager.scores;
        public static int health => PPR.Main.Game.health;
        public static int accuracy => ScoreManager.accuracy;
        public static int combo => ScoreManager.combo;
        public static int maxCombo => ScoreManager.maxCombo;

        public static char GetNoteBinding(Keyboard.Key key) => PPR.Main.Game.GetNoteBinding(key);
        public static float StepsToMilliseconds(float steps) => PPR.Main.Calc.StepsToMilliseconds(steps);
        public static float MillisecondsToSteps(float time) => PPR.Main.Calc.MillisecondsToSteps(time);
        public static float StepsToOffset(float steps) => PPR.Main.Calc.StepsToOffset(steps);
        public static int StepsToDirectionLayer(float steps) => PPR.Main.Calc.StepsToDirectionLayer(steps);
        public static bool StepPassedLine(int step, int lineOffset = 0) =>
            PPR.Main.Game.StepPassedLine(step, lineOffset);
        public static int GetBPMAtStep(int step) => PPR.Main.Calc.GetBPMAtStep(step, Map.currentLevel.speeds);
        public static IEnumerable<int> GetBPMBetweenSteps(int start, int end) =>
            PPR.Main.Calc.GetBPMBetweenSteps(start, end, Map.currentLevel.speeds);
        public static List<LevelSpeed> GetSpeedsBetweenSteps(int start, int end) =>
            PPR.Main.Calc.GetSpeedsBetweenSteps(start, end, Map.currentLevel.speeds);
        public static Color GetAccuracyColor(int accuracy) => ScoreManager.GetAccuracyColor(accuracy);
        public static Color GetComboColor(int accuracy, int misses) => ScoreManager.GetComboColor(accuracy, misses);
    }
}
