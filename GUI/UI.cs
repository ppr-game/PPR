using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

using PPR.GUI.Elements;
using PPR.Main;
using PPR.Main.Levels;
using PPR.Properties;
using PPR.Rendering;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;

namespace PPR.GUI {
    public static class UI {
        public static int fps = 0;

        static readonly Color[] prevHealthColors = new Color[80];
        static readonly Color[] healthColors = new Color[80];
        static readonly float[] healthAnimTimes = new float[80];
        static readonly float[] healthAnimRateOffsets = new float[80];
        public static int health {
            set {
                for(int x = 0; x < 80; x++) {
                    Color color = value > x ? ref ColorScheme.red : ref ColorScheme.darkRed;
                    if(healthColors[x] != color) {
                        prevHealthColors[x] = healthColors[x];
                        healthAnimTimes[x] = 0f;
                        healthAnimRateOffsets[x] = new Random().NextFloat(-3f, 3f);
                    }
                    healthColors[x] = color;
                }
            }
        }

        static readonly Color[] prevProgressColors = new Color[80];
        static readonly Color[] progressColors = new Color[80];
        static readonly float[] progressAnimTimes = new float[80];
        static readonly float[] progressAnimRateOffsets = new float[80];
        public static int progress {
            set {
                for(int x = 0; x < 80; x++) {
                    Color color = value > x ? ref ColorScheme.white : ref ColorScheme.darkGray;
                    if(progressColors[x] != color) {
                        prevProgressColors[x] = progressColors[x];
                        progressAnimTimes[x] = 0f;
                        progressAnimRateOffsets[x] = new Random().NextFloat(-3f, 3f);
                    }
                    progressColors[x] = color;
                }
            }
        }

        static readonly string mainMenuText = File.ReadAllText(Path.Combine("resources", "ui", "mainMenu.txt"));
        static readonly string settingsText = File.ReadAllText(Path.Combine("resources", "ui", "settings.txt"));
        static readonly string keybindsEditorText = File.ReadAllText(Path.Combine("resources", "ui", "keybinds.txt"));
        static readonly string levelSelectText = File.ReadAllText(Path.Combine("resources", "ui", "levelSelect.txt"));
        static readonly string lastStatsText = File.ReadAllText(Path.Combine("resources", "ui", "lastStats.txt"));
        static List<Button> _mainMenuButtons;

        public static int currentLevelSelectIndex;
        public static List<Button> levelSelectLevels;
        public static List<List<LevelScore>> levelSelectScores;
        public static List<LevelMetadata?> levelSelectMetadatas;
        static List<Button> _levelSelectButtons;

        static string _lastLevel = "";
        static List<Button> _lastStatsButtons;

        static List<Button> _levelEditorButtons;

        static Button _skipButton;

