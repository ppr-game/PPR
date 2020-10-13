using System;

using PPR.GUI;

using SFML.Graphics;

namespace PPR.Main.Managers {
    public static class ScoreManager {
        public static int score;
        private static int _accuracy = 100;

        public static int accuracy {
            get => _accuracy;
            private set => _accuracy = Math.Clamp(value, 0, 100);
        }

        public static int[] scores { get; private set; } = new int[3];
        public static int combo { get; set; }
        public static int maxCombo { get; set; }
        public static void ResetScore() {
            score = 0;
            UI.prevScore = 0;
            scores = new int[3];
            accuracy = 100;
            combo = 0;
            maxCombo = 0;
        }
        public static void RecalculateAccuracy() {
            float sum = scores[0] + scores[1] + scores[2];
            float mulSum = scores[1] * 0.5f + scores[2];
            accuracy = (int)MathF.Floor(mulSum / sum * 100f);
        }
        public static Color GetAccuracyColor(int accuracy) => accuracy >= 100 ? ColorScheme.GetColor("accuracy_good") :
            accuracy >= 70 ? ColorScheme.GetColor("accuracy_ok") : ColorScheme.GetColor("accuracy_bad");
        public static Color GetComboColor(int accuracy, int misses) => accuracy >= 100 ? ColorScheme.GetColor("perfect_combo") :
            misses <= 0 ? ColorScheme.GetColor("full_combo") : ColorScheme.GetColor("combo");
    }
}
