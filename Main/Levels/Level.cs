using System;
using System.Collections.Generic;
using System.Linq;

using PPR.GUI;
using PPR.Properties;
using PPR.Rendering;

using SFML.Audio;
using SFML.Graphics;
using SFML.Window;

namespace PPR.Main.Levels {
    public struct LevelScore {
        public Vector2 scorePosition;
        public readonly int score;
        public readonly string scoreStr;
        public Vector2 accComboPosition;
        public readonly int accuracy;
        public readonly int maxCombo;
        public readonly string accuracyStr;
        public Color accuracyColor;
        public Vector2 accComboDividerPosition;
        public Vector2 maxComboPosition;
        public readonly string maxComboStr;
        public Color maxComboColor;
        public Vector2 scoresPosition;
        public readonly int[] scores;
        public Vector2 linePosition;

        public LevelScore(Vector2 position, int score, int accuracy, int maxCombo, int[] scores) {
            scorePosition = position;
            this.score = score;
            scoreStr = $"SCORE: {score.ToString()}";
            accComboPosition = new Vector2(position.x, position.y + 1);
            this.accuracy = accuracy;
            this.maxCombo = maxCombo;
            accuracyStr = $"{accuracy.ToString()}%";
            accuracyColor = Game.GetAccuracyColor(accuracy);
            accComboDividerPosition = accComboPosition + new Vector2(accuracyStr.Length, 0);
            maxComboPosition = accComboDividerPosition + new Vector2(1, 0);
            maxComboStr = $"{maxCombo.ToString()}x";
            maxComboColor = Game.GetComboColor(accuracy, scores[0]);
            scoresPosition = new Vector2(position.x, position.y + 2);
            this.scores = scores;
            linePosition = new Vector2(position.x - 1, position.y + 3);
        }
    }
    public struct LevelMetadata {
        public readonly string name;
        public int hpDrain;
        public int hpRestorage;
        public readonly string difficulty;
        public readonly string author;
        public readonly string length;
        public readonly int maxStep;
        public int linesFrequency;
        public int initialOffsetMs;
        public readonly string bpm;
        public readonly bool skippable;
        public readonly int skipTime;
        public readonly int objectCount;
        public readonly int speedsCount;