        static readonly Vector2 zero = new Vector2();
        public static void RecreateButtons() {
            const Renderer.Alignment center = Renderer.Alignment.Center;
            const Renderer.Alignment right = Renderer.Alignment.Right;
            _mainMenuButtons = new List<Button> {
                new Button(new Vector2(40, 25), "PLAY", 4, ColorScheme.black, ColorScheme.green, ColorScheme.lightDarkGreen, new InputKey("Enter"), center),
                new Button(new Vector2(40, 27), "EDIT", 4, ColorScheme.black, ColorScheme.yellow, ColorScheme.lightDarkYellow,
                    new InputKey("LShift,RShift"), center),
                new Button(new Vector2(40, 29), "SETTINGS", 8, ColorScheme.black, ColorScheme.blue, ColorScheme.lightDarkBlue, new InputKey("Tab"), center),
                new Button(new Vector2(40, 31), "EXIT", 4, ColorScheme.black, ColorScheme.red, ColorScheme.lightDarkRed, null, center),
            };
            _pauseMusicButton = new Button(new Vector2(1, 58), "►", 1, ColorScheme.black, ColorScheme.green, ColorScheme.lightDarkGreen,
                new InputKey("Space"));
            _switchMusicButton = new Button(new Vector2(3, 58), "»", 1, ColorScheme.black, ColorScheme.green, ColorScheme.lightDarkGreen,
                new InputKey("Right"));
            _levelSelectButtons = new List<Button> {
                new Button(new Vector2(25, 10), "AUTO", 4, ColorScheme.black, ColorScheme.blue, ColorScheme.lightDarkBlue, new InputKey("Tab")),
                new Button(new Vector2(25, 10), "NEW", 3, ColorScheme.black, ColorScheme.green, ColorScheme.lightDarkGreen,
                    new InputKey("LControl+N,RControl+N")),
                new Button(new Vector2(39, 52), "BACK", 4, ColorScheme.black, ColorScheme.red, ColorScheme.lightDarkRed, new InputKey("Escape"), center),
            };
            _lastStatsButtons = new List<Button> {
                new Button(new Vector2(2, 53), "CONTINUE", 8, ColorScheme.black, ColorScheme.cyan, ColorScheme.lightDarkCyan),
                new Button(new Vector2(2, 55), "RESTART", 7, ColorScheme.black, ColorScheme.yellow, ColorScheme.lightDarkYellow,
                    new InputKey("LControl+R,RControl+R")),
                new Button(new Vector2(10, 55), "AUTO", 4, ColorScheme.black, ColorScheme.blue, ColorScheme.lightDarkBlue, new InputKey("Tab")),
                new Button(new Vector2(2, 55), "SAVE", 4, ColorScheme.black, ColorScheme.blue, ColorScheme.lightDarkBlue,
                    new InputKey("LControl+S,RControl+S")),
                new Button(new Vector2(2, 57), "EXIT", 4, ColorScheme.black, ColorScheme.red, ColorScheme.lightDarkRed, new InputKey("Backspace")),
            };
            _levelEditorButtons = new List<Button> {
                new Button(new Vector2(78, 58), "►", 1, ColorScheme.black, ColorScheme.green, ColorScheme.lightDarkGreen, new InputKey("Enter")),
            };
            _musicSpeedSlider = new Slider(new Vector2(78, 58), 25, 100, 16, 100, "", ColorScheme.black, ColorScheme.blue, ColorScheme.lightDarkBlue, true,
                Renderer.Alignment.Right, Slider.TextAlignment.Right);
            _skipButton = new Button(new Vector2(78, 58), "SKIP", 4, ColorScheme.black, ColorScheme.orange, ColorScheme.lightDarkOrange,
                new InputKey("Space"), right);
            
            

            musicVolumeSlider = new Slider(new Vector2(), 0, 100, 21, 15, "MUSIC VOLUME", ColorScheme.black, ColorScheme.blue, ColorScheme.lightDarkBlue);
            soundsVolumeSlider = new Slider(new Vector2(), 0, 100, 21, 10, "SOUNDS VOLUME", ColorScheme.black, ColorScheme.blue, ColorScheme.lightDarkBlue);

            bloomSwitch = new Button(new Vector2(4, 24), "BLOOM", 5, ColorScheme.black, ColorScheme.blue, ColorScheme.lightDarkBlue);
            fullscreenSwitch = new Button(new Vector2(4, 26), "FULLSCREEN", 10, ColorScheme.black, ColorScheme.blue, ColorScheme.lightDarkBlue);
            uppercaseSwitch = new Button(new Vector2(4, 28), "UPPERCASE NOTES", 15, ColorScheme.black, ColorScheme.blue, ColorScheme.lightDarkBlue);

            showFpsSwitch = new Button(new Vector2(4, 37), "SHOW FPS", 8, ColorScheme.black, ColorScheme.blue, ColorScheme.lightDarkBlue);

            _keybindsButton = new Button(new Vector2(2, 57), "KEYBINDS", 8, ColorScheme.black, ColorScheme.blue, ColorScheme.lightDarkBlue);

            UpdateAllFolderSwitchButtons();
        }
        static Button _pauseMusicButton;
        static Button _switchMusicButton;
        static readonly Vector2 nowPlayingCtrlPos = new Vector2(5, 58);
        static readonly Vector2 nowPlayingPos = new Vector2(1, 58);
        static void DrawNowPlaying(bool controls = false) {
            string text = $"NOW PLAYING : {Game.currentMusicName}";
            Core.renderer.DrawText(controls ? nowPlayingCtrlPos : nowPlayingPos, text, ColorScheme.white, Color.Transparent);
            if(!controls) return;
            _pauseMusicButton.text = Game.music.Status switch
            {
                SoundStatus.Playing => "║",
                _ => "►"
            };
            if(_pauseMusicButton.Draw())
                switch(_pauseMusicButton.text) {
                    case "►": Game.music.Play();
                        break;
                    case "║": Game.music.Pause();
                        break;
                }

            if(_switchMusicButton.Draw()) Game.SwitchMusic();
        }
        static void DrawMainMenu() {
            Renderer.instance.DrawText(zero, mainMenuText, ColorScheme.white, ColorScheme.black);
            // ReSharper disable once HeapView.ObjectAllocation
            // ReSharper disable once HeapView.ObjectAllocation.Possible
            foreach(Button button in _mainMenuButtons.Where(button => button.Draw()))
                switch(button.text) {
                    case "PLAY":
                    case "EDIT":
                        Game.editing = button.text == "EDIT";
                        Renderer.instance.window.SetKeyRepeatEnabled(Game.editing);
                        Game.auto = false;
                        Game.currentMenu = Menu.LevelSelect;
                        break;
                    case "SETTINGS": Game.currentMenu = Menu.Settings;
                        break;
                    case "EXIT": Game.End();
                        break;
                }

            DrawNowPlaying(true);
        }
        public static readonly Vector2 scoresPos = new Vector2(1, 12);
        static void DrawLevelSelect() {
            Renderer.instance.DrawText(zero, levelSelectText, ColorScheme.white, ColorScheme.black);
            for(int i = 0; i < levelSelectLevels.Count; i++) {
                Button button = levelSelectLevels[i];
                if(button.position.y < 12 || button.position.y > 49) continue;
                if(button.Draw()) {
                    _lastLevel = button.text;
                    string path = Path.Combine("levels", _lastLevel);
                    Map.LoadLevelFromLines(File.ReadAllLines(Path.Combine(path, "level.txt")), _lastLevel, Game.GetSoundFilePath(Path.Combine(path, "music")));
                    Game.currentMenu = Menu.Game;
                    Game.RecalculatePosition();
                }
                if((button.currentState == Button.State.Hovered || button.currentState == Button.State.Clicked) &&
                    button.prevFrameState != Button.State.Hovered && button.prevFrameState != Button.State.Clicked) {
                    string levelPath = Path.Combine("levels", button.text);
                    string musicPath = Game.GetSoundFilePath(Path.Combine(levelPath, "music"));
                    if(File.Exists(musicPath)) {
                        Game.currentMusicPath = musicPath;
                        Game.music.Stop();
                        Game.music = new Music(musicPath) {
                            Volume = Settings.Default.musicVolume
                        };
                        Game.music.Play();
                    }

                    currentLevelSelectIndex = i;
                }
                button.selected = i == currentLevelSelectIndex;
            }
            foreach(Button button in _levelSelectButtons)
                switch(button.text) {
                    case "NEW" when Game.editing && button.Draw():
                        _lastLevel = "unnamed";
                        Map.LoadLevelFromLines(File.ReadAllLines(Path.Combine("levels", "_template", "level.txt")), _lastLevel, "");
                        Game.currentMenu = Menu.Game;
                        Game.RecalculatePosition();
                        break;
                    case "AUTO" when !Game.editing: {
                        if(button.Draw()) Game.auto = !Game.auto;
                        button.selected = Game.auto;
                        break;
                    }
                    case "BACK" when button.Draw(): Game.currentMenu = Menu.Main;
                        break;
                }

            if(_levelSelectButtons.Count > 0 && levelSelectMetadatas.Count > 0 && levelSelectScores.Count > 0) {
                DrawMetadata(levelSelectMetadatas[currentLevelSelectIndex]);
                DrawScores(levelSelectScores[currentLevelSelectIndex]);
            }
            DrawNowPlaying();
        }
        static readonly Vector2 metaLengthPos = new Vector2(56, 12);
        static readonly Vector2 metaDiffPos = new Vector2(56, 13);
        static readonly Vector2 metaBPMPos = new Vector2(56, 14);
        static readonly Vector2 metaAuthorPos = new Vector2(56, 15);

