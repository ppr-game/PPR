using System.Collections.Generic;
using System.Linq;

using PPR.GUI;
using PPR.Rendering;

using SFML.Graphics;

namespace PPR.Main.Levels {
    public static class Map {
        public static Level currentLevel;
        public static readonly Vector2 linePos = new Vector2(0, 54);
        public static void Draw() {
            if(Game.currentMenu != Menu.Game) return;

            Renderer.instance.DrawText(linePos, "────────────────────────────────────────────────────────────────────────────────",
                                                                                    ColorScheme.white, Color.Transparent);
            if(Game.editing) {
                int doubleFrequency = currentLevel.metadata.linesFrequency * 2;
                for(int y = -linePos.y / 2; y < linePos.y / 2 + 30 + currentLevel.metadata.linesFrequency; y++) {
                    int useY = y * 2 + Game.roundedOffset % doubleFrequency - doubleFrequency + linePos.y + 2;
                    if(useY > linePos.y) continue;
                    if(y % currentLevel.metadata.linesFrequency == 0)
                        for(int x = 0; x < 80; x++)
                            Renderer.instance.SetCellColor(new Vector2(x, useY), ColorScheme.white, new Color(6, 6, 6));
                    else
                        for(int x = 0; x < 80; x++)
                            Renderer.instance.SetCellColor(new Vector2(x, useY), ColorScheme.white, new Color(4, 4, 4));
                }
            }

            DestroyToDestroy();
            foreach(LevelObject obj in currentLevel.objects) obj.Draw();
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
        public static void LoadLevelFromLines(string[] lines, string name, string musicPath) {
            currentLevel = new Level(lines, name);
            Game.GameStart(musicPath);
        }
        public static string TextFromLevel(Level level) {
            string[] lines = new string[5];
            List<LevelObject> objects = level.objects.FindAll(obj => obj.character != LevelObject.speedChar);
            lines[0] = string.Join("", objects.Select(obj => obj.character.ToString().ToLower()));
            lines[1] = string.Join(':', objects.Select(obj => obj.step));
            lines[2] = string.Join(':', level.speeds.Select(speed => speed.speed));
            lines[3] = string.Join(':', level.speeds.Select(speed => speed.step));
            lines[4] = level.metadata.hpDrain + ":" +
                                   level.metadata.hpRestorage + ":" +
                                   level.metadata.difficulty + ":" +
                                   level.metadata.author + ":" +
                                   level.metadata.linesFrequency + ":" +
                                   level.metadata.initialOffsetMs;
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
