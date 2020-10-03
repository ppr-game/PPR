using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using PPR.GUI;
using PPR.Properties;

using PRR;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace PPR.Main.Levels {
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
            accuracyColor = Game.GetAccuracyColor(accuracy);
            accComboDividerPosition = accComboPosition + new Vector2i(accuracyStr.Length, 0);
            maxComboPosition = accComboDividerPosition + new Vector2i(1, 0);
            maxComboStr = $"{maxCombo.ToString()}x";
            maxComboColor = Game.GetComboColor(accuracy, scores[0]);
            scoresPosition = new Vector2i(position.X, position.Y + 2);
            this.scores = scores;
            linePosition = new Vector2i(position.X - 1, position.Y + 3);
        }
    }
    public struct LevelMetadata {
        public readonly string name;
        public int hpDrain;
        public int hpRestorage;
        public string difficulty;
        public readonly string author;
        public string length;
        public int maxStep;
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

            maxStep = steps.Count > 0 ? steps.Max() : 0;
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(Game.StepsToMilliseconds(maxStep, speeds) - initialOffsetMs);
            length = $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{timeSpan.ToString($"{(timeSpan.Hours != 0 ? "h':'mm" : "m")}':'ss")}";

            List<LightLevelObject> objects = new List<LightLevelObject>();
            for(int i = 0; i < Math.Min(chars.Count, steps.Count); i++)
                objects.Add(new LightLevelObject(chars[i], steps[i]));
            difficulty = GetDifficulty(objects, speeds, (int)timeSpan.TotalMinutes)
                .ToString("0.00", CultureInfo.InvariantCulture);

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
        public static float GetDifficulty(List<LevelObject> objects, List<LevelSpeed> sortedSpeeds, int lengthMins) => GetDifficulty(objects.Select(obj => new LightLevelObject(obj.character, obj.step)).ToList(), sortedSpeeds,
            lengthMins);
        static float GetDifficulty(IReadOnlyCollection<LightLevelObject> lightObjects, List<LevelSpeed> sortedSpeeds,
            int lengthMins) {
            if (lightObjects.Count == 0 || sortedSpeeds.Count == 0) return 0f;

            List<LightLevelObject> sortedObjects = new List<LightLevelObject>(lightObjects);
            sortedObjects.Sort((obj1, obj2) => obj1.step.CompareTo(obj2.step));
            for(int i = 1; i < sortedObjects.Count; i++) if(sortedObjects[i].character == LevelObject.HOLD_CHAR) sortedObjects.RemoveAt(i - 1);
            sortedObjects = sortedObjects.FindAll(obj => obj.character != LevelObject.HOLD_CHAR);
            //for(int i = 1; i < sortedObjects.Count; i++) if(sortedObjects[i].step == sortedObjects[i - 1].step) sortedObjects.RemoveAt(i);

            List<float> diffFactors = new List<float>();
            
            List<float> speeds = new List<float>();
            List<float> bpm = new List<float>();
            
            List<LightLevelObject>[] objects = {
                new List<LightLevelObject>(),
                new List<LightLevelObject>(),
                new List<LightLevelObject>(),
                new List<LightLevelObject>(),
                new List<LightLevelObject>(),
                new List<LightLevelObject>(),
                new List<LightLevelObject>(),
                new List<LightLevelObject>(),
                new List<LightLevelObject>(),
                new List<LightLevelObject>(),
                new List<LightLevelObject>(),
                new List<LightLevelObject>()
            };
            foreach(LightLevelObject obj in sortedObjects)
                objects[(LevelObject.GetXPosForCharacter(obj.character) - 6) / 12].Add(obj);
            //List<LightLevelObject> objs = objects[0];
            foreach(List<LightLevelObject> objs in objects)
                for(int i = 1; i < objs.Count; i++) {
                    LightLevelObject prevObj = objs[i - 1];
                    LightLevelObject currObj = objs[i];
                    int startBPM = Math.Abs(Game.GetBPMAtStep(prevObj.step, sortedSpeeds));
                    int endBPM = Math.Abs(Game.GetBPMAtStep(currObj.step, sortedSpeeds));
                    int currStep = prevObj.step - startBPM / 600;
                    int endStep = currObj.step + endBPM / 600;
                    float time = 0;
                    int currBPM = startBPM;
                    foreach(LevelSpeed speed in Game.GetSpeedsBetweenSteps(prevObj.step, currObj.step,
                        sortedSpeeds)) {
                        time += 60f / currBPM * (speed.step - currStep);
                        currStep = speed.step;
                        currBPM = Math.Abs(speed.speed);
                    }
                    time += 60f / endBPM * (endStep - currStep);
                    float distance = LevelObject.GetPhysicalKeyDistance(currObj.character, prevObj.character);
                    speeds.Add(distance + 1f);
                    if(time != 0f) speeds.Add(1f / time);
                }

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach(LevelSpeed speed in sortedSpeeds) bpm.Add(Math.Abs(speed.speed) / 60f);
            
            diffFactors.Add(speeds.Average());
            diffFactors.Add(bpm.Average());
            diffFactors.Add(lengthMins);

            return diffFactors.Count > 0 ? diffFactors.Average() : 0f;
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
        public readonly string script;

        public Level(IReadOnlyList<string> lines, string name, string script = null) {
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
        Vector2i _position;
        readonly Vector2i _startPosition;
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

        public bool ignore { get; private set; }

        public static int GetXPosForCharacter(char character) {
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
        public static float GetPhysicalKeyDistance(char leftChar, char rightChar) {
            int leftX = GetXPosForCharacter(leftChar);
            int rightX = GetXPosForCharacter(rightChar);
            int leftY = 0;
            int rightY = 0;
            int lineOffset = 0;
            foreach(string line in lines) {
                if(line.Contains(leftChar)) leftY = lineOffset;
                if(line.Contains(rightChar)) rightY = lineOffset;
                lineOffset++;
            }
            return MathF.Sqrt((leftX - rightX) / 6f * ((leftX - rightX) / 6f) + (leftY - rightY) * (leftY - rightY));
        }
        public LevelObject(char character, int step, IEnumerable<LevelSpeed> speeds, IReadOnlyCollection<LevelObject> objects = null) {
            character = Settings.GetBool("uppercaseNotes") ? char.ToUpper(character) : char.ToLower(character);
            int x = GetXPosForCharacter(character);
            if(character == HOLD_CHAR && objects != null) {
                List<LevelObject> existingObjects = new List<LevelObject>(objects);
                existingObjects.Sort((obj1, obj2) => -obj1.step.CompareTo(obj2.step));
                foreach(LevelObject obj in existingObjects.Where(obj =>
                    obj.step <= step && obj.character != SPEED_CHAR && obj.character != HOLD_CHAR)) {
                    x = obj._position.X;
                    _key = obj._key;
                    obj.ignore = true;
                    break;
                }
            }
            List<LevelSpeed> existingSpeeds = new List<LevelSpeed>(speeds);
            existingSpeeds.Sort((spd1, spd2) => spd1.step.CompareTo(spd2.step));
            _startPosition = new Vector2i(x, Map.linePos.Y - (int)Game.StepsToOffset(step, existingSpeeds));
            _position = _startPosition;
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

        Color NormalColor() => NormalColor(_directionLayer, Game.currentDirectionLayer, _color, _nextDirLayerColor);
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
                    ColorScheme.GetColor("transparent"), character == SPEED_CHAR ? speedColor : NormalColor()));
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
            _position = new Vector2i(_position.X, _startPosition.Y + Game.roundedOffset);
            if(Game.editing && Game.music.Status == SoundStatus.Playing && step == (int)Game.steps)
                PlayHitsound();
        }


        public override bool Equals(object obj) => obj is LevelObject @object &&
                                                   EqualityComparer<Vector2i>.Default.Equals(_position, @object._position) &&
                                                   character == @object.character &&
                                                   step == @object.step;
        public override int GetHashCode() =>
            // ReSharper disable NonReadonlyMemberInGetHashCode
            HashCode.Combine(_position, character, step);
        public static bool operator ==(LevelObject left, LevelObject right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(LevelObject left, LevelObject right) => !(left == right);
    }
}