        static readonly Vector2 metaObjCountPos = new Vector2(56, 48);
        static readonly Vector2 metaSpdCountPos = new Vector2(56, 49);
        static void DrawMetadata(LevelMetadata? metadata) {
            if(metadata == null) return;
            Renderer.instance.DrawText(metaLengthPos, "LENGTH:" + metadata.Value.length, ColorScheme.white, Color.Transparent);
            Renderer.instance.DrawText(metaDiffPos, "DIFFICULTY:" + metadata.Value.difficulty, ColorScheme.white, Color.Transparent);
            Renderer.instance.DrawText(metaBPMPos, "BPM:" + metadata.Value.bpm, ColorScheme.white, Color.Transparent);
            Renderer.instance.DrawText(metaAuthorPos, "AUTHOR:" + metadata.Value.author, ColorScheme.white, Color.Transparent);

            Renderer.instance.DrawText(metaObjCountPos, "objects:" + metadata.Value.objectCount, ColorScheme.white, Color.Transparent);
            Renderer.instance.DrawText(metaSpdCountPos, "speeds:" + metadata.Value.speedsCount, ColorScheme.white, Color.Transparent);
        }
        static void DrawScores(IReadOnlyCollection<LevelScore> scores) {
            if(scores == null) return;
            // ReSharper disable once HeapView.ObjectAllocation.Possible
            foreach(LevelScore score in scores) {
                if(score.scorePosition.y >= 12 && score.scorePosition.y <= 49)
                    Renderer.instance.DrawText(score.scorePosition, score.scoreStr, ColorScheme.blue, Color.Transparent);
                if(score.accComboPosition.y >= 12 && score.accComboPosition.y <= 49) {
                    Renderer.instance.DrawText(score.accComboPosition, score.accuracyStr, score.accuracyColor, Color.Transparent);
                    Renderer.instance.DrawText(score.accComboDividerPosition, "│", ColorScheme.blue, Color.Transparent);
                    Renderer.instance.DrawText(score.maxComboPosition, score.maxComboStr, score.maxComboColor, Color.Transparent);
                }
                if(score.scoresPosition.y >= 12 && score.scoresPosition.y <= 49)
                    DrawMiniScores(score.scoresPosition, score.scores);
                if(score.linePosition.y >= 12 && score.linePosition.y <= 49)
                    Renderer.instance.DrawText(score.linePosition, "├───────────────────────┤", ColorScheme.white, Color.Transparent);
            }
        }

