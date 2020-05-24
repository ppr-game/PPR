using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using DiscordRPC;

using PPR.Configuration;
using PPR.GUI;
using PPR.Levels;
using PPR.Rendering;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace PPR.Core {
    public enum Menu { Main, LevelSelect, Settings, LastStats, Game }
    public class Game {
        static Menu _currentMenu = Menu.Main;
        public static Menu currentMenu {
            get => _currentMenu;
            set {
                if(Map.currentLevel != null && Map.currentLevel.objects.Count > 0 && health > 0) {
                    if(_currentMenu == Menu.Game && value == Menu.LastStats) { // Pause
                        music.Pause();
                    }
                    else if(!editing && _currentMenu == Menu.LastStats && value == Menu.Game) { // Unpause
                        music.Play();
                    }
                }
                if(value == Menu.Game) {
                    if(auto) usedAuto = true;
                }
                if(value == Menu.LastStats && !usedAuto && Map.currentLevel.objects.Count <= 0 && health > 0) {
                    string path = Path.Combine("scores", Map.currentLevel.metadata.name + ".txt");
                    string text = File.Exists(path) ? File.ReadAllText(path) : "";
                    text = Map.TextFromScore(new LevelScore(Vector2.zero, score, accuracy, maxCombo, scores)) + "\n" + text;
                    _ = Directory.CreateDirectory(path);
                    File.WriteAllText(path, text);
                }
                if((value == Menu.Main || value == Menu.LevelSelect) && music.Status == SoundStatus.Paused) {
                    music.Play();
                }
                _currentMenu = value;
                switch(value) {
                    case Menu.Main:
                        RPC.client.SetPresence(new RichPresence() {
                            Details = "In main menu",
                            Timestamps = Timestamps.Now
                        });
                        break;
                    case Menu.LevelSelect:
                        RPC.client.SetPresence(new RichPresence() {
                            Details = "Choosing what to " + (editing ? "edit" : "play"),
                            Timestamps = Timestamps.Now
                        });
                        break;
                    case Menu.Game:
                        RPC.client.SetPresence(new RichPresence() {
                            Details = editing ? "Editing" : auto ? "Watching" : "Playing",
                            State = Map.currentLevel.metadata.name,
                            Timestamps = Timestamps.Now
                        });
                        break;
                    case Menu.LastStats:
                        RPC.client.SetPresence(new RichPresence() {
                            Details = "Looking at statistics",
                            State = Map.currentLevel.metadata.name,
                            Timestamps = Timestamps.Now
                        });
                        break;
                }
            }
        }
        public static LevelMetadata? selectedMetadata = null;
        public static Time time;
        static float _offset = 0f;
        public static float offset {
            set {
                _offset = value;
                if(!editing) UI.progress = (int)(value / Map.currentLevel.metadata.maxOffset * 80f);
            }
            get => _offset;
        }
        public static int roundedOffset = 0;
        public static float prevOffset = 0f;
        public static int currentBPM = 1;
        public static Music music = new Music(Path.Combine("resources", "audio", "mainMenu.ogg"));
        public static int score = 0;
        static int _health = 80;
        public static int health {
            get => _health;
            set {
                value = Math.Clamp(value, 0, 80);
                _health = value;
                UI.health = value;
            }
        }
        public static int accuracy = 100;
        public static int[] scores = new int[3]; // score / 5 = index
        public static int combo = 0;
        public static int maxCombo = 0;
        public static bool editing = false;
        public static bool auto = false;
        public static bool usedAuto = false;
        public void Start() {
            RPC.Initialize();
            Config.LoadConfig();

            music.Volume = Config.musicVolume;
            music.Play();
        }
        public void End() {
            Config.SaveConfig();
            Renderer.instance.window.Close();
        }
        public void Update() {
            if(currentMenu != Menu.Game) return;

            if(MathF.Floor(prevOffset) != MathF.Floor(offset)) {
                roundedOffset = (int)MathF.Round(offset);
                RecalculatePosition();
            }

            prevOffset = offset;

            if(music.Status == SoundStatus.Playing) {
                offset = MillisecondsToOffset(music.PlayingOffset.AsMilliseconds(), Map.currentLevel.speeds);
            }
            if(editing) UI.progress = (int)(music.PlayingOffset.AsSeconds() / music.Duration.AsSeconds() * 80f);
        }
        public static void GameStart(string musicPath) {
            usedAuto = auto;
            UI.progress = 80;
            offset = 0;
            roundedOffset = 0;
            prevOffset = 0;
            UI.health = 0;
            health = 80;
            score = 0;
            UI.prevScore = 0;
            scores = new int[3];
            accuracy = 100;
            combo = 0;
            maxCombo = 0;
            music.Stop();
            if(File.Exists(musicPath)) {
                music = new Music(musicPath) {
                    Volume = Config.musicVolume
                };
                if(!editing) music.Play();
            }
        }
        public static void RecalculatePosition() {
            for(int i = 0; i < Map.currentLevel.speeds.Count; i++) {
                if(Map.currentLevel.speeds[i].offset <= offset) {
                    currentBPM = Map.currentLevel.speeds[i].speed;
                }
            }
            Map.StepAll();
        }
        public static void GenerateLevelList() {
            UI.levelSelectLevels.Clear();
            string[] directories = Directory.GetDirectories("levels");
            List<Button> buttons = new List<Button>();
            for(int i = 0; i < directories.Length; i++) {
                string name = Path.GetFileName(directories[i]);
                if(name == "_template") continue;
                buttons.Add(new Button(new Vector2(25, 12 + i), name, 30, Color.Black, Color.White, Color.White));
            }
            UI.levelSelectLevels = buttons;
        }
        public static void RecalculateAccuracy() {
            float sum = scores[0] + scores[1] + scores[2];
            float mulSum = scores[1] * 0.5f + scores[2];
            accuracy = (int)MathF.Floor(mulSum / sum * 100f);
        }
        public static Color GetAccuracyColor(int accuracy) {
            return accuracy >= 100 ? Color.Green : accuracy >= 70 ? Color.Yellow : Color.Red;
        }
        public static Color GetComboColor(int accuracy, int misses) {
            return accuracy >= 100 ? Color.Green : misses <= 0 ? Color.Yellow : Color.Blue;
        }
        public void KeyPressed(object caller, KeyEventArgs key) {
            if(key.Code == Keyboard.Key.Escape) {
                if(currentMenu == Menu.Game) currentMenu = Menu.LastStats;
                else if(currentMenu == Menu.LastStats) {
                    currentMenu = Map.currentLevel.objects.Count > 0 ? Menu.Game : Menu.LevelSelect;
                }
                else if(currentMenu == Menu.LevelSelect || currentMenu == Menu.Settings) currentMenu = Menu.Main;
            }
            if(currentMenu == Menu.Game) {
                if(editing) {
                    char character = key.Code switch
                    {
                        Keyboard.Key.Num1 => '1',
                        Keyboard.Key.Num2 => '2',
                        Keyboard.Key.Num3 => '3',
                        Keyboard.Key.Num4 => '4',
                        Keyboard.Key.Num5 => '5',
                        Keyboard.Key.Num6 => '6',
                        Keyboard.Key.Num7 => '7',
                        Keyboard.Key.Num8 => '8',
                        Keyboard.Key.Num9 => '9',
                        Keyboard.Key.Num0 => '0',
                        Keyboard.Key.Hyphen => '-',
                        Keyboard.Key.Equal => '=',
                        Keyboard.Key.Q => 'q',
                        Keyboard.Key.W => 'w',
                        Keyboard.Key.E => 'e',
                        Keyboard.Key.R => 'r',
                        Keyboard.Key.T => 't',
                        Keyboard.Key.Y => 'y',
                        Keyboard.Key.U => 'u',
                        Keyboard.Key.I => 'i',
                        Keyboard.Key.O => 'o',
                        Keyboard.Key.P => 'p',
                        Keyboard.Key.LBracket => '[',
                        Keyboard.Key.RBracket => ']',
                        Keyboard.Key.A => 'a',
                        Keyboard.Key.S => 's',
                        Keyboard.Key.D => 'd',
                        Keyboard.Key.F => 'f',
                        Keyboard.Key.G => 'g',
                        Keyboard.Key.H => 'h',
                        Keyboard.Key.J => 'j',
                        Keyboard.Key.K => 'k',
                        Keyboard.Key.L => 'l',
                        Keyboard.Key.Semicolon => ';',
                        Keyboard.Key.Quote => '\'',
                        Keyboard.Key.Z => 'z',
                        Keyboard.Key.X => 'x',
                        Keyboard.Key.C => 'c',
                        Keyboard.Key.V => 'v',
                        Keyboard.Key.B => 'b',
                        Keyboard.Key.N => 'n',
                        Keyboard.Key.M => 'm',
                        Keyboard.Key.Comma => ',',
                        Keyboard.Key.Period => '.',
                        Keyboard.Key.Slash => '/',
                        _ => '\0'
                    };
                    if(character == '\0') {
                        if(key.Code == Keyboard.Key.Backspace) {
                            List<LevelObject> objects = Map.currentLevel.objects.FindAll(obj => obj.offset == roundedOffset &&
                                                                                                                                                                                                       obj.character != LevelObject.speedChar);
                            foreach(LevelObject obj in objects) {
                                _ = Map.currentLevel.objects.Remove(obj);
                            }
                        }
                        else if(key.Code == Keyboard.Key.Up || key.Code == Keyboard.Key.Down) {
                            int delta = key.Code == Keyboard.Key.Up ? 1 : -1;
                            if(key.Alt) {
                                Map.currentLevel.metadata.linesFrequency += delta;
                            }
                            else {
                                if(!Map.currentLevel.speeds.Select(speed => speed.offset).Contains(roundedOffset)) {
                                    int speedIndex = 0;
                                    for(int i = 0; i < Map.currentLevel.speeds.Count; i++) {
                                        if(Map.currentLevel.speeds[i].offset <= roundedOffset) speedIndex = i;
                                    }
                                    Map.currentLevel.speeds.Add(new LevelSpeed(Map.currentLevel.speeds[speedIndex].speed, roundedOffset));
                                    Map.currentLevel.speeds.Sort((speed1, speed2) => speed1.offset.CompareTo(speed2.offset));
                                }

                                int index = Map.currentLevel.speeds.Select(speed => speed.offset).ToList().IndexOf(roundedOffset);

                                Map.currentLevel.speeds[index].speed += (key.Shift ? 1 : 10) * delta;

                                if(index >= 1 && Map.currentLevel.speeds[index].speed == Map.currentLevel.speeds[index - 1].speed) {
                                    Map.currentLevel.speeds.RemoveAt(index);
                                }

                                List<LevelObject> speedObjects = Map.currentLevel.objects.FindAll(obj => obj.character == LevelObject.speedChar);
                                foreach(LevelObject obj in speedObjects) {
                                    _ = Map.currentLevel.objects.Remove(obj);
                                }
                                for(int i = 0; i < Map.currentLevel.speeds.Count; i++) {
                                    Map.currentLevel.objects.Add(new LevelObject(LevelObject.speedChar, Map.currentLevel.speeds[i].offset));
                                }
                            }
                        }
                        else if(key.Code == Keyboard.Key.Left || key.Code == Keyboard.Key.Right) {
                            if(key.Shift) {
                                Map.currentLevel.metadata.hpRestorage += key.Code == Keyboard.Key.Right ? 1 : -1;
                            }
                            else {
                                Map.currentLevel.metadata.hpDrain += key.Code == Keyboard.Key.Right ? 1 : -1;
                            }
                        }
                    }
                    else {
                        if(Map.currentLevel.objects.FindAll(obj => obj.character == character && obj.offset == roundedOffset).Count <= 0) {
                            Map.currentLevel.objects.Add(new LevelObject(character, roundedOffset));
                            if(key.Shift) {
                                character = LevelObject.holdChar;
                                Map.currentLevel.objects.Add(new LevelObject(character, roundedOffset, Map.currentLevel.objects));
                            }
                        }
                    }

                    RecalculatePosition();
                }
                else {
                    for(int y = Map.linePos.y + 2; y >= Map.linePos.y - 2; y--) {
                        if(CheckLine(y)) break;
                    }
                }
            }
        }
        public void LostFocus(object caller, EventArgs args) {
            if(currentMenu == Menu.Game) {
                currentMenu = Menu.LastStats;
            }
            music.Volume = 0;
        }
        public void GainedFocus(object caller, EventArgs args) {
            music.Volume = Config.musicVolume;
        }
        bool CheckLine(int y) {
            List<LevelObject> objects = Map.currentLevel.objects.FindAll(obj => obj.character != LevelObject.speedChar &&
                                                                                                                                                                                       obj.character != LevelObject.holdChar &&
                                                                                                                                                                                       !obj.removed &&
                                                                                                                                                                                       !obj.ignore &&
                                                                                                                                                                                       obj.position.y == y);
            for(int i = 0; i < objects.Count; i++) {
                objects[i].CheckPress();
                if(objects[i].removed) return true;
            }
            return false;
        }
        public void MouseWheelScrolled(object caller, MouseWheelScrollEventArgs scroll) {
            if(currentMenu == Menu.LevelSelect) {
                Vector2 mousePos = Renderer.instance.mousePosition;
                if(mousePos.y >= 12 && mousePos.y <= 49) {
                    if(mousePos.x >= 28 && mousePos.x <= 51) {
                        if(scroll.Delta > 0 && UI.levelSelectLevels.First().position.y >= 12) return;
                        if(scroll.Delta < 0 && UI.levelSelectLevels.Last().position.y <= 49) return;
                        foreach(Button button in UI.levelSelectLevels) {
                            button.position.y += (int)scroll.Delta;
                        }
                    }
                    else if(mousePos.x >= 1 && mousePos.x <= 26) {
                        if(scroll.Delta > 0 && UI.levelSelectScores.First().scorePosition.y >= 12) return;
                        if(scroll.Delta < 0 && UI.levelSelectScores.Last().scoresPosition.y <= 49) return;
                        foreach(LevelScore score in UI.levelSelectScores) {
                            int increment = (int)scroll.Delta;
                            score.scorePosition.y += increment;
                            score.accComboPosition.y += increment;
                            score.accComboDividerPosition.y += increment;
                            score.maxComboPosition.y += increment;
                            score.scoresPosition.y += increment;
                            score.linePosition.y += increment;
                        }
                    }
                }
            }
            else if(currentMenu == Menu.Game && editing) {
                offset = roundedOffset;
                offset += scroll.Delta;
                RecalculateTime();
            }
        }
        public static void RecalculateTime() {
            long useMicrosecs = (long)(Math.Abs(OffsetToMilliseconds(offset, Map.currentLevel.speeds)) * 1000f);
            time = Time.FromMicroseconds(useMicrosecs);
            music.PlayingOffset = time;
        }
        public static float OffsetToMilliseconds(float offset, List<LevelSpeed> sortedSpeeds) {
            float useOffset = offset;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++) {
                if(sortedSpeeds[i].offset <= useOffset) speedIndex = i;
            }
            float time = 0;
            for(int i = 0; i <= speedIndex; i++) {
                if(i != speedIndex) time += (sortedSpeeds[i + 1].offset - sortedSpeeds[i].offset) * (60000f / sortedSpeeds[i].speed);
                else time += (useOffset - sortedSpeeds[i].offset) * (60000f / sortedSpeeds[speedIndex].speed);
            }
            return time;
        }
        public static float MillisecondsToOffset(float time, List<LevelSpeed> sortedSpeeds) {
            float useTime = time;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++) {
                if(OffsetToMilliseconds(sortedSpeeds[i].offset, sortedSpeeds) <= useTime) speedIndex = i;
                else break;
            }
            float offset = 0;
            for(int i = 0; i <= speedIndex; i++) {
                if(i != speedIndex) {
                    int increment = sortedSpeeds[i + 1].offset - sortedSpeeds[i].offset;
                    offset += increment;
                    useTime -= increment * (60000f / sortedSpeeds[i].speed);
                }
                else offset += useTime / (60000f / sortedSpeeds[i].speed);
            }
            return offset;
        }
    }
    public static class RPC {
        public static DiscordRpcClient client;
        public static void Initialize() {
            client = new DiscordRpcClient("699266677698723941");
            client.OnError += (sender, e) => Debug.Fail(e.Message);
            _ = client.Initialize();
            client.SetPresence(new RichPresence() {
                Details = "In main menu",
                Timestamps = Timestamps.Now
            });
        }
    }

    public class Vector2 {
        public static Vector2 zero => new Vector2();
        public static Vector2 one => new Vector2(1, 1);
        public static Vector2 left => new Vector2(-1, 0);
        public static Vector2 right => new Vector2(1, 0);
        public static Vector2 up => new Vector2(0, -1);
        public static Vector2 down => new Vector2(0, 1);

        public int x;
        public int y;
        public Vector2 normalized => new Vector2(x == 0 ? 0 : (x == 1 ? 1 : -1), y == 0 ? 0 : (y == 1 ? 1 : -1));
        public Vector2() {
            x = 0;
            y = 0;
        }
        public Vector2(int x, int y) {
            this.x = x;
            this.y = y;
        }
        public Vector2(Vector2 vector) {
            x = vector.x;
            y = vector.y;
        }

        public void Normalize() {
            x = x == 0 ? 0 : (x == 1 ? 1 : -1);
            y = y == 0 ? 0 : (y == 1 ? 1 : -1);
        }
        public Vector2 Abs() {
            return new Vector2(Math.Abs(x), Math.Abs(y));
        }
        public bool InBounds(int minX, int minY, int maxX, int maxY) {
            return x >= minX && x <= maxX && y >= minY && y <= maxY;
        }
        public bool InBounds(Vector2 min, Vector2 max) {
            return InBounds(min.x, min.y, max.x, max.y);
        }
        public override string ToString() {
            return "(" + x + ", " + y + ")";
        }

        public static Vector2 operator +(Vector2 left, Vector2 right) {
            return new Vector2(left.x + right.x, left.y + right.y);
        }
        public static Vector2 operator -(Vector2 left, Vector2 right) {
            return new Vector2(left.x - right.x, left.y - right.y);
        }
        public static Vector2 operator *(Vector2 left, int right) {
            return new Vector2(left.x * right, left.y * right);
        }
        public static Vector2 operator /(Vector2 left, int right) {
            return new Vector2(left.x / right, left.y / right);
        }
        public static bool operator ==(Vector2 left, Vector2 right) {
            return left.x == right.x && left.y == right.y;
        }
        public static bool operator !=(Vector2 left, Vector2 right) {
            return left.x != right.x || left.y != right.y;
        }
        public override bool Equals(object obj) {
            return obj is Vector2 vector &&
                   x == vector.x &&
                   y == vector.y;
        }
        public override int GetHashCode() {
            return HashCode.Combine(x, y);
        }
    }

    public static class Random_Extensions {
        public static float NextFloat(this Random rng, float min, float max) {
            return (float)rng.NextDouble() * (max - min) + min;
        }
    }
    public static class IEnumerable_Extensions {
        public static T ElementAtOrDefault<T>(this IList<T> list, int index, Func<T> @default) {
            return index >= 0 && index < list.Count ? list[index] : @default();
        }
    }
}
