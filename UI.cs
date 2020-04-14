using System;
using System.Collections.Generic;
using System.IO;

using PPR.Core;
using PPR.Levels;
using PPR.Rendering;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace PPR.GUI {
    public enum ButtonState { Idle, Hovered, Clicked, Selected };
    public class Button {
        public Vector2 position;
        public string text;
        public readonly int width;
        public Color idleColor;
        public Color hoverColor;
        public Color clickColor;
        Color currentColor;
        Color prevColor;
        public ButtonState currentButton = ButtonState.Clicked;
        ButtonState prevButton = ButtonState.Hovered;
        public ButtonState prevFrameButton = ButtonState.Hovered;
        readonly float[] animTimes;
        readonly float[] animRateOffsets;
        public bool selected = false;
        public Button(Vector2 position, string text, int width, Color idleColor, Color hoverColor, Color clickColor) {
            this.position = position;
            this.text = text;
            this.width = width;
            this.idleColor = idleColor;
            this.hoverColor = hoverColor;
            this.clickColor = clickColor;
            animTimes = new float[width];
            animRateOffsets = new float[width];
            currentColor = hoverColor;
        }

        ButtonState DrawWithState(Vector2 position, string text) {
            Renderer.instance.DrawText(position, text.Substring(0, Math.Min(text.Length, width)), Color.White, Color.Transparent);
            Vector2 maxBound = new Vector2(position.x + width - 1, position.y);
            return Renderer.instance.mousePosition.InBounds(position, maxBound)
                              ? Mouse.IsButtonPressed(Mouse.Button.Left) ? ButtonState.Clicked : ButtonState.Hovered
                               : selected ? ButtonState.Selected : ButtonState.Idle;
        }
        public bool Draw() {
            prevFrameButton = currentButton;
            currentButton = DrawWithState(position, text);
            if(prevButton != currentButton) {
                Color color = idleColor;
                switch(currentButton) {
                    case ButtonState.Hovered:
                        color = hoverColor;
                        break;
                    case ButtonState.Selected:
                    case ButtonState.Clicked:
                        color = clickColor;
                        break;
                }
                if(currentColor != color) {
                    prevColor = currentColor;
                    for(int x = 0; x < width; x++) {
                        animTimes[x] = 0f;
                        animRateOffsets[x] = new Random().NextFloat(-1f, 1f);
                    }
                }
                currentColor = color;
            }
            prevButton = currentButton;

            for(int x = 0; x < width; x++) {
                Vector2 pos = position + new Vector2(x, 0);
                //Color fg = Color.White - new Color(prevColor.R, prevColor.G, prevColor.B, 0);
                //Color bg = Color.White - new Color(currentColor.R, currentColor.G, currentColor.B, 0);
                Renderer.instance.SetCellColor(pos, Renderer.AnimateColor(animTimes[x], currentColor, prevColor, 4f + animRateOffsets[x]),
                                                                                                           Renderer.AnimateColor(animTimes[x], prevColor, currentColor, 4f + animRateOffsets[x]));
                animTimes[x] += MainGame.deltaTime;
            }
            return Renderer.instance.window.HasFocus() ? currentButton == ButtonState.Clicked && prevFrameButton != ButtonState.Clicked : false;
        }
    }
    public static class UI {
        public static int fps = 0;

        static readonly Color[] prevHealthColors = new Color[80];
        static readonly Color[] healthColors = new Color[80];
        static readonly float[] healthAnimTimes = new float[80];
        static readonly float[] healthAnimRateOffsets = new float[80];
        public static int health {
            set {
                for(int x = 0; x < 80; x++) {
                    Color color = value > x ? Color.Red : new Color(16, 0, 0);
                    if(healthColors[x] != color) {
                        prevHealthColors[x] = healthColors[x];
                        healthAnimTimes[x] = 0f;
                        healthAnimRateOffsets[x] = new Random().NextFloat(-3f, 3f);
                    }
                    healthColors[x] = color;
                }
            }
        }
        static readonly string mainMenuText = File.ReadAllText(Path.Combine("resources", "ui", "mainMenu.txt"));
        static readonly string levelSelectText = File.ReadAllText(Path.Combine("resources", "ui", "levelSelect.txt"));
        static readonly string lastStatsText = File.ReadAllText(Path.Combine("resources", "ui", "lastStats.txt"));
        static readonly List<Button> mainMenuButtons = new List<Button>() {
            new Button(new Vector2(38, 25), "PLAY", 4, Color.Black, Color.Green, Color.Green),
            new Button(new Vector2(38, 27), "EDIT", 4, Color.Black, Color.Yellow, Color.Yellow),
            new Button(new Vector2(38, 29), "EXIT", 4, Color.Black, Color.Red, Color.Red),
        };
        public static List<Button> levelSelectLevels = new List<Button>();
        public static List<LevelScore> levelSelectScores;
        static readonly List<Button> levelSelectButtons = new List<Button>() {
            new Button(new Vector2(28, 10), "AUTO", 4, Color.Black, Color.Blue, new Color(0, 0, 64)),
            new Button(new Vector2(28, 10), "NEW", 3, Color.Black, Color.Green, Color.Green),
        };
        static string lastLevel = "";
        static readonly List<Button> lastStatsButtons = new List<Button>() {
            new Button(new Vector2(2, 53), "CONTINUE", 8, Color.Black, Color.Cyan, Color.Cyan),
            new Button(new Vector2(2, 55), "RESTART", 7, Color.Black, Color.Yellow, Color.Yellow),
            new Button(new Vector2(2, 55), "SAVE", 4, Color.Black, Color.Blue, new Color(0, 0, 64)),
            new Button(new Vector2(2, 57), "EXIT", 4, Color.Black, Color.Red, Color.Red),
        };
        static readonly List<Button> levelEditorButtons = new List<Button>() {
            new Button(new Vector2(78, 58), "►", 1, Color.Black, Color.Green, new Color(0, 64, 0)),
        };
        static readonly Button skipButton = new Button(new Vector2(74, 58), "SKIP", 4, Color.Black, new Color(255, 127, 0), new Color(255, 127, 0));

        static readonly Vector2 zero = Vector2.zero;
        static void DrawMainMenu() {
            Renderer.instance.DrawText(zero, mainMenuText, Color.White, Color.Black);
            foreach(Button button in mainMenuButtons) {
                if(button.Draw()) {
                    if(button.text == "PLAY" || button.text == "EDIT") {
                        Game.editing = button.text == "EDIT";
                        Game.auto = false;
                        Game.currentMenu = Menu.LevelSelect;
                        Game.GenerateLevelList();
                    }
                    else if(button.text == "EXIT") {
                        Renderer.instance.window.Close();
                    }
                }
            }
        }
        static readonly Vector2 scoresPos = new Vector2(1, 12);
        static void DrawLevelSelect() {
            Renderer.instance.DrawText(zero, levelSelectText, Color.White, Color.Black);
            for(int i = 0; i < levelSelectLevels.Count; i++) {
                Button button = levelSelectLevels[i];
                if(button.position.y < 12 || button.position.y > 49) continue;
                if(button.Draw()) {
                    lastLevel = button.text;
                    string path = Path.Combine("levels", lastLevel);
                    Map.LoadLevelFromLines(File.ReadAllLines(Path.Combine(path, "level.txt")), lastLevel, Path.Combine(path, "music.ogg"));
                    Game.currentMenu = Menu.Game;
                    Game.RecalculatePosition();
                }
                if(button.currentButton == ButtonState.Hovered && button.prevFrameButton != ButtonState.Hovered) {
                    string levelPath = Path.Combine("levels", button.text);
                    Game.selectedMetadata = new LevelMetadata(File.ReadAllLines(Path.Combine(levelPath, "level.txt")), button.text);
                    string musicPath = Path.Combine(levelPath, "music.ogg");
                    if(File.Exists(musicPath)) {
                        Game.music.Stop();
                        Game.music = new Music(musicPath) {
                            Volume = Game.musicVolume
                        };
                        Game.music.Play();
                    }

                    levelSelectScores = null;
                    string scoresPath = Path.Combine("scores", button.text + ".txt");
                    if(File.Exists(scoresPath)) {
                        levelSelectScores = Map.ScoresFromLines(File.ReadAllLines(scoresPath), scoresPos);
                    }
                }
            }
            foreach(Button button in levelSelectButtons) {
                if(button.text == "NEW" && Game.editing && button.Draw()) {
                    lastLevel = "unnamed";
                    Map.LoadLevelFromLines(File.ReadAllLines(Path.Combine("levels", "_template", "level.txt")), lastLevel, "");
                    Game.currentMenu = Menu.Game;
                    Game.RecalculatePosition();
                }
                else if(button.text == "AUTO" && !Game.editing) {
                    if(button.Draw()) Game.auto = !Game.auto;
                    button.selected = Game.auto;
                }
            }
            DrawMetadata(Game.selectedMetadata);
            DrawScores(levelSelectScores);
        }
        static readonly Vector2 metaLengthPos = new Vector2(53, 12);
        static readonly Vector2 metaDiffPos = new Vector2(53, 13);
        static readonly Vector2 metaBPMpos = new Vector2(53, 14);
        static readonly Vector2 metaAuthorPos = new Vector2(53, 15);

        static readonly Vector2 metaObjCountPos = new Vector2(53, 48);
        static readonly Vector2 metaSpdCountPos = new Vector2(53, 49);
        static void DrawMetadata(LevelMetadata? metadata) {
            if(metadata == null) return;
            Renderer.instance.DrawText(metaLengthPos, "LENGTH:" + metadata.Value.length, Color.White, Color.Transparent);
            Renderer.instance.DrawText(metaDiffPos, "DIFFICULTY:" + metadata.Value.difficulty, Color.White, Color.Transparent);
            Renderer.instance.DrawText(metaBPMpos, "BPM:" + metadata.Value.bpm, Color.White, Color.Transparent);
            Renderer.instance.DrawText(metaAuthorPos, "AUTHOR:" + metadata.Value.author, Color.White, Color.Transparent);

            Renderer.instance.DrawText(metaObjCountPos, "objects:" + metadata.Value.objectCount, Color.White, Color.Transparent);
            Renderer.instance.DrawText(metaSpdCountPos, "speeds:" + metadata.Value.speedsCount, Color.White, Color.Transparent);
        }
        static void DrawScores(List<LevelScore> scores) {
            if(scores == null) return;
            foreach(LevelScore score in scores) {
                if(score.scorePosition.y >= 12 && score.scorePosition.y <= 49) Renderer.instance.DrawText(score.scorePosition, score.scoreStr, Color.Blue, Color.Transparent);
                if(score.accComboPosition.y >= 12 && score.accComboPosition.y <= 49) Renderer.instance.DrawText(score.accComboPosition, score.accCombo, Color.Blue, Color.Transparent);
                if(score.scoresPosition.y >= 12 && score.scoresPosition.y <= 49) DrawMiniScores(score.scoresPosition, score.scores);
                if(score.linePosition.y >= 12 && score.linePosition.y <= 49) Renderer.instance.DrawText(score.linePosition, "├──────────────────────────┤", Color.White, Color.Transparent);
            }
        }
        static readonly Vector2 levelNamePos = new Vector2(0, 0);
        static readonly Vector2 scorePos = new Vector2(0, 57);
        static readonly Vector2 accPos = new Vector2(0, 58);
        static readonly Vector2 comboPos = new Vector2(0, 59);
        static readonly Vector2 miniScoresPos = new Vector2(25, 59);
        static readonly Vector2 bpmPos = new Vector2(0, 57);
        static readonly Vector2 timePos = new Vector2(0, 58);
        static void DrawGame() {
            if(Game.editing) {
                foreach(Button button in levelEditorButtons) {
                    if(button.Draw()) {
                        if(button.text == "►") {
                            Game.music.Play();
                            button.text = "║";
                        }
                        else if(button.text == "║") {
                            Game.music.Pause();
                            button.text = "►";
                            Game.offset = (int)Game.offset;
                            Game.RecalculateTime();
                        }
                    }
                }
                Renderer.instance.DrawText(bpmPos, "BPM: " + Game.currentBPM +
                                                                                                          "   HP DRAIN:" + Map.currentLevel.metadata.hpDrain, Color.White, Color.Transparent);
                Renderer.instance.DrawText(timePos, "TIME: " + Game.music.PlayingOffset.AsSeconds() + "sec" +
                                                                                                          "   HP RESTORAGE:" + Map.currentLevel.metadata.hpRestorage, Color.White, Color.Transparent);
                DrawLevelName(levelNamePos, Color.White);
            }
            else {
                DrawHealth();
                DrawScore(scorePos, Color.Blue);
                DrawAccuracy(accPos, Color.Blue);
                DrawCombo(comboPos);
                DrawMiniScores(miniScoresPos, Game.scores);
                DrawLevelName(levelNamePos, Color.Black);
                LevelMetadata metadata = Map.currentLevel.metadata;
                if(metadata.skippable && Game.music.PlayingOffset.AsMilliseconds() < metadata.skipTime && skipButton.Draw()) {
                    Game.music.PlayingOffset = Time.FromMilliseconds(metadata.skipTime);
                }
            }
        }
        static void DrawHealth() {
            for(int x = 0; x < 80; x++) {
                Vector2 pos = new Vector2(x, 0);
                //Console.WriteLine(x + ":" + prevHealthColors[x] + "-" + healthColors[x] + " : " + healthAnimTimes[x]);
                float rate = 3.5f + healthAnimRateOffsets[x];
                Renderer.instance.SetCellColor(pos, Color.Transparent,
                                                                                                           Renderer.AnimateColor(healthAnimTimes[x], prevHealthColors[x], healthColors[x], rate));
                healthAnimTimes[x] += MainGame.deltaTime;
            }
        }
        static int scoreChange = 0;
        static int prevScore = 0;
        static float newScoreAnimationTime = 0f;
        static void DrawScore(Vector2 position, Color color) {
            string scoreStr = "SCORE: " + Game.score;
            Renderer.instance.DrawText(position, scoreStr, color, Color.Transparent);
            if(prevScore != Game.score) {
                if(newScoreAnimationTime >= 1f) scoreChange = 0;
                newScoreAnimationTime = 0f;
                scoreChange += Game.score - prevScore;
            }
            Renderer.instance.DrawText(new Vector2(position.x + scoreStr.Length + 2, position.y), "+" + scoreChange,
                                                                                    Renderer.AnimateColor(newScoreAnimationTime, color, Color.Transparent, 2f), Color.Transparent);
            newScoreAnimationTime += MainGame.deltaTime;

            prevScore = Game.score;
        }
        static void DrawAccuracy(Vector2 position, Color color) {
            Renderer.instance.DrawText(position, "ACCURACY: " + Game.accuracy + "%", color, Color.Transparent);
        }
        static void DrawCombo(Vector2 position, bool maxCombo = false) {
            string prefix = maxCombo ? "MAX " : "";
            Color color = Color.Blue;
            if(Game.scores[0] <= 0 && Game.accuracy < 100) {
                prefix = "FULL ";
                color = Color.Yellow;
            }
            else if(Game.accuracy >= 100) {
                prefix = "PERFECT ";
                color = Color.Green;
            }
            Renderer.instance.DrawText(position, prefix + "COMBO: " + (maxCombo ? Game.maxCombo : Game.combo), color, Color.Transparent);
        }
        static void DrawMiniScores(Vector2 position, int[] scores) {
            string scores0str = scores[0].ToString();
            Renderer.instance.DrawText(position, scores0str, Color.Black, Color.Red);

            string scores1str = scores[1].ToString();
            int x1 = position.x + scores0str.Length + 1;
            Renderer.instance.DrawText(new Vector2(x1, position.y), scores1str, Color.Black, Color.Yellow);

            Renderer.instance.DrawText(new Vector2(x1 + scores1str.Length + 1, position.y), scores[2].ToString(), Color.Black, Color.Green);
        }
        static void DrawScores(Vector2 position) {
            int posXoffseted = position.x + 15;
            Renderer.instance.DrawText(position, "MISSES:", Color.Red, Color.Black);
            Renderer.instance.DrawText(new Vector2(posXoffseted, position.y), Game.scores[0].ToString(), Color.Black, Color.Red);

            int posYhits = position.y + 2;
            Renderer.instance.DrawText(new Vector2(position.x, posYhits), "HITS:", Color.Yellow, Color.Black);
            Renderer.instance.DrawText(new Vector2(posXoffseted, posYhits), Game.scores[1].ToString(), Color.Black, Color.Yellow);

            int posYPerfectHits = position.y + 4;
            Renderer.instance.DrawText(new Vector2(position.x, posYPerfectHits), "PERFECT HITS:", Color.Green, Color.Black);
            Renderer.instance.DrawText(new Vector2(posXoffseted, posYPerfectHits), Game.scores[2].ToString(), Color.Black, Color.Green);
        }
        static void DrawLevelName(Vector2 position, Color color) {
            Renderer.instance.DrawText(position, Map.currentLevel.metadata.name + " : " + Map.currentLevel.metadata.author, color, Color.Transparent);
        }
        static readonly Vector2 passFailText = new Vector2(38, 5);
        static readonly Vector2 lastLevelPos = new Vector2(2, 13);
        static readonly Vector2 lastScorePos = new Vector2(2, 16);
        static readonly Vector2 lastAccPos = new Vector2(2, 18);
        static readonly Vector2 lastScoresPos = new Vector2(25, 16);
        static readonly Vector2 lastMaxComboPos = new Vector2(2, 20);
        static void DrawLastStats() {
            Renderer.instance.DrawText(zero, lastStatsText, Color.White, Color.Black);
            string text = "PAUSE";
            Color color = Color.Cyan;
            if(!Game.editing && (Map.currentLevel.objects.Count <= 0 || Game.health <= 0)) {
                if(Game.health > 0) {
                    text = "PASS";
                    color = Color.Green;
                }
                else {
                    text = "FAIL";
                    color = Color.Red;
                }
            }
            Renderer.instance.DrawText(passFailText, text, color, Color.Transparent);
            DrawLevelName(lastLevelPos, Color.White);
            if(!Game.editing) {
                DrawScore(lastScorePos, Color.Blue);
                DrawAccuracy(lastAccPos, Color.Blue);
                DrawScores(lastScoresPos);
                DrawCombo(lastMaxComboPos, true);
            }
            foreach(Button button in lastStatsButtons) {
                if(button.text == "CONTINUE") {
                    if(Map.currentLevel.objects.Count > 0 && Game.health > 0 && button.Draw()) Game.currentMenu = Menu.Game;
                }
                else if(button.text == "RESTART") {
                    if(!Game.editing && button.Draw()) {
                        Game.currentMenu = Menu.Game;
                        string path = Path.Combine("levels", lastLevel);
                        Map.LoadLevelFromLines(File.ReadAllLines(Path.Combine(path, "level.txt")), lastLevel, Path.Combine(path, "music.ogg"));
                    }
                }
                else if(button.text == "SAVE") {
                    if(Game.editing && button.Draw()) {
                        string path = Path.Combine("levels", lastLevel);
                        _ = Directory.CreateDirectory(path);
                        File.WriteAllText(Path.Combine(path, "level.txt"), Map.TextFromLevel(Map.currentLevel));
                    }
                }
                else if(button.Draw()) {
                    if(button.text == "EXIT") {
                        Game.currentMenu = Menu.LevelSelect;
                    }
                }
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
                case Menu.Game:
                    DrawGame();
                    break;
                case Menu.LastStats:
                    DrawLastStats();
                    break;
            }
            //Renderer.instance.DrawText(zero, fps + " FPS", fps >= 60 ? Color.Green : fps > 20 ? Color.Yellow : Color.Red, Color.Transparent);
        }
    }
}