        LevelMetadata(string name, IReadOnlyList<string> meta, int objectCount, IReadOnlyList<char> chars, IReadOnlyList<int> steps, List<LevelSpeed> speeds) {
            this.name = name;
            hpDrain = int.Parse(meta[0]);
            hpRestorage = int.Parse(meta[1]);
            //difficulty = meta[2];
            author = meta[3];
            linesFrequency = meta.Count > 4 ? int.Parse(meta[4]) : 4;
            initialOffsetMs = meta.Count > 5 ? int.Parse(meta[5]) : 0;

            //speeds = Game.SortLevelSpeeds(speeds);
            speeds.Sort((speed1, speed2) => speed1.step.CompareTo(speed2.step));

            difficulty = GetDifficulty(chars, steps, speeds).ToString();

            maxStep = steps.Count > 0 ? steps.Max() : 0;
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(Game.StepsToMilliseconds(maxStep, speeds) - initialOffsetMs);
            length =
                $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{timeSpan.ToString($"{(timeSpan.Hours != 0 ? "h':'mm" : "m")}':'ss")}";

            int minStep = steps.Count > 0 ? steps.Min() : 0;
            int minTime = (int)Game.StepsToMilliseconds(minStep, speeds) + initialOffsetMs;
            skipTime = minTime - 3000;
            skippable = skipTime > 3000;

            IEnumerable<int> onlyAbsoluteSpeeds = speeds.Select(speed => Math.Abs(speed.speed));
            int minBPM = onlyAbsoluteSpeeds.Min();
            int maxBPM = onlyAbsoluteSpeeds.Max();
            int avgBPM = (int)Math.Floor(onlyAbsoluteSpeeds.Average());
            string avgBPMStr = avgBPM.ToString();
            string minmaxBPMStr = $"{minBPM.ToString()}-{maxBPM.ToString()}";
            bpm = minBPM == maxBPM ? avgBPMStr : $"{minmaxBPMStr} ({avgBPMStr})";

            this.objectCount = objectCount;
            speedsCount = speeds.Count;
        }
        public LevelMetadata(Level level, IReadOnlyList<string> meta, string name) : this(name, meta,
            level.objects.FindAll(obj => obj.character != LevelObject.HOLD_CHAR).Count,
            level.objects.FindAll(obj => obj.character != LevelObject.SPEED_CHAR).Select(obj => obj.character).ToList(),
            level.objects.FindAll(obj => obj.character != LevelObject.SPEED_CHAR).Select(obj => obj.step).ToList(), level.speeds) { }
        static List<LevelSpeed> SpeedsFromLists(IReadOnlyList<int> speeds, IEnumerable<int> speedsSteps) {
            List<LevelSpeed> combinedSpeeds = speedsSteps.Select((t, i) => new LevelSpeed(speeds[i], t)).ToList();
            combinedSpeeds.Sort((speed1, speed2) => speed1.step.CompareTo(speed2.step));
            return combinedSpeeds;
        }
        static int GetDifficulty(IReadOnlyList<char> chars, IReadOnlyList<int> steps, List<LevelSpeed> sortedSpeeds) {
            List<LightLevelObject> sortedObjects = new List<LightLevelObject>();
            for(int i = 0; i < Math.Min(chars.Count, steps.Count); i++) sortedObjects.Add(new LightLevelObject(chars[i], steps[i]));
            sortedObjects.Sort((obj1, obj2) => obj1.step.CompareTo(obj2.step));
            for(int i = 1; i < sortedObjects.Count; i++) if(sortedObjects[i].character == LevelObject.HOLD_CHAR) sortedObjects.RemoveAt(i - 1);
            sortedObjects = sortedObjects.FindAll(obj => obj.character != LevelObject.HOLD_CHAR);
            //for(int i = 1; i < sortedObjects.Count; i++) if(sortedObjects[i].step == sortedObjects[i - 1].step) sortedObjects.RemoveAt(i);

            List<float> diffFactors = new List<float>();

            List<float> timeDifferences = new List<float>();
            for(int i = 1; i < sortedObjects.Count; i++) {
                LightLevelObject obj = sortedObjects[i];
                LightLevelObject prevObj = sortedObjects[i - 1];
                if(obj.step - prevObj.step == 0) continue;
                timeDifferences.Add(Game.StepsToMilliseconds(obj.step, sortedSpeeds) / 1000f -
                    Game.StepsToMilliseconds(prevObj.step, sortedSpeeds) / 1000f);
            }
            diffFactors.Add(timeDifferences.Count == 0 ? 0 : 1f / ((timeDifferences.Average() + timeDifferences.Min()) / 2f));

            List<float> timeFrames = (from obj in sortedObjects
                                      select Math.Abs(GetBPMAtStep(obj.step, sortedSpeeds)) into bpm
                                      let rangeModifier = (int)(bpm / 600f / 1.5f) + 1
                                      select bpm / rangeModifier into feelBpm
                                      select 60f / feelBpm).ToList();
            diffFactors.Add(timeFrames.Count == 0 ? 0 : 1f / ((timeFrames.Average() + timeFrames.Min()) / 2f));

            List<float> keyDistances = new List<float>();
            for(int i = 1; i < sortedObjects.Count; i++) {
                LightLevelObject obj = sortedObjects[i];
                LightLevelObject prevObj = sortedObjects[i - 1];
                keyDistances.Add(LevelObject.GetKeyboardKeyDistance(obj.character, prevObj.character));
            }
            diffFactors.Add(keyDistances.Count == 0 ? 0 : (keyDistances.Average() + keyDistances.Max()) / 16f);

            return (int)diffFactors.Average();
        }
        static int GetBPMAtStep(int step, IEnumerable<LevelSpeed> sortedSpeeds) {
            int bpm = 0;
            foreach(LevelSpeed speed in sortedSpeeds)
                if(speed.step <= step) bpm = speed.speed;
                else break;
            return bpm;
        }
        public LevelMetadata(IReadOnlyList<string> lines, string name) : this(name, lines[4].Split(':'),
            lines[0].ToList().FindAll(obj => obj != LevelObject.HOLD_CHAR).Count, lines[0].ToList(),
            lines[1].Length > 0 ? lines[1].Split(':').Select(int.Parse).ToList() : new List<int>(),
            SpeedsFromLists(
                lines[2].Length > 0 ? lines[2].Split(':').Select(int.Parse).ToList() : new List<int>(),
                lines[3].Length > 0 ? lines[3].Split(':').Select(int.Parse).ToList() : new List<int>())) { }
    }
    public class Level {
        public LevelMetadata metadata;
        public readonly List<LevelObject> objects = new List<LevelObject>();
        public readonly List<LevelSpeed> speeds = new List<LevelSpeed>();

