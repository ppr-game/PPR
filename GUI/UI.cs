using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using PPR.GUI.Elements;
using PPR.Main;
using PPR.Main.Levels;
using PPR.Properties;

using PRR;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;

namespace PPR.GUI {
    public static class UI {
        public static int fps = 0;
        public static int avgFPS = 0;
        public static int tempAvgFPS = 0;
        public static int tempAvgFPSCounter = 0;
        
        static readonly Random random = new Random();
        static readonly Perlin perlin = new Perlin();

        static readonly Color[] prevHealthColors = new Color[80];
        static readonly Color[] healthColors = new Color[80];
        static readonly float[] healthAnimTimes = new float[80];
        static readonly float[] healthAnimRateOffsets = new float[80];
        public static int health {
            set {
                for(int x = 0; x < 80; x++) {
                    Color color = value > x ? ColorScheme.GetColor("health_bar") : ColorScheme.GetColor("dark_health_bar");
                    if(healthColors[x] != color) {
                        prevHealthColors[x] = healthColors[x];
                        healthAnimTimes[x] = 0f;
                        healthAnimRateOffsets[x] = random.NextFloat(-3f, 3f);
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
                    Color color = value > x ? ColorScheme.GetColor("progress_bar") : ColorScheme.GetColor("dark_progress_bar");
                    if(progressColors[x] != color) {
                        prevProgressColors[x] = progressColors[x];
                        progressAnimTimes[x] = 0f;
                        progressAnimRateOffsets[x] = random.NextFloat(-3f, 3f);
                    }
                    progressColors[x] = color;
                }
            }
        }

        static readonly string[] mainMenuText = File.ReadAllLines(Path.Join("resources", "ui", "mainMenu.txt"));
        static readonly string[] settingsText = File.ReadAllLines(Path.Join("resources", "ui", "settings.txt"));
        static readonly string[] keybindsEditorText = File.ReadAllLines(Path.Join("resources", "ui", "keybinds.txt"));
        static readonly string[] levelSelectText = File.ReadAllLines(Path.Join("resources", "ui", "levelSelect.txt"));
        static readonly string[] lastStatsText = File.ReadAllLines(Path.Join("resources", "ui", "lastStats.txt"));
        //static readonly string[] notificationsText = File.ReadAllLines(Path.Join("resources", "ui", "notifications.txt"));
        static List<Button> _mainMenuButtons;

        public static int currentLevelSelectIndex;
        public static List<Button> levelSelectLevels;
        public static List<List<LevelScore>> levelSelectScores;
        public static List<LevelMetadata?> levelSelectMetadatas;
        static List<Button> _levelSelectButtons;

        static string _lastLevel = "";
        static List<Button> _gameLastStatsButtons;
        static List<Button> _editorLastStatsButtons;

        static List<Button> _levelEditorButtons;

        static Button _skipButton;

        //static bool _showNotificationsMenu;

        static readonly Vector2i zero = new Vector2i();
        public static void RecreateButtons() {
            const Renderer.Alignment center = Renderer.Alignment.Center;
            const Renderer.Alignment right = Renderer.Alignment.Right;
            _mainMenuButtons = new List<Button> {
                new Button(new Vector2i(40, 25), "PLAY", "mainMenu.play", 4, new InputKey("Enter"), center),
                new Button(new Vector2i(40, 27), "EDIT", "mainMenu.edit", 4, new InputKey("LShift,RShift"), center),
                new Button(new Vector2i(40, 29), "SETTINGS", "mainMenu.settings", 8, new InputKey("Tab"), center),
                new Button(new Vector2i(40, 31), "EXIT", "mainMenu.exit", 4, new InputKey("Tilde"), center),
                new Button(new Vector2i(1, 1), "SFML", "mainMenu.sfml", 4),
                new Button(new Vector2i(6, 1), "GITHUB", "mainMenu.github", 6),
                new Button(new Vector2i(13, 1), "DISCORD", "mainMenu.discord", 7)
            };
            _pauseMusicButton = new Button(new Vector2i(1, 58), "►", "mainMenu.music.pause", 1,
                new InputKey("Space"));
            _switchMusicButton = new Button(new Vector2i(3, 58), "»", "mainMenu.music.switch", 1,
                new InputKey("Right"));
            _levelSelectButtons = new List<Button> {
                new Button(new Vector2i(25, 10), "AUTO", "levelSelect.auto", 4, new InputKey("Tab")),
                new Button(new Vector2i(25, 10), "NEW", "levelSelect.new", 3,
                    new InputKey("LControl+N,RControl+N")),
                new Button(new Vector2i(39, 52), "BACK", "levelSelect.back", 4, new InputKey("Escape"), center)
            };
            _gameLastStatsButtons = new List<Button> {
                new Button(new Vector2i(2, 53), "CONTINUE", "lastStats.continue", 8, new InputKey("Enter")),
                new Button(new Vector2i(2, 55), "RESTART", "lastStats.restart", 7, new InputKey("LControl+R,RControl+R")),
                new Button(new Vector2i(10, 55), "AUTO", "lastStats.auto", 4, new InputKey("Tab")),
                new Button(new Vector2i(2, 57), "EXIT", "lastStats.exit", 4, new InputKey("Tilde"))
            };
            _editorLastStatsButtons = new List<Button> {
                new Button(new Vector2i(2, 51), "CONTINUE", "lastStats.continue", 8, new InputKey("Enter")),
                new Button(new Vector2i(2, 53), "SAVE", "lastStats.save", 4, new InputKey("LControl+S,RControl+S")),
                new Button(new Vector2i(2, 55), "SAVE & EXIT", "lastStats.saveAndExit", 11),
                new Button(new Vector2i(2, 57), "EXIT", "lastStats.exit", 4, new InputKey("Tilde"))
            };
            _levelEditorButtons = new List<Button> {
                new Button(new Vector2i(78, 58), "►", "editor.playPause", 1, new InputKey("Space")),
                new Button(hpDrainPos, "<", "editor.hp.drain.down", 1),
                new Button(hpDrainPos + new Vector2i(2, 0), ">", "editor.hp.drain.up", 1),
                new Button(hpRestoragePos, "<", "editor.hp.restorage.down", 1),
                new Button(hpRestoragePos + new Vector2i(2, 0), ">", "editor.hp.restorage.up", 1),
                new Button(musicOffsetPos, "<", "editor.music.offset.down", 1),
                new Button(musicOffsetPos + new Vector2i(2, 0), ">", "editor.music.offset.up", 1)
            };

            //_notificationsMenuButton = new Button(new Vector2i(78, 1), "□", "mainMenu.notifications", 1);
            
            _musicSpeedSlider = new Slider(new Vector2i(78, 58), 25, 100, 16, 100,
                "[value]%", "", "editor.music.speed", Renderer.Alignment.Right);
            _skipButton = new Button(new Vector2i(78, 58), "SKIP", "game.skip", 4,
                new InputKey("Space"), right);

            musicVolumeSlider = new Slider(new Vector2i(), 0, 100, 21, 15, "MUSIC VOLUME",
                "[value]", "settings.volume.music");
            soundsVolumeSlider = new Slider(new Vector2i(), 0, 100, 21, 10, "SOUNDS VOLUME",
                "[value]", "settings.volume.sounds");

            bloomSwitch = new Button(new Vector2i(4, 24), "BLOOM", "settings.bloom", 5);
            fullscreenSwitch = new Button(new Vector2i(4, 26), "FULLSCREEN", "settings.fullscreen", 10);
            fpsLimitSlider = new Slider(new Vector2i(4, 28), 0, 1020, 18, 480, "FPS LIMIT", "[value]",
                "settings.fpsLimit");
            uppercaseSwitch = new Button(new Vector2i(4, 30), "UPPERCASE NOTES", "settings.uppercaseNotes", 15);

            showFpsSwitch = new Button(new Vector2i(4, 39), "SHOW FPS", "settings.showFPS", 8);

            //_keybindsButton = new Button(new Vector2i(2, 57), "KEYBINDS", "settings.keybinds", 8);

            UpdateAllFolderSwitchButtons();
        }
        static Button _pauseMusicButton;
        static Button _switchMusicButton;
        static readonly Vector2i nowPlayingCtrlPos = new Vector2i(5, 58);
        static readonly Vector2i nowPlayingPos = new Vector2i(1, 58);
        static void DrawNowPlaying(bool controls = false) {
            string text = $"NOW PLAYING : {Game.currentMusicName}";
            Core.renderer.DrawText(controls ? nowPlayingCtrlPos : nowPlayingPos, text);
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

        public static Dictionary<Vector2i, float> positionRandoms;
        public static void RegenPositionRandoms() {
            positionRandoms = new Dictionary<Vector2i, float>(Core.renderer.width * Core.renderer.height);
            for(int x = 0; x < Core.renderer.width; x++)
                for(int y = 0; y < Core.renderer.height; y++)
                    positionRandoms[new Vector2i(x, y)] = random.NextFloat(0f, 1f);
        }
        
        static float _fadeInTime = 1f;
        public static bool fadeInFinished { get; private set; }
        public static void FadeIn(float speed = 1f) {
            fadeInFinished = false;
            const float min = 0.5f;
            const float max = 4f;
            Core.renderer.charactersModifier = (position, character) => {
                float posRandom = positionRandoms[position] * (max - min) + min;
                if(_fadeInTime * speed * posRandom < 1f) fadeInFinished = false;
                return ((Vector2f)position, new RenderCharacter(Renderer.AnimateColor(_fadeInTime,
                        ColorScheme.GetColor("background"), character.background, speed * posRandom),
                    Renderer.AnimateColor(_fadeInTime,
                        ColorScheme.GetColor("background"), character.foreground, speed * posRandom),
                    character));
            };
            _fadeInTime = 0f;
        }
        static float _fadeOutTime = 1f;
        public static bool fadeOutFinished { get; private set; }
        public static void FadeOut(float speed = 1f) {
            fadeOutFinished = false;
            const float min = 0.5f;
            const float max = 4f;
            Core.renderer.charactersModifier = (position, character) => {
                float posRandom = positionRandoms[position] * (max - min) + min;
                if(_fadeOutTime * speed * posRandom < 1f) fadeOutFinished = false;
                return ((Vector2f)position, new RenderCharacter(Renderer.AnimateColor(_fadeOutTime,
                        character.background, ColorScheme.GetColor("background"), speed * posRandom),
                    Renderer.AnimateColor(_fadeOutTime,
                        character.foreground, ColorScheme.GetColor("background"), speed * posRandom),
                    character));
            };
            _fadeOutTime = 0f;
        }
        
        static float _menusAnimTime;
        public static int menusAnimBPM = 120;
        static void DrawMenusAnim() {
            Color background = ColorScheme.GetColor("background");
            Color menusAnimMax = ColorScheme.GetColor("menus_anim_max");
            Color transparent = ColorScheme.GetColor("transparent");
            for(int x = -3; x < Core.renderer.width + 3; x++) {
                for(int y = -3; y < Core.renderer.height + 3; y++) {
                    if(x % 3 != 0 || y % 3 != 0) continue;
                    float noiseX = (float)perlin.Get(x / 10f, y / 10f, _menusAnimTime / 2f) - 0.5f;
                    float noiseY = (float)perlin.Get(x / 10f, y / 10f, _menusAnimTime / 2f + 100f) - 0.5f;
                    float noise = MathF.Abs(noiseX * noiseY);
                    float xOffset = (Core.renderer.mousePositionF.X / Core.renderer.width - 0.5f) * noise * -100f;
                    float yOffset = (Core.renderer.mousePositionF.Y / Core.renderer.width - 0.5f) * noise * -100f;
                    Color useColor = Renderer.AnimateColor(noise, background, menusAnimMax, 30f);
                    float xPos = x + noiseX * 10f + xOffset;
                    float yPos = y + noiseY * 10f + yOffset;
                    int flooredX = (int)xPos;
                    int flooredY = (int)yPos;
                    for(int useX = flooredX; useX <= flooredX + 1; useX++) {
                        for(int useY = flooredY; useY <= flooredY + 1; useY++) {
                            float percentX = 1f - MathF.Abs(xPos - useX);
                            float percentY = 1f - MathF.Abs(yPos - useY);
                            float percent = percentX * percentY;
                            Color posColor = Renderer.LerpColors(background, useColor, percent);
                            Core.renderer.SetCellColor(new Vector2i(useX, useY), transparent, posColor);
                        }
                    }
                }
            }
            _menusAnimTime += Core.deltaTime * menusAnimBPM / 120f;
        }
        //static Button _notificationsMenuButton;
        static void DrawMainMenu() {
            DrawMenusAnim();
            Core.renderer.DrawText(zero, mainMenuText);
            Core.renderer.DrawText(new Vector2i(1, 2), $"PPR v{Core.version}");
            Core.renderer.DrawText(new Vector2i(1, 3), $"PRR v{Core.prrVersion}");
            DrawNowPlaying(true);
            // ReSharper disable once HeapView.ObjectAllocation
            // ReSharper disable once HeapView.ObjectAllocation.Possible
            foreach(Button button in _mainMenuButtons.Where(button => button.Draw()))
                switch(button.id) {
                    case "mainMenu.play":
                    case "mainMenu.edit":
                        Game.editing = button.id == "mainMenu.edit";
                        Core.renderer.window.SetKeyRepeatEnabled(Game.editing);
                        Game.auto = false;
                        Game.currentMenu = Menu.LevelSelect;
                        break;
                    case "mainMenu.settings": Game.currentMenu = Menu.Settings;
                        break;
                    case "mainMenu.exit": Game.End();
                        break;
                    case "mainMenu.sfml": Helper.OpenURL("https://sfml-dev.org");
                        break;
                    case "mainMenu.github": Helper.OpenURL("https://github.com/ppr-game/PPR");
                        break;
                    case "mainMenu.discord": Helper.OpenURL("https://discord.gg/AuYUVs5");
                        break;
                }
            /*if(_showNotificationsMenu) DrawNotificationsMenu();
            if(!_notificationsMenuButton.Draw()) return;
            _showNotificationsMenu = !_showNotificationsMenu;
            _notificationsMenuButton.selected = _showNotificationsMenu;
            DrawNotificationsMenu();*/
        }
        //static void DrawNotificationsMenu() => Core.renderer.DrawText(new Vector2i(79, 0), notificationsText, Renderer.Alignment.Right, true);
        public static readonly Vector2i scoresPos = new Vector2i(1, 12);
        static void DrawLevelSelect() {
            DrawMenusAnim();
            Core.renderer.DrawText(zero, levelSelectText);
            for(int i = 0; i < levelSelectLevels.Count; i++) {
                Button button = levelSelectLevels[i];
                if(button.position.Y < 12 || button.position.Y > 49) continue;
                if(button.Draw()) {
                    _lastLevel = button.text;
                    string path = Path.Join("levels", _lastLevel);
                    Map.LoadLevelFromPath(path, _lastLevel);
                    Game.currentMenu = Menu.Game;
                    Game.RecalculatePosition();
                }
                if((button.currentState == Button.State.Hovered || button.currentState == Button.State.Clicked) &&
                    button.prevFrameState != Button.State.Hovered && button.prevFrameState != Button.State.Clicked) {
                    string levelPath = Path.Join("levels", button.text);
                    string musicPath = Game.GetSoundFilePath(Path.Join(levelPath, "music"));
                    if(File.Exists(musicPath)) {
                        Game.currentMusicPath = musicPath;
                        Game.music.Stop();
                        Game.music = new Music(musicPath) {
                            Volume = Settings.GetInt("musicVolume")
                        };
                        Game.music.Play();
                    }
                    string scriptPath = Path.Join(levelPath, "script.lua");
                    _showLuaPrompt = File.Exists(scriptPath);

                    currentLevelSelectIndex = i;
                }
                button.selected = i == currentLevelSelectIndex;
            }
            foreach(Button button in _levelSelectButtons)
                switch(button.text) {
                    case "NEW" when Game.editing && button.Draw():
                        _lastLevel = "unnamed";
                        Map.LoadLevelFromPath(Path.Join("levels", "_template"), _lastLevel, false);
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

            if(_levelSelectButtons.Count > 0) {
                if(levelSelectMetadatas.Count > currentLevelSelectIndex)
                    DrawMetadata(levelSelectMetadatas[currentLevelSelectIndex]);
                if(levelSelectScores.Count > currentLevelSelectIndex)
                    DrawScores(levelSelectScores[currentLevelSelectIndex]);
            }
            DrawNowPlaying();
        }
        static readonly Vector2i metaLengthPos = new Vector2i(56, 12);
        static readonly Vector2i metaDiffPos = new Vector2i(56, 13);
        static readonly Vector2i metaBPMPos = new Vector2i(56, 14);
        static readonly Vector2i metaAuthorPos = new Vector2i(56, 15);
        
        static bool _showLuaPrompt;
        static readonly Vector2i luaPromptPos = new Vector2i(56, 46);

        static readonly Vector2i metaObjCountPos = new Vector2i(56, 48);
        static readonly Vector2i metaSpdCountPos = new Vector2i(56, 49);
        static void DrawMetadata(LevelMetadata? metadata) {
            if(metadata == null) return;
            Core.renderer.DrawText(metaLengthPos, $"LENGTH:{metadata.Value.length}");
            Core.renderer.DrawText(metaDiffPos, $"DIFFICULTY:{metadata.Value.difficulty}");
            Core.renderer.DrawText(metaBPMPos, $"BPM:{metadata.Value.bpm}");
            Core.renderer.DrawText(metaAuthorPos, $"AUTHOR:{metadata.Value.author}");
            
            if(_showLuaPrompt)
                Core.renderer.DrawText(luaPromptPos, "○ Contains Lua", ColorScheme.GetColor("lua_prompt"));

            Core.renderer.DrawText(metaObjCountPos, $"objects:{metadata.Value.objectCount.ToString()}");
            Core.renderer.DrawText(metaSpdCountPos, $"speeds:{metadata.Value.speedsCount.ToString()}");
        }
        static void DrawScores(IReadOnlyCollection<LevelScore> scores) {
            if(scores == null) return;
            // ReSharper disable once HeapView.ObjectAllocation.Possible
            foreach(LevelScore score in scores) {
                if(score.scorePosition.Y >= 12 && score.scorePosition.Y <= 49)
                    Core.renderer.DrawText(score.scorePosition, score.scoreStr, ColorScheme.GetColor("score"));
                if(score.accComboPosition.Y >= 12 && score.accComboPosition.Y <= 49) {
                    Core.renderer.DrawText(score.accComboPosition, score.accuracyStr, score.accuracyColor);
                    Core.renderer.DrawText(score.accComboDividerPosition, "│", ColorScheme.GetColor("combo"));
                    Core.renderer.DrawText(score.maxComboPosition, score.maxComboStr, score.maxComboColor);
                }
                if(score.scoresPosition.Y >= 12 && score.scoresPosition.Y <= 49)
                    DrawMiniScores(score.scoresPosition, score.scores);
                if(score.linePosition.Y >= 12 && score.linePosition.Y <= 49)
                    Core.renderer.DrawText(score.linePosition, "├───────────────────────┤");
            }
        }

        static readonly Vector2i levelNamePos = new Vector2i(0, 0);
        static readonly Vector2i musicTimePos = new Vector2i(79, 0);
        static readonly Vector2i scorePos = new Vector2i(0, 57);
        static readonly Vector2i accPos = new Vector2i(0, 58);
        static readonly Vector2i comboPos = new Vector2i(0, 59);
        static readonly Vector2i miniScoresPos = new Vector2i(25, 59);
        static readonly Vector2i bpmPos = new Vector2i(0, 57);
        static readonly Vector2i timePos = new Vector2i(0, 58);
        static readonly Vector2i offsetPos = new Vector2i(0, 59);
        static readonly Vector2i hpDrainPos = new Vector2i(20, 57);
        static readonly Vector2i hpRestoragePos = new Vector2i(20, 58);
        static readonly Vector2i musicOffsetPos = new Vector2i(20, 59);
        static Slider _musicSpeedSlider;
        static void DrawGame() {
            if(Game.editing) {
                foreach(Button button in _levelEditorButtons) {
                    if(button.id == "editor.playPause")
                        button.text = Game.music.Status switch {
                            SoundStatus.Playing => "║",
                            _ => "►"
                        };
                    if(button.Draw()) {
                        switch(button.text) {
                            case "►":
                                Game.music.Play();
                                break;
                            case "║":
                                Game.music.Pause();
                                Game.steps = Game.roundedSteps;
                                Game.UpdateTime();
                                break;
                        }

                        switch(button.id) {
                            case "editor.hp.drain.up": Map.currentLevel.metadata.hpDrain++;
                                break;
                            case "editor.hp.drain.down": Map.currentLevel.metadata.hpDrain--;
                                break;
                            case "editor.hp.restorage.up": Map.currentLevel.metadata.hpRestorage++;
                                break;
                            case "editor.hp.restorage.down": Map.currentLevel.metadata.hpRestorage--;
                                break;
                            case "editor.music.offset.up": Map.currentLevel.metadata.initialOffsetMs++;
                                break;
                            case "editor.music.offset.down": Map.currentLevel.metadata.initialOffsetMs--;
                                break;
                        }
                    }
                }
                Core.renderer.DrawText(bpmPos, $"BPM: {Game.currentBPM.ToString()}", ColorScheme.GetColor("bpm"));
                TimeSpan curTime = TimeSpan.FromMilliseconds(Game.timeFromStart.AsMilliseconds());
                Core.renderer.DrawText(timePos,
                    $"TIME: {(curTime < TimeSpan.Zero ? "'-'" : "")}{curTime.ToString($"{(curTime.Hours != 0 ? "h':'mm" : "m")}':'ss'.'fff")}",
                    ColorScheme.GetColor("time"));
                Core.renderer.DrawText(offsetPos, $"OFFSET: {Game.roundedOffset.ToString()} ({Game.roundedSteps.ToString()})",
                    ColorScheme.GetColor("offset"));

                Core.renderer.DrawText(hpDrainPos, $"    HP DRAIN: {Map.currentLevel.metadata.hpDrain.ToString()}", ColorScheme.GetColor("hp_drain"));
                Core.renderer.DrawText(hpRestoragePos, $"    HP RESTORAGE: {Map.currentLevel.metadata.hpRestorage.ToString()}", ColorScheme.GetColor("hp_restorage"));

                Core.renderer.DrawText(musicOffsetPos,
                    $"    MUSIC OFFSET: {Map.currentLevel.metadata.initialOffsetMs.ToString()} MS", ColorScheme.GetColor("music_offset"));

                if(_musicSpeedSlider.Draw()) Game.music.Pitch = _musicSpeedSlider.value / 100f;

                DrawProgress();
                DrawLevelName(levelNamePos, ColorScheme.GetColor("game_level_name"));
                DrawEditorDifficulty(musicTimePos, ColorScheme.GetColor("game_music_time"));
            }
            else {
                DrawHealth();
                DrawProgress();
                DrawScore(scorePos, ColorScheme.GetColor("score"));
                DrawAccuracy(accPos);
                DrawCombo(comboPos);
                DrawMiniScores(miniScoresPos, Game.scores);
                DrawLevelName(levelNamePos, ColorScheme.GetColor("game_level_name"));
                DrawMusicTime(musicTimePos, ColorScheme.GetColor("game_music_time"));
                LevelMetadata metadata = Map.currentLevel.metadata;

                if(metadata.skippable &&
                   Game.music.PlayingOffset.AsMilliseconds() < Map.currentLevel.metadata.skipTime && _skipButton.Draw())
                    Game.music.PlayingOffset = Time.FromMilliseconds(Map.currentLevel.metadata.skipTime);
            }
        }
        static void DrawHealth() {
            for(int x = 0; x < 80; x++) {
                float rate = 3.5f + healthAnimRateOffsets[x];
                Core.renderer.SetCellColor(new Vector2i(x, 1), ColorScheme.GetColor("transparent"),
                    Renderer.AnimateColor(healthAnimTimes[x], prevHealthColors[x], healthColors[x], rate));
                healthAnimTimes[x] += Core.deltaTime;
            }
        }
        static void DrawProgress() {
            for(int x = 0; x < 80; x++) {
                float rate = 3.5f + progressAnimRateOffsets[x];
                Core.renderer.SetCellColor(new Vector2i(x, 0), ColorScheme.GetColor("transparent"),
                    Renderer.AnimateColor(progressAnimTimes[x], prevProgressColors[x], progressColors[x], rate));
                progressAnimTimes[x] += Core.deltaTime;
            }
        }
        static int _scoreChange;
        public static int prevScore;
        static float _newScoreAnimationTime;
        static float _scoreAnimationRate = 2f;
        static void DrawScore(Vector2i position, Color color) {
            string scoreStr = $"SCORE: {Game.score.ToString()}";
            Core.renderer.DrawText(position, scoreStr, color);
            if(prevScore != Game.score) {
                if(_newScoreAnimationTime >= 1f / _scoreAnimationRate) _scoreChange = 0;
                _newScoreAnimationTime = 0f;
                _scoreChange += Game.score - prevScore;
            }
            Core.renderer.DrawText(new Vector2i(position.X + scoreStr.Length + 2, position.Y),
                $"+{_scoreChange.ToString()}",
                Renderer.AnimateColor(_newScoreAnimationTime, color, ColorScheme.GetColor("transparent"),
                    _scoreAnimationRate));
            _newScoreAnimationTime += Core.deltaTime;

            prevScore = Game.score;
        }
        static void DrawAccuracy(Vector2i position) => Core.renderer.DrawText(position, $"ACCURACY: {Game.accuracy.ToString()}%",
            Game.GetAccuracyColor(Game.accuracy));
        static void DrawCombo(Vector2i position, bool maxCombo = false) {
            string prefix = Game.accuracy >= 100 ? "PERFECT " : Game.scores[0] <= 0 ? "FULL " : maxCombo ? "MAX " : "";
            Color color = Game.GetComboColor(Game.accuracy, Game.scores[0]);
            Core.renderer.DrawText(position, $"{prefix}COMBO: {(maxCombo ? Game.maxCombo : Game.combo).ToString()}",
                color, ColorScheme.GetColor("transparent"));
        }
        static void DrawMiniScores(Vector2i position, int[] scores) {
            string scores0Str = scores[0].ToString();
            Core.renderer.DrawText(position, scores0Str, ColorScheme.GetColor("background"),
                ColorScheme.GetColor("miss"));

            string scores1Str = scores[1].ToString();
            int x1 = position.X + scores0Str.Length + 1;
            Core.renderer.DrawText(new Vector2i(x1, position.Y), scores1Str, ColorScheme.GetColor("background"),
                ColorScheme.GetColor("hit"));

            Core.renderer.DrawText(new Vector2i(x1 + scores1Str.Length + 1, position.Y), scores[2].ToString(), 
                ColorScheme.GetColor("background"),
                ColorScheme.GetColor("perfect_hit"));
        }
        static void DrawScores(Vector2i position) {
            int posXOffseted = position.X + 15;
            Core.renderer.DrawText(position, "MISSES:", ColorScheme.GetColor("miss"));
            Core.renderer.DrawText(new Vector2i(posXOffseted, position.Y), Game.scores[0].ToString(),
                ColorScheme.GetColor("background"),
                ColorScheme.GetColor("miss"));

            int posYHits = position.Y + 2;
            Core.renderer.DrawText(new Vector2i(position.X, posYHits), "HITS:", ColorScheme.GetColor("hit"));
            Core.renderer.DrawText(new Vector2i(posXOffseted, posYHits), Game.scores[1].ToString(),
                ColorScheme.GetColor("background"),
                ColorScheme.GetColor("hit"));

            int posYPerfectHits = position.Y + 4;
            Core.renderer.DrawText(new Vector2i(position.X, posYPerfectHits), "PERFECT HITS:", ColorScheme.GetColor("perfect_hit"));
            Core.renderer.DrawText(new Vector2i(posXOffseted, posYPerfectHits), Game.scores[2].ToString(),
                ColorScheme.GetColor("background"),
                ColorScheme.GetColor("perfect_hit"));
        }
        static void DrawLevelName(Vector2i position, Color color, bool invertOnDarkBG = true) => Core.renderer.DrawText(position,
            $"{Map.currentLevel.metadata.name} : {Map.currentLevel.metadata.author}", color,
            Renderer.Alignment.Left, false, invertOnDarkBG);
        static void DrawMusicTime(Vector2i position, Color color) {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(Game.timeFromStart.AsMilliseconds());
            string at = $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{timeSpan.ToString($"{(timeSpan.Hours != 0 ? "h':'mm" : "m")}':'ss")}";
            Core.renderer.DrawText(position, $"{at}/{Map.currentLevel.metadata.length}", color,
                Renderer.Alignment.Right, false, true);
        }
        static void DrawEditorDifficulty(Vector2i position, Color color) => Core.renderer.DrawText(position, $"DIFFICULTY: {Map.currentLevel.metadata.difficulty}", color,
            Renderer.Alignment.Right, false, true);
        static readonly Vector2i passFailText = new Vector2i(40, 5);
        static readonly Vector2i lastLevelPos = new Vector2i(2, 13);
        static readonly Vector2i lastScorePos = new Vector2i(2, 16);
        static readonly Vector2i lastAccPos = new Vector2i(2, 18);
        static readonly Vector2i lastScoresPos = new Vector2i(25, 16);
        static readonly Vector2i lastMaxComboPos = new Vector2i(2, 20);
        static void DrawLastStats() {
            DrawMenusAnim();
            Core.renderer.DrawText(zero, lastStatsText);
            string text = "PAUSE";
            Color color = ColorScheme.GetColor("pause");
            if(!Game.editing && Game.statsState != StatsState.Pause) {
                if(Game.statsState == StatsState.Pass) {
                    text = "PASS";
                    color = ColorScheme.GetColor("pass");
                }
                else {
                    text = "FAIL";
                    color = ColorScheme.GetColor("fail");
                }
            }
            Core.renderer.DrawText(passFailText, text, color, Renderer.Alignment.Center);
            DrawLevelName(lastLevelPos, ColorScheme.GetColor("stats_level_name"), false);
            if(!Game.editing) {
                DrawScore(lastScorePos, ColorScheme.GetColor("score"));
                DrawAccuracy(lastAccPos);
                DrawScores(lastScoresPos);
                DrawCombo(lastMaxComboPos, true);
            }
            DrawSettingsList(true);
            if(Game.editing)
                foreach(Button button in _editorLastStatsButtons)
                    switch(button.id) {
                        case "lastStats.continue": LastStatsContinue(button);
                            break;
                        case "lastStats.save":
                        case "lastStats.saveAndExit":
                            if(button.Draw()) {
                                Game.changed = false;
                                string path = Path.Join("levels", _lastLevel);
                                _ = Directory.CreateDirectory(path);
                                File.WriteAllText(Path.Join(path, "level.txt"), Map.TextFromLevel(Map.currentLevel));
                                if(button.id == "lastStats.saveAndExit") LastStatsExit();
                            }
                            if(button.text.EndsWith('*') && !Game.changed) {
                                button.text = button.text.Remove(button.text.Length - 1);
                                button.width--;
                            }
                            else if(!button.text.EndsWith('*') && Game.changed) {
                                button.text = $"{button.text}*";
                                button.width++;
                            }
                            break;
                        case "lastStats.exit":
                            if(button.Draw()) {
                                Game.changed = false;
                                LastStatsExit();
                            }
                            break;
                    }
            else
                foreach(Button button in _gameLastStatsButtons)
                    switch(button.id) {
                        case "lastStats.continue": LastStatsContinue(button);
                            break;
                        case "lastStats.restart":
                            if(!Game.editing && button.Draw()) {
                                Game.currentMenu = Menu.Game;
                                string path = Path.Join("levels", _lastLevel);
                                Map.LoadLevelFromPath(path, _lastLevel);
                            }
                            break;
                        case "lastStats.auto":
                            if(!Game.editing && button.Draw()) Game.auto = !Game.auto;
                            button.selected = Game.auto;
                            break;
                        case "lastStats.exit": if(button.Draw()) LastStatsExit();
                            break;
                    }
        }
        static void LastStatsContinue(Button button) {
            if(Map.currentLevel.objects.Count > 0 && Game.health > 0 && button.Draw()) Game.currentMenu = Menu.Game;
        }
        static void LastStatsExit() {
            Game.currentMenu = Menu.LevelSelect;
            Game.music.Pitch = 1f;
            _musicSpeedSlider.value = 100;
            Lua.ClearScript();
        }

        static readonly Vector2i audioGroupTextPos = new Vector2i(2, 13);
        public static Slider musicVolumeSlider;
        public static Slider soundsVolumeSlider;
        static readonly Vector2i audioSwitchPos = new Vector2i(4, 19);
        static readonly List<Button> audioSwitchButtonsList = new List<Button>();

        static readonly Vector2i graphicsGroupTextPos = new Vector2i(2, 22);
        public static Button bloomSwitch;
        public static Button fullscreenSwitch;
        public static Slider fpsLimitSlider;
        public static Button uppercaseSwitch;
        static readonly Vector2i fontSwitchPos = new Vector2i(4, 32);
        static readonly List<Button> fontSwitchButtonsList = new List<Button>();
        static readonly Vector2i colorSchemeSwitchPos = new Vector2i(4, 34);
        static readonly List<Button> colorSchemeSwitchButtonsList = new List<Button>();

        static readonly Vector2i advancedGroupTextPos = new Vector2i(2, 37);
        public static Button showFpsSwitch;

        static string IncreaseFolderSwitchDirectory(string currentPath, string basePath, int at) {
            // Disassemble the path
            List<string> fullDirNames = currentPath.Split(Path.DirectorySeparatorChar).ToList();
            while(fullDirNames.Count > at + 1) fullDirNames.RemoveAt(fullDirNames.Count - 1);
            string fullDir = Path.Join(fullDirNames.ToArray());
            string inDir = Path.GetDirectoryName(fullDir);
            string[] inDirNames = Directory.GetDirectories(Path.Join(basePath, inDir ?? ""))
                .Select(Path.GetFileName).ToArray();

            // Move to the next folder
            int curPathIndex = Array.IndexOf(inDirNames, fullDirNames.Last());
            int nextIndex = curPathIndex + 1;
            fullDirNames.RemoveAt(at);
            fullDirNames.Add(inDirNames[nextIndex >= inDirNames.Length ? 0 : nextIndex]);

            // Assemble the path back
            string newPath = Path.Join(fullDirNames.ToArray());
            string[] newPathDirs = Directory.GetDirectories(Path.Join(basePath, newPath));
            while(newPathDirs.Length > 0) {
                newPath = Path.Join(newPath, Path.GetFileName(newPathDirs[0]) ?? string.Empty);
                newPathDirs = Directory.GetDirectories(Path.Join(basePath, newPath));
            }
            return newPath;
        }

        static void UpdateAllFolderSwitchButtons() {
            UpdateFolderSwitchButtons(audioSwitchButtonsList, Settings.GetPath("audio"), audioSwitchPos.X,
                audioSwitchPos.Y, 7);
            UpdateFolderSwitchButtons(fontSwitchButtonsList, Settings.GetPath("font"), fontSwitchPos.X,
                fontSwitchPos.Y, 5);
            UpdateFolderSwitchButtons(colorSchemeSwitchButtonsList, Settings.GetPath("colorScheme"),
                colorSchemeSwitchPos.X, colorSchemeSwitchPos.Y, 13);
        }

        static void UpdateFolderSwitchButtons(IList<Button> buttonsList, string path, int baseX, int baseY, int xOffset) {
            buttonsList.Clear();
            UpdateFolderSwitchButton(buttonsList, path, baseX, baseY, xOffset);
        }

        static void UpdateFolderSwitchButton(IList<Button> buttonsList, string path, int baseX, int baseY, int xOffset) {
            while(true) {
                if(path == null) return;
                string[] names = path.Split(Path.DirectorySeparatorChar);

                string prevDir = Path.GetDirectoryName(path) ?? string.Empty;
                Vector2i position = new Vector2i(baseX + xOffset + (names.Length > 1 ? 1 : 0) + prevDir.Length, baseY);
                string text = names[^1];
                buttonsList.Insert(0, new Button(position, text, "settings.folderButton", text.Length));

                string nextPath = Path.GetDirectoryName(path);
                if(nextPath != "") {
                    path = nextPath;
                    continue;
                }

                break;
            }
        }

        static void DrawSettingsList(bool pauseMenu = false) {
            if(pauseMenu) {
                musicVolumeSlider.position = new Vector2i(78, 55);
                musicVolumeSlider.align = Renderer.Alignment.Right;
                musicVolumeSlider.swapTexts = true;

                soundsVolumeSlider.position = new Vector2i(78, 57);
                soundsVolumeSlider.align = Renderer.Alignment.Right;
                soundsVolumeSlider.swapTexts = true;
            }
            else {
                Core.renderer.DrawText(audioGroupTextPos, "[ AUDIO ]", ColorScheme.GetColor("settings_header_audio"));
                musicVolumeSlider.position = new Vector2i(4, 15);
                musicVolumeSlider.align = Renderer.Alignment.Left;
                musicVolumeSlider.swapTexts = false;

                soundsVolumeSlider.position = new Vector2i(4, 17);
                soundsVolumeSlider.align = Renderer.Alignment.Left;
                soundsVolumeSlider.swapTexts = false;

                Core.renderer.DrawText(audioSwitchPos, "SOUNDS", ColorScheme.GetColor("settings"));
                for(int i = audioSwitchButtonsList.Count - 1; i >= 0; i--)
                    if(audioSwitchButtonsList[i].Draw()) {
                        Settings.SetPath("audio",
                            IncreaseFolderSwitchDirectory(Settings.GetPath("audio"),
                                Path.Join("resources", "audio"), i));
                        UpdateFolderSwitchButtons(audioSwitchButtonsList, Settings.GetPath("audio"), audioSwitchPos.X,
                            audioSwitchPos.Y, 7);
                    }

                Core.renderer.DrawText(graphicsGroupTextPos, "[ GRAPHICS ]", ColorScheme.GetColor("settings_header_graphics"));
                if(bloomSwitch.Draw()) Settings.SetBool("bloom", bloomSwitch.selected = !bloomSwitch.selected);
                if(fullscreenSwitch.Draw())
                    Settings.SetBool("fullscreen", fullscreenSwitch.selected = !fullscreenSwitch.selected);
                fpsLimitSlider.rightText = fpsLimitSlider.value < 60 ? "V-Sync" :
                    fpsLimitSlider.value > 960 ? "Unlimited" : "[value]";
                if(fpsLimitSlider.Draw()) Settings.SetInt("fpsLimit", fpsLimitSlider.value);
                if(uppercaseSwitch.Draw())
                    Settings.SetBool("uppercaseNotes", uppercaseSwitch.selected = !uppercaseSwitch.selected);
                Core.renderer.DrawText(fontSwitchPos, "FONT", ColorScheme.GetColor("settings"));
                for(int i = fontSwitchButtonsList.Count - 1; i >= 0; i--)
                    if(fontSwitchButtonsList[i].Draw()) {
                        Settings.SetPath("font",
                            IncreaseFolderSwitchDirectory(Settings.GetPath("font"),
                                Path.Join("resources", "fonts"), i));
                        UpdateFolderSwitchButtons(fontSwitchButtonsList, Settings.GetPath("font"), fontSwitchPos.X,
                            fontSwitchPos.Y, 5);
                    }

                Core.renderer.DrawText(colorSchemeSwitchPos, "COLOR SCHEME", ColorScheme.GetColor("settings"));
                for(int i = colorSchemeSwitchButtonsList.Count - 1; i >= 0; i--)
                    if(colorSchemeSwitchButtonsList[i].Draw()) {
                        Settings.SetPath("colorScheme",
                            IncreaseFolderSwitchDirectory(Settings.GetPath("colorScheme"),
                                Path.Join("resources", "colors"), i));
                        UpdateFolderSwitchButtons(colorSchemeSwitchButtonsList, Settings.GetPath("colorScheme"),
                            colorSchemeSwitchPos.X, colorSchemeSwitchPos.Y, 13);
                    }

                Core.renderer.DrawText(advancedGroupTextPos, "[ ADVANCED ]", ColorScheme.GetColor("settings_header_advanced"));
                if(showFpsSwitch.Draw()) Settings.SetBool("showFps", showFpsSwitch.selected = !showFpsSwitch.selected);
            }

            if(musicVolumeSlider.Draw()) Settings.SetInt("musicVolume", musicVolumeSlider.value);
            if(soundsVolumeSlider.Draw()) Settings.SetInt("soundsVolume", soundsVolumeSlider.value);
        }
        //static Button _keybindsButton;
        static void DrawSettings() {
            DrawMenusAnim();
            Core.renderer.DrawText(zero, settingsText);
            DrawSettingsList();
            //if(_keybindsButton.Draw()) Game.currentMenu = Menu.KeybindsEditor;
        }
        static void DrawKeybindsEditor() {
            DrawMenusAnim();
            Core.renderer.DrawText(zero, keybindsEditorText);

            int y = 17;
            foreach((string origName, InputKey key) in Bindings.keys) {
                string name = origName.AddSpaces().ToUpper();
                string[] primAndSec = key.asString.Split(',');
                string primary = primAndSec[0];
                string secondary = primAndSec.Length > 1 ? primAndSec[1] : "<NONE>";
                Core.renderer.DrawText(new Vector2i(2, y), name, ColorScheme.GetColor("settings_keybind_name"));
                Core.renderer.DrawText(new Vector2i(37, y), primary, ColorScheme.GetColor("settings_keybind_primary"));
                Core.renderer.DrawText(new Vector2i(59, y), secondary, ColorScheme.GetColor("settings_keybind_secondary"));
                y += 2;
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

            Lua.DrawUI();
            
            if(Settings.GetBool("showFps"))
                Core.renderer.DrawText(fpsPos, $"{fps.ToString()}/{avgFPS.ToString()} FPS", fps >= 60 ?
                    ColorScheme.GetColor("fps_good") : fps > 20 ? ColorScheme.GetColor("fps_ok") : 
                        ColorScheme.GetColor("fps_bad"), Renderer.Alignment.Right);
        }
        static readonly Vector2i fpsPos = new Vector2i(79, 59);

        public static void UpdateAnims() {
            bool useScriptCharMod =
                Scripts.Rendering.Renderer.scriptCharactersModifier != null && Game.currentMenu == Menu.Game;
            if(useScriptCharMod) Core.renderer.charactersModifier = Scripts.Rendering.Renderer.scriptCharactersModifier;
            if(fadeInFinished && fadeOutFinished) { if(!useScriptCharMod) Core.renderer.charactersModifier = null; }
            else {
                if(!fadeInFinished) {
                    fadeInFinished = true;
                    _fadeInTime += Core.deltaTime;
                }
                if(fadeOutFinished) return;
                fadeOutFinished = true;
                _fadeOutTime += Core.deltaTime;
            }
        }
    }
}
