using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using PPR.GUI;
using PPR.GUI.Elements;
using PPR.Main.Managers;
using PPR.Properties;

using PRR;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace PPR.Main.Levels {
    public struct LevelSelectLevel {
        public Button button;
        public Dictionary<string, LevelSelectDiff> diffs;
    }
    public struct LevelSelectDiff {
        public Button button;
        public List<LevelScore> scores;
        public LevelMetadata metadata;
    }
    public struct LevelScore {
        public Vector2i scorePosition;
        public readonly int score;
        public readonly string scoreStr;
        public Vector2i accComboPosition;
        public readonly int accuracy;
        public readonly int maxCombo;
        public readonly string accuracyStr;
        public Color accuracyColor;
        public Vector2i accComboDividerPosition;
        public Vector2i maxComboPosition;
        public readonly string maxComboStr;
        public Color maxComboColor;
        public Vector2i scoresPosition;
        public readonly int[] scores;
        public Vector2i linePosition;

        public LevelScore(Vector2i position, int score, int accuracy, int maxCombo, int[] scores) {
            scorePosition = position;
            this.score = score;
            scoreStr = $"SCORE: {score.ToString()}";
            accComboPosition = new Vector2i(position.X, position.Y + 1);
            this.accuracy = accuracy;
            this.maxCombo = maxCombo;
            accuracyStr = $"{accuracy.ToString()}%";
            accuracyColor = ScoreManager.GetAccuracyColor(accuracy);
            accComboDividerPosition = accComboPosition + new Vector2i(accuracyStr.Length, 0);
            maxComboPosition = accComboDividerPosition + new Vector2i(1, 0);
            maxComboStr = $"{maxCombo.ToString()}x";
            maxComboColor = ScoreManager.GetComboColor(accuracy, scores[0]);
            scoresPosition = new Vector2i(position.X, position.Y + 2);
            this.scores = scores;
            linePosition = new Vector2i(position.X - 1, position.Y + 3);
        }
        public LevelScore(int score, int accuracy, int maxCombo, int[] scores) : this(new Vector2i(), score, accuracy,
            maxCombo, scores) { }
        public void Move(Vector2i by) {
            scorePosition += by;
            accComboPosition += by;
            accComboDividerPosition += by;
            maxComboPosition += by;
            scoresPosition += by;
            linePosition += by;
        }
    }
    public struct LevelMetadata {
        public readonly string name;
        public readonly string diff;
        public readonly string displayDiff;
        public int hpDrain;
        public int hpRestorage;
        private float _difficulty;
        public float difficulty {
            get => _difficulty;
            set {
                _difficulty = value;
                displayDifficulty = value.ToString("0.00", CultureInfo.InvariantCulture);
            }
        }
        public string displayDifficulty { get; private set; }
        public readonly string author;
        public TimeSpan lengthSpan;
        public string length;
        public string totalLength;
        public int maxStep;
        public int linesFrequency;
        public int musicOffset;
        public readonly string bpm;
        public readonly bool skippable;
        public readonly int skipTime;
        public readonly int objectCount;
        public readonly int speedsCount;

        private LevelMetadata(string name, string diff, IReadOnlyList<string> meta, int objectCount,
            IReadOnlyList<char> chars, IReadOnlyList<int> steps, List<LevelSpeed> speeds) {
            this.name = name;
            this.diff = diff;
            displayDiff = diff == "level" || diff == null ? "DEFAULT" : diff.ToUpper();
            hpDrain = int.Parse(meta[0]);
            hpRestorage = int.Parse(meta[1]);
            //difficulty = meta[2];
            author = meta[3];
            linesFrequency = meta.Count > 4 ? int.Parse(meta[4]) : 4;
            musicOffset = meta.Count > 5 ? int.Parse(meta[5]) : 0;

            List<LightLevelObject> objects = new List<LightLevelObject>();
            for(int i = 0; i < Math.Min(chars.Count, steps.Count); i++)
                objects.Add(new LightLevelObject(chars[i], steps[i]));
            speeds.Sort((speed1, speed2) => speed1.step.CompareTo(speed2.step));

            lengthSpan = Calc.GetLevelLength(objects, speeds, musicOffset);
            length = Calc.TimeSpanToLength(lengthSpan);
            totalLength = Calc.TimeSpanToLength(Calc.GetTotalLevelLength(objects, speeds, musicOffset));

            _difficulty = Calc.GetDifficulty(objects, speeds, (int)lengthSpan.TotalMinutes);
            displayDifficulty = _difficulty.ToString("0.00", CultureInfo.InvariantCulture);

            int minStep = objects.Count > 0 ? Calc.GetFirstObject(objects).step : 0;
            int minTime = (int)Calc.StepsToMilliseconds(minStep, speeds);
            skipTime = minTime - 3000;
            skippable = skipTime > 3000;

            IEnumerable<int> onlyAbsoluteSpeeds = speeds.Select(speed => Math.Abs(speed.speed));
            int minBPM = onlyAbsoluteSpeeds.Min();
            int maxBPM = onlyAbsoluteSpeeds.Max();
            int avgBPM = (int)Calc.GetAverageBPM(speeds, objects.Count > 0 ? Calc.GetLastObject(objects).step : 0);
            bpm = minBPM == maxBPM ? avgBPM.ToString() : $"{minBPM.ToString()}-{maxBPM.ToString()} ({avgBPM.ToString()})";

            maxStep = objects.Count > 0 ? Calc.GetLastObject(objects).step : 0;

            this.objectCount = objectCount;
            speedsCount = speeds.Count;
        }
        public LevelMetadata(Level level, IReadOnlyList<string> meta, string name, string diff) : this(name, diff, meta,
            level.objects.FindAll(obj => obj.character != LevelObject.HoldChar).Count,
            level.objects.FindAll(obj => obj.character != LevelObject.SpeedChar).Select(obj => obj.character).ToList(),
            level.objects.FindAll(obj => obj.character != LevelObject.SpeedChar).Select(obj => obj.step).ToList(), level.speeds) { }
        private static List<LevelSpeed> SpeedsFromLists(IReadOnlyList<int> speeds, IEnumerable<int> speedsSteps) {
            List<LevelSpeed> combinedSpeeds = speedsSteps.Select((t, i) => new LevelSpeed(speeds[i], t)).ToList();
            combinedSpeeds.Sort((speed1, speed2) => speed1.step.CompareTo(speed2.step));
            return combinedSpeeds;
        }
        public LevelMetadata(IReadOnlyList<string> lines, string name, string diff) : this(name, diff,
            lines[4].Split(':'),
            lines[0].ToList().FindAll(obj => obj != LevelObject.HoldChar).Count, lines[0].ToList(),
            lines[1].Length > 0 ? lines[1].Split(':').Select(int.Parse).ToList() : new List<int>(),
            SpeedsFromLists(
                lines[2].Length > 0 ? lines[2].Split(':').Select(int.Parse).ToList() : new List<int>(),
                lines[3].Length > 0 ? lines[3].Split(':').Select(int.Parse).ToList() : new List<int>())) { }
    }
    public class Level {
        public LevelMetadata metadata;
        public List<LevelObject> objects { get; } = new List<LevelObject>();
        public List<LevelSpeed> speeds { get; } = new List<LevelSpeed>();
        public string script { get; }

        public static bool IsLevelValid(IReadOnlyList<string> lines) {
            bool any;
            try {
                any = lines[2].Split(':').Select(int.Parse).Any();
                any = any && lines[3].Split(':').Select(int.Parse).Any();
            }
            catch(Exception) {
                return false;
            }
            return any;
        }
        public Level(IReadOnlyList<string> lines, string name, string diff, string script = null) {
            int[] objectsSteps = lines[1].Length > 0 ? lines[1].Split(':').Select(int.Parse).ToArray() : new int[0];
            int[] speeds = lines[2].Length > 0 ? lines[2].Split(':').Select(int.Parse).ToArray() : new int[0];
            int[] speedsStarts = lines[3].Length > 0 ? lines[3].Split(':').Select(int.Parse).ToArray() : new int[0];
            for(int i = 0; i < speedsStarts.Length; i++) {
                this.speeds.Add(new LevelSpeed(speeds[i], speedsStarts[i]));
                objects.Add(new LevelObject(LevelObject.SpeedChar, speedsStarts[i], this.speeds));
            }
            for(int i = 0; i < objectsSteps.Length; i++) {
                int step = objectsSteps[i];
                objects.Add(new LevelObject(lines[0][i], step, this.speeds, objects));
            }
            this.speeds.Sort((speed1, speed2) => speed1.step.CompareTo(speed2.step));
            string[] meta = lines[4].Split(':');
            metadata = new LevelMetadata(this, meta, name, diff);
            this.script = script;
        }
    }
    public class LevelSpeed {
        public int speed;
        public readonly int step;
        public LevelSpeed(int speed, int step) {
            this.speed = speed;
            this.step = step;
        }
    }
    public readonly struct LightLevelObject {
        public readonly char character;
        public readonly int step;
        public LightLevelObject(char character, int step) {
            this.character = character;
            this.step = step;
        }
    }
    public readonly struct ClipboardLevelObject {
        public readonly char character;
        public readonly int step;
        public readonly int xPos;
        public ClipboardLevelObject(char character, int step, int xPos) {
            this.character = character;
            this.step = step;
            this.xPos = xPos;
        }
    }
    public class LevelObject {
        public static readonly string[] lines = {
            "1234567890-=", // ReSharper disable once StringLiteralTypo
            "qwertyuiop[]", // ReSharper disable once StringLiteralTypo
            "asdfghjkl;'", // ReSharper disable once StringLiteralTypo
            "zxcvbnm,./"
        };
        public static Color[] linesColors { get; set; }
        public static Color[] linesDarkColors { get; set; }
        public const char SpeedChar = '>';
        public const char HoldChar = '│';
        public static int perfectRange { get; set; }
        public static int hitRange { get; set; }
        public static int missRange { get; set; }
        public Vector2i startPosition { get; }
        public char character { get; }
        public Keyboard.Key key { get; }
        public int step { get; }
        public static Color speedColor { get; set; }
        public static Color nextDirLayerSpeedColor { get; set; }

        public bool removed { get; private set; }
        public bool toDestroy { get; set; }

        private Vector2i _position;
        private readonly int _directionLayer;
        private Color _hitColor;
        private readonly int _line;
        private Color _color;
        private Color _nextDirLayerColor;
        private float _removeAnimationTime;
        
        public bool ignore { get; private set; }

        public LevelObject(char character, int step, IEnumerable<LevelSpeed> speeds, IReadOnlyCollection<LevelObject> objects = null) {
            character = Settings.GetBool("uppercaseNotes") ? char.ToUpper(character) : char.ToLower(character);
            int x = Calc.GetXPosForCharacter(character);
            if(character == HoldChar && objects != null) {
                List<LevelObject> existingObjects = objects.Where(obj =>
                    obj.step == step && obj.character != SpeedChar && obj.character != HoldChar).ToList();
                LevelObject keyObject = existingObjects.Last(obj =>
                    obj.step == step && obj.character != SpeedChar && obj.character != HoldChar);
                x = keyObject._position.X;
                key = keyObject.key;
                keyObject.ignore = true;
            }
            List<LevelSpeed> existingSpeeds = new List<LevelSpeed>(speeds);
            existingSpeeds.Sort((spd1, spd2) => spd1.step.CompareTo(spd2.step));
            startPosition = new Vector2i(x, Map.linePos.Y - (int)Calc.StepsToOffset(step, existingSpeeds));
            _position = startPosition;
            this.character = character;
            char lineChar = character == HoldChar ? Game.GetNoteBinding(key) : character;
            lineChar = char.ToLower(lineChar);
            foreach(string line in lines) {
                if(line.Contains(lineChar)) break;
                _line++;
            }
            UpdateColors();
            this.step = step;
            _directionLayer = Calc.StepsToDirectionLayer(step, existingSpeeds);

            #region Set the key

            key = char.ToUpper(character) switch {
                '1' => Keyboard.Key.Num1,
                '2' => Keyboard.Key.Num2,
                '3' => Keyboard.Key.Num3,
                '4' => Keyboard.Key.Num4,
                '5' => Keyboard.Key.Num5,
                '6' => Keyboard.Key.Num6,
                '7' => Keyboard.Key.Num7,
                '8' => Keyboard.Key.Num8,
                '9' => Keyboard.Key.Num9,
                '0' => Keyboard.Key.Num0,
                '-' => Keyboard.Key.Hyphen,
                '=' => Keyboard.Key.Equal,
                'Q' => Keyboard.Key.Q,
                'W' => Keyboard.Key.W,
                'E' => Keyboard.Key.E,
                'R' => Keyboard.Key.R,
                'T' => Keyboard.Key.T,
                'Y' => Keyboard.Key.Y,
                'U' => Keyboard.Key.U,
                'I' => Keyboard.Key.I,
                'O' => Keyboard.Key.O,
                'P' => Keyboard.Key.P,
                '[' => Keyboard.Key.LBracket,
                ']' => Keyboard.Key.RBracket,
                'A' => Keyboard.Key.A,
                'S' => Keyboard.Key.S,
                'D' => Keyboard.Key.D,
                'F' => Keyboard.Key.F,
                'G' => Keyboard.Key.G,
                'H' => Keyboard.Key.H,
                'J' => Keyboard.Key.J,
                'K' => Keyboard.Key.K,
                'L' => Keyboard.Key.L,
                ';' => Keyboard.Key.Semicolon,
                '\'' => Keyboard.Key.Quote,
                'Z' => Keyboard.Key.Z,
                'X' => Keyboard.Key.X,
                'C' => Keyboard.Key.C,
                'V' => Keyboard.Key.V,
                'B' => Keyboard.Key.B,
                'N' => Keyboard.Key.N,
                'M' => Keyboard.Key.M,
                ',' => Keyboard.Key.Comma,
                '.' => Keyboard.Key.Period,
                '/' => Keyboard.Key.Slash,
                _ => key
            };

            #endregion
        }
        public void UpdateColors() {
            if(character == SpeedChar) return;
            _color = linesColors[_line];
            _nextDirLayerColor = linesDarkColors[_line];
        }

        public void Draw() {
            if(removed && !ignore) {
                if(_removeAnimationTime <= 0f) {
                    List<LevelObject> samePosObjects = Map.currentLevel.objects.FindAll(obj =>
                        obj._position == _position && obj.removed && obj != this);
                    if(samePosObjects.Count > 0) samePosObjects.ForEach(obj => obj.toDestroy = true);
                }
                Core.renderer.SetCellColor(_position,
                    Renderer.AnimateColor(_removeAnimationTime, _hitColor, ColorScheme.GetColor("foreground"), 3f),
                    Renderer.AnimateColor(_removeAnimationTime, _hitColor, ColorScheme.GetColor("transparent"), 3f));
                if(_removeAnimationTime >= 1f) toDestroy = true;
                _removeAnimationTime += Core.deltaTime;
                return;
            }
            if(!ignore && !toDestroy &&
               (!Game.editing || (_position.Y <= Map.gameLinePos.Y && _directionLayer - Game.currentDirectionLayer >= 0)) &&
               (_directionLayer == Game.currentDirectionLayer || Core.renderer.GetDisplayedCharacter(_position) == '\0'))
                Core.renderer.SetCharacter(_position, new RenderCharacter(character,
                    ColorScheme.GetColor("transparent"), character == SpeedChar ? SpeedColor() : NormalColor()));
        }
        public void Simulate() {
            if(removed || toDestroy || Game.editing || !Game.StepPassedLine(step)) return;

            if(character == SpeedChar || ignore) toDestroy = true;
            else if(Game.StepPassedLine(step, character == HoldChar ? hitRange : missRange)) {
                Miss();
                ScoreManager.RecalculateAccuracy();
                removed = true;
            }
            else if(Game.auto || character == HoldChar) CheckPress();
        }

        public void CheckPress() {
            if(removed || toDestroy || ignore) return;
            if(Game.auto || Keyboard.IsKeyPressed(key)) CheckHit();
        }
        public void Step() {
            if(removed || toDestroy) return;
            _position = new Vector2i(_position.X, startPosition.Y + Game.roundedOffset);
            if(Game.editing && SoundManager.music.Status == SoundStatus.Playing && step == (int)Game.steps)
                PlayHitsound();
        }
        
        private void CheckHit() {
            if(Game.StepPassedLine(step, character == HoldChar ? 0 : -hitRange)) Hit();
            else Miss();
            PlayHitsound();
            ScoreManager.RecalculateAccuracy();
            removed = true;
        }
        private void PlayHitsound() {
            if(character == SpeedChar || ignore || removed || toDestroy) return;
            SoundManager.PlaySound(character == HoldChar ? SoundType.Hold : SoundType.Hit);
        }
        private void Hit() {
            bool perfect = Math.Abs(step - Game.roundedSteps) < perfectRange || character == HoldChar;
            Game.health += Map.currentLevel.metadata.hpRestorage / (perfect ? 1 : 2);
            int score = perfect ? 10 : 5;
            ScoreManager.combo++;
            ScoreManager.maxCombo = Math.Max(ScoreManager.combo, ScoreManager.maxCombo);
            ScoreManager.score += score * ScoreManager.combo;
            ScoreManager.scores[score / 5]++;
            _hitColor = perfect ? ColorScheme.GetColor("perfect_hit") : ColorScheme.GetColor("hit");
        }
        private void Miss() {
            Game.health -= Map.currentLevel.metadata.hpDrain;
            ScoreManager.combo = 0;
            ScoreManager.scores[0]++;
            _hitColor = ColorScheme.GetColor("miss");
        }

        private Color SpeedColor() => NormalColor(_directionLayer, Game.currentDirectionLayer, speedColor,
            nextDirLayerSpeedColor, Map.OffsetSelected(Calc.StepsToOffset(step)));
        private Color NormalColor() => NormalColor(_directionLayer, Game.currentDirectionLayer, _color, _nextDirLayerColor,
            Map.OffsetSelected(Calc.StepsToOffset(step)));
        private static Color NormalColor(int noteDirLayer, int curDirLayer, Color color, Color nextDirLayerColor, bool selected) {
            int difference = Math.Abs(noteDirLayer - curDirLayer);
            if(selected && difference <= 1) return ColorScheme.GetColor("selected_note");
            return difference switch {
                0 => color,
                1 => nextDirLayerColor,
                _ => ColorScheme.GetColor("transparent")
            };
        }
    }
}