        public Level(IReadOnlyList<string> lines, string name) {
            int[] objectsSteps = lines[1].Length > 0 ? lines[1].Split(':').Select(int.Parse).ToArray() : new int[0];
            int[] speeds = lines[2].Length > 0 ? lines[2].Split(':').Select(int.Parse).ToArray() : new int[0];
            int[] speedsStarts = lines[3].Length > 0 ? lines[3].Split(':').Select(int.Parse).ToArray() : new int[0];
            for(int i = 0; i < speedsStarts.Length; i++) {
                this.speeds.Add(new LevelSpeed(speeds[i], speedsStarts[i]));
                objects.Add(new LevelObject(LevelObject.SPEED_CHAR, speedsStarts[i], this.speeds));
            }
            for(int i = 0; i < objectsSteps.Length; i++) {
                int step = objectsSteps[i];
                objects.Add(new LevelObject(lines[0][i], step, this.speeds, objects));
            }
            this.speeds.Sort((speed1, speed2) => speed1.step.CompareTo(speed2.step));
            string[] meta = lines[4].Split(':');
            metadata = new LevelMetadata(this, meta, name);
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
    public class LightLevelObject {
        public readonly char character;
        public readonly int step;
        public LightLevelObject(char character, int step) {
            this.character = character;
            this.step = step;
        }
    }
    public class LevelObject {
        static readonly string[] lines = new string[] {
            "1234567890-=", // ReSharper disable once StringLiteralTypo
            "qwertyuiop[]", // ReSharper disable once StringLiteralTypo
            "asdfghjkl;'", // ReSharper disable once StringLiteralTypo
            "zxcvbnm,./"
        };
        public static Color[] linesColors;
        public static Color[] linesDarkColors;
        public const char SPEED_CHAR = '>';
        public const char HOLD_CHAR = '│';
        public static int perfectRange;
        public static int hitRange;
        public static int missRange;
        Vector2 _position;
        readonly Vector2 _startPosition;
        public readonly char character;
        readonly Keyboard.Key _key;
        public readonly int step;
        readonly int _directionLayer;
        Color _hitColor;
        readonly int _line;
        Color _color;
        Color _nextDirLayerColor;
        public static Color speedColor;

        public bool removed;
        float _removeAnimationTime;
        public bool toDestroy;

        public bool ignore;

        static int GetXPosForCharacter(char character) {
            character = char.ToLower(character);
            int x = 0;
            int xLineOffset = 0;
            int mul = 90 / lines.Select(line => line.Length).Max();
            foreach(string line in lines) {
                if(line.Contains(character)) {
                    x = (line.IndexOf(character) + 1) * (mul - 1) + xLineOffset * mul / 3;
                    break;
                }
                xLineOffset++;
            }
            return x;
        }
        public static float GetKeyboardKeyDistance(char leftChar, char rightChar) {
            int leftX = GetXPosForCharacter(leftChar);
            int rightX = GetXPosForCharacter(rightChar);
            int leftY = 0;
            int rightY = 0;
            int lineOffset = 0;
            foreach(string line in lines) {
                if(line.Contains(leftChar)) leftY = lineOffset;
                if(line.Contains(rightChar)) leftY = lineOffset;
                lineOffset++;
            }
            return MathF.Sqrt((leftX + rightX) * (leftX + rightX) + (leftY + rightY) * (leftY + rightY));
        }
        public LevelObject(char character, int step, IEnumerable<LevelSpeed> speeds, IReadOnlyCollection<LevelObject> objects = null) {
            character = Settings.Default.uppercaseNotes ? char.ToUpper(character) : char.ToLower(character);
            int x = GetXPosForCharacter(character);
            if(character == HOLD_CHAR && objects != null) {
                List<LevelObject> existingObjects = new List<LevelObject>(objects);
                existingObjects.Sort((obj1, obj2) => -obj1.step.CompareTo(obj2.step));
                foreach(LevelObject obj in existingObjects.Where(obj =>
                    obj.step <= step && obj.character != SPEED_CHAR && obj.character != HOLD_CHAR)) {
                    x = obj._position.x;
                    _key = obj._key;
                    obj.ignore = true;
                    break;
                }
            }
            List<LevelSpeed> existingSpeeds = new List<LevelSpeed>(speeds);
            existingSpeeds.Sort((spd1, spd2) => spd1.step.CompareTo(spd2.step));
            _startPosition = new Vector2(x, Map.linePos.y - (int)Game.StepsToOffset(step, existingSpeeds));
            _position = new Vector2(_startPosition);
            this.character = character;
            char lineChar = character == HOLD_CHAR ? Game.GetNoteBinding(_key) : character;
            lineChar = char.ToLower(lineChar);
            foreach(string line in lines) {
                if(line.Contains(lineChar)) break;
                _line++;
            }
            UpdateColors();
            this.step = step;
            _directionLayer = Game.StepsToDirectionLayer(step, existingSpeeds);
            _key = char.ToUpper(character) switch {
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
                _ => _key
            };
        }
        public void UpdateColors() {
            if(character == SPEED_CHAR) return;
            _color = linesColors[_line];
            _nextDirLayerColor = linesDarkColors[_line];
        }

        Color NormalColor() {
            return NormalColor(_directionLayer, Game.currentDirectionLayer, _color, _nextDirLayerColor);
        }
        static Color NormalColor(int noteDirLayer, int curDirLayer, Color color, Color nextDirLayerColor) {
            int difference = Math.Abs(noteDirLayer - curDirLayer);
            return difference switch {
                0 => color,
                1 => nextDirLayerColor,
                _ => ColorScheme.GetColor("transparent")
            };
        }
        public void Draw() {
            if(removed && !ignore) {
                if(_removeAnimationTime <= 0f) {
                    List<LevelObject> samePosObjects = Map.currentLevel.objects.FindAll(obj =>
                        obj._position == _position && obj.removed && obj != this);
                    if(samePosObjects.Count > 0) samePosObjects.ForEach(obj => obj.toDestroy = true);
                }
                Renderer.instance.SetCellColor(_position,
                    Renderer.AnimateColor(_removeAnimationTime, _hitColor, ColorScheme.GetColor("foreground"), 3f),
                    Renderer.AnimateColor(_removeAnimationTime, _hitColor, ColorScheme.GetColor("transparent"), 3f));
                if(_removeAnimationTime >= 1f) toDestroy = true;
                _removeAnimationTime += Core.deltaTime;
                return;
            }
            if(!ignore && !toDestroy && (!Game.editing || !Game.StepPassedLine(step, 1)) &&
                (_directionLayer == Game.currentDirectionLayer || Renderer.instance.GetCharacter(_position) == '\0'))
                Renderer.instance.SetCharacter(_position, character, character == SPEED_CHAR ? speedColor : NormalColor(),
                    ColorScheme.GetColor("transparent"));
        }
        public void Simulate() {
            if(removed || toDestroy || Game.editing || !Game.StepPassedLine(step)) return;

            if(character == SPEED_CHAR || ignore) toDestroy = true;
            else if(Game.StepPassedLine(step, character == HOLD_CHAR ? hitRange : missRange)) {
                Miss();
                Game.RecalculateAccuracy();
                removed = true;
            }
            else if(Game.auto || character == HOLD_CHAR) CheckPress();
        }

        void CheckHit() {
            if(Game.StepPassedLine(step, character == HOLD_CHAR ? 0 : -hitRange)) Hit();
            else Miss();
            PlayHitsound();
            Game.RecalculateAccuracy();
            removed = true;
        }
        public void CheckPress() {
            if(removed || toDestroy || ignore) return;
            if(Game.auto || Keyboard.IsKeyPressed(_key)) CheckHit();
        }
        void PlayHitsound() {
            if(character == SPEED_CHAR || ignore || removed || toDestroy) return;
            if(character == HOLD_CHAR) Game.tickSound.Play();
            else Game.hitSound.Play();
        }
        void Hit() {
            bool perfect = Math.Abs(step - Game.roundedSteps) < perfectRange || character == HOLD_CHAR;
            Game.health += Map.currentLevel.metadata.hpRestorage / (perfect ? 1 : 2);
            int score = perfect ? 10 : 5;
            Game.combo++;
            Game.maxCombo = Math.Max(Game.combo, Game.maxCombo);
            Game.score += score * Game.combo;
            Game.scores[score / 5]++;
            _hitColor = perfect ? ColorScheme.GetColor("perfect_hit") : ColorScheme.GetColor("hit");
        }
        void Miss() {
            Game.health -= Map.currentLevel.metadata.hpDrain;
            Game.combo = 0;
            Game.scores[0]++;
            _hitColor = ColorScheme.GetColor("miss");
        }
        public void Step() {
            if(removed || toDestroy) return;
            _position.y = _startPosition.y + Game.roundedOffset;
            if(Game.editing && Game.music.Status == SoundStatus.Playing && step == (int)Game.steps)
                PlayHitsound();
        }


        public override bool Equals(object obj) {
            return obj is LevelObject @object &&
                   EqualityComparer<Vector2>.Default.Equals(_position, @object._position) &&
                   character == @object.character &&
                   step == @object.step;
        }
        public override int GetHashCode() {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return HashCode.Combine(_position, character, step);
        }
        public static bool operator ==(LevelObject left, LevelObject right) {
            return left?.Equals(right) ?? right is null;
        }
        public static bool operator !=(LevelObject left, LevelObject right) {
            return !(left == right);
        }
    }
}