        static readonly Vector2 levelNamePos = new Vector2(0, 0);
        static readonly Vector2 scorePos = new Vector2(0, 57);
        static readonly Vector2 accPos = new Vector2(0, 58);
        static readonly Vector2 comboPos = new Vector2(0, 59);
        static readonly Vector2 miniScoresPos = new Vector2(25, 59);
        static readonly Vector2 bpmPos = new Vector2(0, 57);
        static readonly Vector2 timePos = new Vector2(0, 58);
        static readonly Vector2 offsetPos = new Vector2(0, 59);
        static readonly Vector2 hpDrainPos = new Vector2(20, 57);
        static readonly Vector2 hpRestoragePos = new Vector2(20, 58);
        static readonly Vector2 musicOffsetPos = new Vector2(20, 59);
        static Slider _musicSpeedSlider;
        static void DrawGame() {
            if(Game.editing) {
                foreach(Button button in _levelEditorButtons) {
                    button.text = Game.music.Status switch
                    {
                        SoundStatus.Playing => "║",
                        _ => "►"
                    };
                    if(button.Draw())
                        switch(button.text) {
                            case "►": Game.music.Play();
                                break;
                            case "║":
                                Game.music.Pause();
                                Game.steps = Game.roundedSteps;
                                Game.UpdateTime();
                                break;
                        }
                }
                Renderer.instance.DrawText(bpmPos, $"BPM: {Game.currentBPM.ToString()}", ColorScheme.blue,
                    Color.Transparent);
                TimeSpan curTime = TimeSpan.FromMilliseconds(Game.timeFromStart.AsMilliseconds());
                Renderer.instance.DrawText(timePos, "TIME: " + (curTime < TimeSpan.Zero ? "'-'" : "") + curTime.ToString((curTime.Hours != 0 ? "h':'mm" : "m") + "':'ss"),
                                            ColorScheme.blue, Color.Transparent);
                Renderer.instance.DrawText(offsetPos, "OFFSET: " + Game.roundedOffset + " (" + Game.roundedSteps + ")", ColorScheme.blue,
                    Color.Transparent);

                Renderer.instance.DrawText(hpDrainPos, "HP DRAIN: " + Map.currentLevel.metadata.hpDrain, ColorScheme.red, Color.Transparent);
                Renderer.instance.DrawText(hpRestoragePos, "HP RESTORAGE: " + Map.currentLevel.metadata.hpRestorage, ColorScheme.red,
                    Color.Transparent);

                Renderer.instance.DrawText(musicOffsetPos, "MUSIC OFFSET: " + Map.currentLevel.metadata.initialOffsetMs + " MS", ColorScheme.gray,
                    Color.Transparent);

                Game.music.Pitch = _musicSpeedSlider.Draw() / 100f;

                DrawProgress();
                DrawLevelName(levelNamePos, ColorScheme.black);
            }
            else {
                DrawHealth();
                DrawProgress();
                DrawScore(scorePos, ColorScheme.blue);
                DrawAccuracy(accPos);
                DrawCombo(comboPos);
                DrawMiniScores(miniScoresPos, Game.scores);
                DrawLevelName(levelNamePos, ColorScheme.black);
                LevelMetadata metadata = Map.currentLevel.metadata;

                if(metadata.skippable &&
                   Game.music.PlayingOffset.AsMilliseconds() < Map.currentLevel.metadata.skipTime && _skipButton.Draw())
                    Game.music.PlayingOffset = Time.FromMilliseconds(Map.currentLevel.metadata.skipTime);
            }
        }
        static Vector2 _healthTempVector = new Vector2(0, 1);
        static void DrawHealth() {
            for(int x = 0; x < 80; x++) {
                _healthTempVector.x = x;
                float rate = 3.5f + healthAnimRateOffsets[x];
                Renderer.instance.SetCellColor(_healthTempVector, Color.Transparent,
                    Renderer.AnimateColor(healthAnimTimes[x], prevHealthColors[x], healthColors[x], rate));
                healthAnimTimes[x] += Core.deltaTime;
            }
        }
        static Vector2 _progressTempVector;
        static void DrawProgress() {
            for(int x = 0; x < 80; x++) {
                _progressTempVector.x = x;
                float rate = 3.5f + progressAnimRateOffsets[x];
                Renderer.instance.SetCellColor(_progressTempVector, Color.Transparent,
                    Renderer.AnimateColor(progressAnimTimes[x], prevProgressColors[x], progressColors[x], rate));
                progressAnimTimes[x] += Core.deltaTime;
            }
        }
        static int _scoreChange;
        public static int prevScore;
        static float _newScoreAnimationTime;
        static void DrawScore(Vector2 position, Color color) {
            string scoreStr = "SCORE: " + Game.score;
            Renderer.instance.DrawText(position, scoreStr, color, Color.Transparent);
            if(prevScore != Game.score) {
                if(_newScoreAnimationTime >= 1f) _scoreChange = 0;
                _newScoreAnimationTime = 0f;
                _scoreChange += Game.score - prevScore;
            }
            Renderer.instance.DrawText(new Vector2(position.x + scoreStr.Length + 2, position.y), "+" + _scoreChange,
                                                                                    Renderer.AnimateColor(_newScoreAnimationTime, color, Color.Transparent, 2f), Color.Transparent);
            _newScoreAnimationTime += Core.deltaTime;

            prevScore = Game.score;
        }
        static void DrawAccuracy(Vector2 position) {
            Renderer.instance.DrawText(position, "ACCURACY: " + Game.accuracy + "%", Game.GetAccuracyColor(Game.accuracy), Color.Transparent);
        }
        static void DrawCombo(Vector2 position, bool maxCombo = false) {
            string prefix = Game.accuracy >= 100 ? "PERFECT " : Game.scores[0] <= 0 ? "FULL " : maxCombo ? "MAX " : "";
            Color color = Game.GetComboColor(Game.accuracy, Game.scores[0]);
            Renderer.instance.DrawText(position, prefix + "COMBO: " + (maxCombo ? Game.maxCombo : Game.combo), color, Color.Transparent);
        }
        static void DrawMiniScores(Vector2 position, int[] scores) {
            string scores0Str = scores[0].ToString();
            Renderer.instance.DrawText(position, scores0Str, ColorScheme.black, ColorScheme.red);

            string scores1Str = scores[1].ToString();
            int x1 = position.x + scores0Str.Length + 1;
            Renderer.instance.DrawText(new Vector2(x1, position.y), scores1Str, ColorScheme.black, ColorScheme.yellow);

            Renderer.instance.DrawText(new Vector2(x1 + scores1Str.Length + 1, position.y), scores[2].ToString(), ColorScheme.black, ColorScheme.green);
        }
        static void DrawScores(Vector2 position) {
            int posXOffseted = position.x + 15;
            Renderer.instance.DrawText(position, "MISSES:", ColorScheme.red, ColorScheme.black);
            Renderer.instance.DrawText(new Vector2(posXOffseted, position.y), Game.scores[0].ToString(), ColorScheme.black, ColorScheme.red);

            int posYHits = position.y + 2;
            Renderer.instance.DrawText(new Vector2(position.x, posYHits), "HITS:", ColorScheme.yellow, ColorScheme.black);
            Renderer.instance.DrawText(new Vector2(posXOffseted, posYHits), Game.scores[1].ToString(), ColorScheme.black, ColorScheme.yellow);

            int posYPerfectHits = position.y + 4;
            Renderer.instance.DrawText(new Vector2(position.x, posYPerfectHits), "PERFECT HITS:", ColorScheme.green, ColorScheme.black);
            Renderer.instance.DrawText(new Vector2(posXOffseted, posYPerfectHits), Game.scores[2].ToString(), ColorScheme.black, ColorScheme.green);
        }
        static void DrawLevelName(Vector2 position, Color color) {
            Renderer.instance.DrawText(position, Map.currentLevel.metadata.name + " : " + Map.currentLevel.metadata.author, color, Color.Transparent);
        }
        static readonly Vector2 passFailText = new Vector2(40, 5);
        static readonly Vector2 lastLevelPos = new Vector2(2, 13);
        static readonly Vector2 lastScorePos = new Vector2(2, 16);
        static readonly Vector2 lastAccPos = new Vector2(2, 18);
        static readonly Vector2 lastScoresPos = new Vector2(25, 16);
        static readonly Vector2 lastMaxComboPos = new Vector2(2, 20);
        static void DrawLastStats() {
            Renderer.instance.DrawText(zero, lastStatsText, ColorScheme.white, ColorScheme.black);
            string text = "PAUSE";
            Color color = ColorScheme.cyan;
            if(!Game.editing && Game.statsState != StatsState.Pause) {
                if(Game.statsState == StatsState.Pass) {
                    text = "PASS";
                    color = ColorScheme.green;
                }
                else {
                    text = "FAIL";
                    color = ColorScheme.red;
                }
            }
            Renderer.instance.DrawText(passFailText, text, color, Color.Transparent, Renderer.Alignment.Center);
            DrawLevelName(lastLevelPos, ColorScheme.white);
            if(!Game.editing) {
                DrawScore(lastScorePos, ColorScheme.blue);
                DrawAccuracy(lastAccPos);
                DrawScores(lastScoresPos);
                DrawCombo(lastMaxComboPos, true);
            }
            DrawSettingsList(true);
            foreach(Button button in _lastStatsButtons)
                switch(button.text) {
                    case "CONTINUE": {
                        if(Map.currentLevel.objects.Count > 0 && Game.health > 0 && button.Draw()) Game.currentMenu = Menu.Game;
                        break;
                    }
                    case "RESTART": {
                        if(!Game.editing && button.Draw()) {
                            Game.currentMenu = Menu.Game;
                            string path = Path.Combine("levels", _lastLevel);
                            Map.LoadLevelFromLines(File.ReadAllLines(Path.Combine(path, "level.txt")), _lastLevel, Game.GetSoundFilePath(Path.Combine(path, "music")));
                        }
                        break;
                    }
                    case "AUTO": {
                        if(!Game.editing && button.Draw()) Game.auto = !Game.auto;
                        button.selected = Game.auto;
                        break;
                    }
                    case "SAVE": {
                        if(Game.editing && button.Draw()) {
                            string path = Path.Combine("levels", _lastLevel);
                            _ = Directory.CreateDirectory(path);
                            File.WriteAllText(Path.Combine(path, "level.txt"), Map.TextFromLevel(Map.currentLevel));
                        }
                        break;
                    }
                    default: {
                        if(button.Draw())
                            if(button.text == "EXIT") {
                                Game.currentMenu = Menu.LevelSelect;
                                Game.music.Pitch = 1f;
                                _musicSpeedSlider.value = 100;
                            }
                        break;
                    }
                }
        }

