using System;

using PPR.GUI;

using SFML.Graphics;

namespace PPR.Main.Managers {
    public static class ScoreManager {
        public static int score {
            get => _score;
            set {
                Lua.InvokeEvent(null, "scoreChanged", value, _score);
                _score = value;
            }
        }

        public static int accuracy {
            get => _accuracy;
            private set {
                int newValue = Math.Clamp(value, 0, 100);
                Lua.InvokeEvent(null, "accuracyChanged", newValue, _accuracy);
                _accuracy = newValue;
            }
        }

        public static int[] scores {
            get => _scores;
            private set {
                Lua.InvokeEvent(null, "scoresChanged", 1);
                Lua.InvokeEvent(null, "scoresChanged", 2);
                Lua.InvokeEvent(null, "scoresChanged", 3);
                _scores = value;
            }
        }

        public static int combo {
            get => _combo;
            set {
                Lua.InvokeEvent(null, "comboChanged", value, _combo);
                _combo = value;
            }
        }

        public static int maxCombo {
            get => _maxCombo;
            set {
                Lua.InvokeEvent(null, "maxComboChanged", value, _maxCombo);
                _maxCombo = value;
            }
        }

        private static int _score;
        private static int _accuracy = 100;
        private static int[] _scores = new int[3];
        private static int _combo;
        private static int _maxCombo;

        public static void ResetScore() {
            score = 0;
            //UI.prevScore = 0;
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
    }
}
