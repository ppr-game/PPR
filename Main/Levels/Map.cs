using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using PPR.GUI;
using PPR.Rendering;

namespace PPR.Main.Levels {
    public static class Map {
        public static Level currentLevel;
        public static readonly Vector2 gameLinePos = new Vector2(0, 54);
        public static readonly Vector2 editorLinePos = new Vector2(0, 44);
        public static Vector2 linePos;
        static readonly float[] lineFlashTimes = new float[Core.renderer.width];
        public static int flashLine {
            set => lineFlashTimes[value] = 1f;
        }
        public static event EventHandler onDraw;
        public static void Draw() {
            if(Game.currentMenu != Menu.Game) return;

            for(int x = 0; x < Core.renderer.width; x++) {
                Vector2 pos = new Vector2(x, linePos.y);
                Core.renderer.SetCellColor(pos, ColorScheme.GetColor("foreground"),
                    Renderer.AnimateColor(lineFlashTimes[x], ColorScheme.GetColor("background"),
                        ColorScheme.GetColor("foreground"), 1f));
                lineFlashTimes[x] -= Core.deltaTime * 3f;
            }
            Core.renderer.DrawText(linePos,
                "────────────────────────────────────────────────────────────────────────────────");
            if(Game.editing) {
                int doubleFrequency = currentLevel.metadata.linesFrequency * 2;
                for(int y = -linePos.y; y < 30 + currentLevel.metadata.linesFrequency; y++) {
                    int useY = y + Game.roundedOffset % doubleFrequency - doubleFrequency + linePos.y;
                    if(useY > gameLinePos.y) continue;
                    if(y % currentLevel.metadata.linesFrequency == 0)
                        for(int x = 0; x < 80; x++)
                            Renderer.instance.SetCellColor(new Vector2(x, useY), ColorScheme.GetColor("foreground"),
                                ColorScheme.GetColor("light_guidelines"));
                    else if(y % 2 == 0)
                        for(int x = 0; x < 80; x++)
                            Renderer.instance.SetCellColor(new Vector2(x, useY), ColorScheme.GetColor("foreground"),
                                ColorScheme.GetColor("guidelines"));
                }

                /*for(int x = 0; x < 80; x++) {
                    Core.renderer.SetCharacter(new Vector2(x, 1), ((x - 6) / 12).ToString()[0],
                        ColorScheme.GetColor("foreground"), ColorScheme.GetColor("transparent"));
                    if((x - 6) % 12 != 0) continue;
                    for(int y = 0; y < 60; y++)
                        Core.renderer.SetCellColor(new Vector2(x, y), ColorScheme.GetColor("foreground"),
                            ColorScheme.GetColor("guidelines"));
                }*/
            }

            DestroyToDestroy();
            foreach(LevelObject obj in currentLevel.objects) obj.Draw();
            
            onDraw?.Invoke(null, EventArgs.Empty);
        }
        public static void ClearCustomScriptEvents() {
            onDraw = null;
        }

        static void DestroyToDestroy() {
            int destroyIndex = 0;
            while(destroyIndex < currentLevel.objects.Count) {
                LevelObject obj = currentLevel.objects[destroyIndex];
                if(obj.toDestroy) _ = currentLevel.objects.Remove(obj);
                else destroyIndex++;
            }
        }
        public static void StepAll() {
            if(Game.currentMenu != Menu.Game) return;

            foreach(LevelObject obj in currentLevel.objects) obj.Step();
        }
        public static void SimulateAll() {
            if(Game.currentMenu != Menu.Game) return;

            foreach(LevelObject obj in currentLevel.objects) obj.Simulate();
            DestroyToDestroy();
        }
        public static void LoadLevelFromLines(string[] lines, string name, string musicPath, string scriptPath) {
            linePos = Game.editing ? editorLinePos : gameLinePos;
            currentLevel = new Level(lines, name, File.Exists(scriptPath) ? File.ReadAllText(scriptPath) : null);
            Game.GameStart(musicPath);
        }
        public static string TextFromLevel(Level level) {
            string[] lines = new string[5];
            string hpDrain = level.metadata.hpDrain.ToString();
            string hpRestorage = level.metadata.hpRestorage.ToString();
            string diff = level.metadata.difficulty;
            string author = level.metadata.author;
            string linesFreq = level.metadata.linesFrequency.ToString();
            string initialOffset = level.metadata.initialOffsetMs.ToString();
            List<LevelObject> objects = level.objects.FindAll(obj => obj.character != LevelObject.SPEED_CHAR);
            lines[0] = string.Join("", objects.Select(obj => obj.character.ToString().ToLower()));
            lines[1] = string.Join(':', objects.Select(obj => obj.step));
            lines[2] = string.Join(':', level.speeds.Select(speed => speed.speed));
            lines[3] = string.Join(':', level.speeds.Select(speed => speed.step));
            lines[4] = $"{hpDrain}:{hpRestorage}:{diff}:{author}:{linesFreq}:{initialOffset}";
            return string.Join('\n', lines);
        }
        public static List<LevelScore> ScoresFromLines(string[] lines, Vector2 position) {
            return lines.Select(t => t.Split(':')).Select((data, i) =>
                    new LevelScore(new Vector2(position.x, position.y + i * 4), int.Parse(data[0]), int.Parse(data[1]),
                        int.Parse(data[2]), new int[] { int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]) }))
                .ToList();
        }
        public static string TextFromScore(LevelScore score) {
            return string.Join(':',
                new int[] {
                    score.score, score.accuracy, score.maxCombo, score.scores[0], score.scores[1], score.scores[2]
                });
        }
    }
}