        static readonly Vector2 audioGroupTextPos = new Vector2(2, 13);
        public static Slider musicVolumeSlider;
        public static Slider soundsVolumeSlider;
        static readonly Vector2 audioSwitchPos = new Vector2(4, 19);
        static readonly List<Button> audioSwitchButtonsList = new List<Button>();

        static readonly Vector2 graphicsGroupTextPos = new Vector2(2, 22);
        public static Button bloomSwitch;
        public static Button fullscreenSwitch;
        public static Button uppercaseSwitch;
        static readonly Vector2 fontSwitchPos = new Vector2(4, 30);
        static readonly List<Button> fontSwitchButtonsList = new List<Button>();
        static readonly Vector2 colorSchemeSwitchPos = new Vector2(4, 32);
        static readonly List<Button> colorSchemeSwitchButtonsList = new List<Button>();

        static readonly Vector2 advancedGroupTextPos = new Vector2(2, 35);
        public static Button showFpsSwitch;

        static string IncreaseFolderSwitchDirectory(string currentPath, string basePath, int at) {
            // Disassemble the path
            List<string> fullDirNames = currentPath.Split(Path.DirectorySeparatorChar).ToList();
            while(fullDirNames.Count > at + 1) fullDirNames.RemoveAt(fullDirNames.Count - 1);
            string fullDir = Path.Combine(fullDirNames.ToArray());
            string inDir = Path.GetDirectoryName(fullDir);
            string[] inDirNames = Directory.GetDirectories(Path.Combine(basePath, inDir ?? ""))
                // ReSharper disable once HeapView.ObjectAllocation
                .Select(Path.GetFileName).ToArray();

            // Move to the next folder
            int curPathIndex = Array.IndexOf(inDirNames, fullDirNames.Last());
            int nextIndex = curPathIndex + 1;
            fullDirNames.RemoveAt(at);
            fullDirNames.Add(inDirNames[nextIndex >= inDirNames.Length ? 0 : nextIndex]);

            // Assemble the path back
            string newPath = Path.Combine(fullDirNames.ToArray());
            string[] newPathDirs = Directory.GetDirectories(Path.Combine(basePath, newPath));
            while(newPathDirs.Length > 0) {
                newPath = Path.Combine(newPath, Path.GetFileName(newPathDirs[0]) ?? string.Empty);
                newPathDirs = Directory.GetDirectories(Path.Combine(basePath, newPath));
            }
            return newPath;
        }

