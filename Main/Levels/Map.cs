using System.Collections.Generic;
using System.IO;
using System.Linq;

using PPR.GUI;

using PRR;

using SFML.System;

namespace PPR.Main.Levels {
    public static class Map {
        public static Level currentLevel;
        public static readonly Vector2i gameLinePos = new Vector2i(0, 54);
        public static readonly Vector2i editorLinePos = new Vector2i(0, 44);
        public static Vector2i linePos { get; private set; }
        static readonly float[] lineFlashTimes = new float[Core.renderer.width];
        public static int flashLine {
            set => lineFlashTimes[value] = 1f;
        }
        public static readonly List<ClipboardLevelObject> clipboard = new List<ClipboardLevelObject>();
        public static bool selecting;
        static int _selectionStart;
        static int _selectionEnd;
        public static int selectionStart => _selectionStart <= _selectionEnd ? _selectionStart : _selectionEnd;
        public static int selectionEnd => _selectionStart >= _selectionEnd ? _selectionStart : _selectionEnd;
        static bool lmb => Core.renderer.leftButtonPressed;
        static bool _prevLMB;
        static Vector2f _prevMPosF;
        public static void Draw() {
            if(Game.currentMenu != Menu.Game) return;

            for(int x = 0; x < Core.renderer.width; x++) {
                Vector2i pos = new Vector2i(x, linePos.Y);
                Core.renderer.SetCellColor(pos, ColorScheme.GetColor("foreground"),
                    Renderer.AnimateColor(lineFlashTimes[x], ColorScheme.GetColor("background"),
                        ColorScheme.GetColor("foreground"), 1f));
                lineFlashTimes[x] -= Core.deltaTime * 3f;
            }
            Core.renderer.DrawText(linePos,
                "────────────────────────────────────────────────────────────────────────────────");
            if(Game.editing) {
                if(Core.renderer.mousePosition.Y >= 1 && Core.renderer.mousePosition.Y <= gameLinePos.Y)
                    if(lmb) {
                        _selectionEnd = linePos.Y - Core.renderer.mousePosition.Y + Game.roundedOffset;
                        
                        if(lmb != _prevLMB) {
                            selecting = false;
                            _selectionStart = _selectionEnd;
                        }

                        if(Core.renderer.mousePositionF.Y != _prevMPosF.Y) selecting = true;
                    }

                int doubleFrequency = currentLevel.metadata.linesFrequency * 2;
                for(int y = -linePos.Y; y < 30 + currentLevel.metadata.linesFrequency; y++) {
                    int offsetY = y + Game.roundedOffset % doubleFrequency - doubleFrequency;
                    int useY = offsetY + linePos.Y;
                    bool selected = OffsetSelected(Game.roundedOffset - offsetY);
                    if(useY > gameLinePos.Y) continue;
                    if(y % currentLevel.metadata.linesFrequency == 0)
                        for(int x = 0; x < 80; x++)
                            Core.renderer.SetCellColor(new Vector2i(x, useY),
                                ColorScheme.GetColor("foreground"),
                                ColorScheme.GetColor(selected ? "selected_light_guidelines" : "light_guidelines"));
                    else if(y % 2 == 0)
                        for(int x = 0; x < 80; x++)
                            Core.renderer.SetCellColor(new Vector2i(x, useY),
                                ColorScheme.GetColor("foreground"),
                                ColorScheme.GetColor(selected ? "selected_guidelines" : "guidelines"));
                    else if(selected)
                        for(int x = 0; x < 80; x++)
                            Core.renderer.SetCellColor(new Vector2i(x, useY),
                                ColorScheme.GetColor("foreground"),
                                ColorScheme.GetColor("selection"));
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

            Lua.DrawMap();

            _prevLMB = Core.renderer.leftButtonPressed;
            _prevMPosF = Core.renderer.mousePositionF;
        }

        public static bool OffsetSelected(float offset) {
            if(!selecting) return false;
            return offset >= selectionStart && offset <= selectionEnd;
        }
        public static List<LevelObject> GetSelectedObjects() => currentLevel.objects.FindAll(obj =>
            OffsetSelected(Game.StepsToOffset(obj.step)) && obj.character != LevelObject.SpeedChar);

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
        public static void LoadLevelFromPath(string path, string name, string diff, bool loadMusic = true) {
            string scriptPath = Path.Join(path, $"{diff}.lua");
            if(!File.Exists(scriptPath)) scriptPath = Path.Join(path, "script.lua");
            LoadLevelFromLines(File.ReadAllLines(Path.Join(path, $"{diff}.txt")), name, diff,
                loadMusic ? Game.GetSoundFilePath(Path.Join(path, "music")) : "", scriptPath);
        }
        public static void LoadLevelFromLines(string[] lines, string name, string diff, string musicPath, string scriptPath) {
            linePos = Game.editing ? editorLinePos : gameLinePos;
            currentLevel = new Level(lines, name, diff, File.Exists(scriptPath) ? scriptPath : null);
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
            List<LevelObject> objects = level.objects.FindAll(obj => obj.character != LevelObject.SpeedChar);
            lines[0] = string.Join("", objects.Select(obj => obj.character.ToString().ToLower()));
            lines[1] = string.Join(':', objects.Select(obj => obj.step));
            lines[2] = string.Join(':', level.speeds.Select(speed => speed.speed));
            lines[3] = string.Join(':', level.speeds.Select(speed => speed.step));
            lines[4] = $"{hpDrain}:{hpRestorage}:{diff}:{author}:{linesFreq}:{initialOffset}";
            return string.Join('\n', lines);
        }
        public static List<LevelScore> ScoresFromLines(string[] lines, Vector2i position) {
            List<LevelScore> list = new List<LevelScore>();
            for(int i = 0; i < lines.Length; i++) {
                string[] data = lines[i].Split(':');
                list.Add(new LevelScore(new Vector2i(position.X, position.Y + i * 4), int.Parse(data[0]),
                    int.Parse(data[1]), int.Parse(data[2]),
                    new int[] { int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]) }));
            }

            return list;
        }
        public static string TextFromScore(LevelScore score) => string.Join(':',
            new int[] {
                score.score, score.accuracy, score.maxCombo, score.scores[0], score.scores[1], score.scores[2]
            });
    }
}