        static void UpdateAllFolderSwitchButtons() {
            UpdateFolderSwitchButtons(audioSwitchButtonsList, Settings.Default.audio, audioSwitchPos.x, audioSwitchPos.y, 7);
            UpdateFolderSwitchButtons(fontSwitchButtonsList, Settings.Default.font, fontSwitchPos.x, fontSwitchPos.y, 5);
            UpdateFolderSwitchButtons(colorSchemeSwitchButtonsList, Settings.Default.colorScheme, colorSchemeSwitchPos.x,
                colorSchemeSwitchPos.y, 13);
        }

        static void UpdateFolderSwitchButtons(List<Button> buttonsList, string path, int baseX, int baseY, int xOffset) {
            buttonsList.Clear();
            UpdateFolderSwitchButton(buttonsList, path, baseX, baseY, xOffset);
        }
        static void UpdateFolderSwitchButton(List<Button> buttonsList, string path, int baseX, int baseY, int xOffset) {
            string[] names = path.Split(Path.DirectorySeparatorChar);

            string prevDir = Path.GetDirectoryName(path) ?? string.Empty;
            Vector2 position =
                new Vector2(baseX + xOffset + (names.Length > 1 ? 1 : 0) + prevDir.Length, baseY);
            string text = names[^1];
            buttonsList.Insert(0,
                new Button(position, text, text.Length, ColorScheme.black, ColorScheme.blue,
                    ColorScheme.lightDarkBlue));

            string nextPath = Path.GetDirectoryName(path);
            if(nextPath != "") UpdateFolderSwitchButton(buttonsList, nextPath, baseX, baseY, xOffset);
        }
        static void DrawSettingsList(bool pauseMenu = false) {
            if(pauseMenu) {
                musicVolumeSlider.position.x = 78;
                musicVolumeSlider.position.y = 55;
                musicVolumeSlider.align = Renderer.Alignment.Right;
                musicVolumeSlider.alignText = Slider.TextAlignment.Right;

                soundsVolumeSlider.position.x = 78;
                soundsVolumeSlider.position.y = 57;
                soundsVolumeSlider.align = Renderer.Alignment.Right;
                soundsVolumeSlider.alignText = Slider.TextAlignment.Right;
            }
            else {
                Renderer.instance.DrawText(audioGroupTextPos, "[ AUDIO ]", ColorScheme.white, Color.Transparent);
                musicVolumeSlider.position.x = 4;
                musicVolumeSlider.position.y = 15;
                musicVolumeSlider.align = Renderer.Alignment.Left;
                musicVolumeSlider.alignText = Slider.TextAlignment.Left;

                soundsVolumeSlider.position.x = 4;
                soundsVolumeSlider.position.y = 17;
                soundsVolumeSlider.align = Renderer.Alignment.Left;
                soundsVolumeSlider.alignText = Slider.TextAlignment.Left;

                Renderer.instance.DrawText(audioSwitchPos, "SOUNDS", ColorScheme.blue, Color.Transparent);
                for(int i = audioSwitchButtonsList.Count - 1; i >= 0; i--)
                    if(audioSwitchButtonsList[i].Draw()) {
                        Settings.Default.audio = IncreaseFolderSwitchDirectory(Settings.Default.audio, Path.Combine("resources", "audio"), i);
                        UpdateFolderSwitchButtons(audioSwitchButtonsList, Settings.Default.audio, audioSwitchPos.x, audioSwitchPos.y, 7);
                    }

                Renderer.instance.DrawText(graphicsGroupTextPos, "[ GRAPHICS ]", ColorScheme.white, Color.Transparent);
                if(bloomSwitch.Draw()) Settings.Default.bloom = bloomSwitch.selected = !bloomSwitch.selected;
                if(fullscreenSwitch.Draw()) Settings.Default.fullscreen = fullscreenSwitch.selected = !fullscreenSwitch.selected;
                if(uppercaseSwitch.Draw()) Settings.Default.uppercaseNotes = uppercaseSwitch.selected = !uppercaseSwitch.selected;
                Renderer.instance.DrawText(fontSwitchPos, "FONT", ColorScheme.blue, Color.Transparent);
                for(int i = fontSwitchButtonsList.Count - 1; i >= 0; i--)
                    if(fontSwitchButtonsList[i].Draw()) {
                        Settings.Default.font = IncreaseFolderSwitchDirectory(Settings.Default.font, Path.Combine("resources", "fonts"), i);
                        UpdateFolderSwitchButtons(fontSwitchButtonsList, Settings.Default.font, fontSwitchPos.x, fontSwitchPos.y, 5);
                    }

                Renderer.instance.DrawText(colorSchemeSwitchPos, "COLOR SCHEME", ColorScheme.blue, Color.Transparent);
                for(int i = colorSchemeSwitchButtonsList.Count - 1; i >= 0; i--)
                    if(colorSchemeSwitchButtonsList[i].Draw()) {
                        Settings.Default.colorScheme = IncreaseFolderSwitchDirectory(Settings.Default.colorScheme, Path.Combine("resources", "colors"), i);
                        UpdateFolderSwitchButtons(colorSchemeSwitchButtonsList, Settings.Default.colorScheme, colorSchemeSwitchPos.x,
                            colorSchemeSwitchPos.y, 13);
                    }

                Renderer.instance.DrawText(advancedGroupTextPos, "[ ADVANCED ]", ColorScheme.white, Color.Transparent);
                if(showFpsSwitch.Draw()) Settings.Default.showFps = showFpsSwitch.selected = !showFpsSwitch.selected;
            }

            Settings.Default.musicVolume = musicVolumeSlider.Draw();
            Settings.Default.soundsVolume = soundsVolumeSlider.Draw();
        }
        static Button _keybindsButton;
        static void DrawSettings() {
            Renderer.instance.DrawText(zero, settingsText, ColorScheme.white, ColorScheme.black);
            DrawSettingsList();
            if(_keybindsButton.Draw()) Game.currentMenu = Menu.KeybindsEditor;
        }
        static void DrawKeybindsEditor() {
            Renderer.instance.DrawText(zero, keybindsEditorText, ColorScheme.white, ColorScheme.black);

            IEnumerator enumerator = Bindings.Default.PropertyValues.GetEnumerator();
            for(int y = 17; enumerator.MoveNext(); y += 2) {
                SettingsPropertyValue value = (SettingsPropertyValue)enumerator.Current;
                if(value == null) continue;
                InputKey key = (InputKey)value.PropertyValue;
                string name = value.Name.AddSpaces().ToUpper();
                string[] primAndSec = key.asString.Split(',');
                string primary = primAndSec[0];
                string secondary = primAndSec.Length > 1 ? primAndSec[1] : "<NONE>";
                Renderer.instance.DrawText(new Vector2(2, y), name, ColorScheme.blue, Color.Transparent);
                Renderer.instance.DrawText(new Vector2(37, y), primary, ColorScheme.blue, Color.Transparent);
                Renderer.instance.DrawText(new Vector2(59, y), secondary, ColorScheme.blue, Color.Transparent);
            }
        }
        public static void Draw() {
            switch(Game.currentMenu) {
                case Menu.Main:
                    DrawMainMenu();
                    break;
                case Menu.LevelSelect:
                    DrawLevelSelect();
                    break;
                case Menu.Settings:
                    DrawSettings();
                    break;
                case Menu.KeybindsEditor:
                    DrawKeybindsEditor();
                    break;
                case Menu.Game:
                    DrawGame();
                    break;
                case Menu.LastStats:
                    DrawLastStats();
                    break;
            }
            if(Settings.Default.showFps)
                Renderer.instance.DrawText(fpsPos, fps + " FPS", fps >= 60 ? ColorScheme.green : fps > 20 ? ColorScheme.yellow : ColorScheme.red,
                    ColorScheme.black, Renderer.Alignment.Right);
        }
        static readonly Vector2 fpsPos = new Vector2(79, 59);
    }
}
