using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using PPR.UI;
using PPR.Main.Managers;

using PRR;

using SFML.Graphics;
using SFML.System;

namespace PPR.Main.Levels {
    public static class Map {
        public static Level currentLevel { get; private set; }
        public static Vector2i gameLinePos { get; } = new Vector2i(0, 54);
        public static Vector2i editorLinePos { get; } = new Vector2i(0, 44);
        public static Vector2i linePos { get; private set; }
        public const char LineCharacter = '─';
        public static int flashLine {
            set { if(!lineFlashLocks[value]) lineFlashTimes[value] = 1f; }
        }
        public static int repayLine {
            set { if(!lineFlashLocks[value]) lineFlashTimes[value] = 0f; }
        }
        public static int lockLineFlash {
            set => lineFlashLocks[value] = true;
        }
        public static int unlockLineFlash {
            set => lineFlashLocks[value] = false;
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public static List<ClipboardLevelNote> clipboard { get; } = new List<ClipboardLevelNote>();
        public static bool selecting {
            get => _selecting;
            set {
                _selecting = value;

                if(currentLevel == null) return;
                
                // Delete overlapping objects that can occur because of MoveVertical() and MoveHorizontal()
                foreach(LevelNote lvlObj in currentLevel.notes.Where(lvlObj =>
                    movingNotes.Any(movObj => movObj != lvlObj &&
                                                                 movObj.character == lvlObj.character &&
                                                                 lvlObj.startPosition == movObj.startPosition)))
                    lvlObj.remove = RemoveType.NoAnimation;

                movingNotes.Clear();
            }
        }
        public static int selectionStart => _selectionStart <= _selectionEnd ? _selectionStart : _selectionEnd;
        public static int selectionEnd => _selectionStart >= _selectionEnd ? _selectionStart : _selectionEnd;
        private static readonly float[] lineFlashTimes = new float[Core.renderer.width];
        private static readonly bool[] lineFlashLocks = new bool[Core.renderer.width];
        private static bool _selecting;
        private static int _selectionStart;
        private static int _selectionEnd;
        private static bool _prevLMB;
        private static Vector2f _prevMPosF;
        private static readonly List<LevelNote> movingNotes = new List<LevelNote>();
        public static void Draw() {
            if(!UI.Manager.currentLayout.IsElementEnabled("game")) return;

            //   L     I     N     E
            for(int x = 0; x < Core.renderer.width; x++) {
                Core.renderer.SetCharacter(new Vector2i(linePos.X + x, linePos.Y), 
                    new RenderCharacter(LineCharacter, Core.renderer.background, ColorScheme.GetColor("foreground")));
            }
            // Handle line flashing
            for(int x = 0; x < Core.renderer.width; x++) {
                Vector2i pos = new Vector2i(x, linePos.Y);
                Core.renderer.SetCellColor(pos, ColorScheme.GetColor("foreground"),
                    Renderer.LerpColors(Core.renderer.background,
                        ColorScheme.GetColor("foreground"), lineFlashTimes[x]));
                lineFlashTimes[x] -= Core.deltaTime * 3f;
            }
            if(Game.editing) {
                // Selection
                if(Core.renderer.mousePosition.Y >= 1 && Core.renderer.mousePosition.Y <= gameLinePos.Y)
                    if(Core.renderer.leftButtonPressed) {
                        _selectionEnd = linePos.Y - Core.renderer.mousePosition.Y + Game.roundedOffset;
                        
                        if(Core.renderer.leftButtonPressed != _prevLMB) {
                            selecting = false;
                            _selectionStart = _selectionEnd;
                        }

                        if(Core.renderer.mousePositionF.Y != _prevMPosF.Y) selecting = true;
                    }

                Color foreground = OffsetSelected(Game.roundedOffset) ? ColorScheme.GetColor("selected_note") :
                    ColorScheme.GetColor("foreground");

                // Draw the editor lines
                int doubleFrequency = currentLevel.metadata.linesFrequency * 2;
                for(int y = -linePos.Y; y < 30 + currentLevel.metadata.linesFrequency; y++) {
                    int offsetY = y + Game.roundedOffset % doubleFrequency - doubleFrequency;
                    int useY = offsetY + linePos.Y;
                    bool selected = OffsetSelected(Game.roundedOffset - offsetY);
                    if(useY > gameLinePos.Y) continue;
                    if(y % currentLevel.metadata.linesFrequency == 0)
                        for(int x = 0; x < 80; x++)
                            Core.renderer.SetCellColor(new Vector2i(x, useY), foreground,
                                ColorScheme.GetColor(selected ? "selected_light_guidelines" : "light_guidelines"));
                    else if(y % 2 == 0)
                        for(int x = 0; x < 80; x++)
                            Core.renderer.SetCellColor(new Vector2i(x, useY), foreground,
                                ColorScheme.GetColor(selected ? "selected_guidelines" : "guidelines"));
                    else if(selected)
                        for(int x = 0; x < 80; x++)
                            Core.renderer.SetCellColor(new Vector2i(x, useY), foreground,
                                ColorScheme.GetColor("selection"));
                }
            }

            DestroyToDestroy();
            foreach(LevelObject obj in currentLevel.objects) obj.Draw();

            Lua.Manager.DrawMap();

            _prevLMB = Core.renderer.leftButtonPressed;
            _prevMPosF = Core.renderer.mousePositionF;
        }

        public static bool OffsetSelected(float offset) {
            if(!selecting) return false;
            return offset >= selectionStart && offset <= selectionEnd;
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public static IEnumerable<LevelNote> GetSelectedNotes() => currentLevel.notes.Where(obj =>
            OffsetSelected(Calc.StepsToOffset(obj.step)));

        private static void DestroyToDestroy() {
            int destroyIndex = 0;
            while(destroyIndex < currentLevel.objects.Count) {
                LevelObject obj = currentLevel.objects[destroyIndex];
                switch(obj.remove) {
                    case RemoveType.None: destroyIndex++;
                        break;
                    case RemoveType.Normal when obj.removeAnimation:
                        currentLevel.objects.Add(obj.GetRemoveAnimation(currentLevel.speeds));
                        currentLevel.objects.Remove(obj);
                        break;
                    default: currentLevel.objects.Remove(obj);
                        break;
                }
            }
        }
        public static void StepAll() {
            if(!UI.Manager.currentLayout.IsElementEnabled("game")) return;

            foreach(LevelObject obj in currentLevel.objects) obj.Step();
        }
        public static void TickAll() {
            if(!UI.Manager.currentLayout.IsElementEnabled("game") || currentLevel.objects.Count <= 0) return;

            foreach(LevelObject obj in currentLevel.objects) {
                if(!Game.editing && obj is LevelNote note) note.Input();
                obj.Tick();
            }
            DestroyToDestroy();
        }
        public static void LoadLevelFromPath(string path, string name, string diff, bool loadMusic = true) {
            string scriptPath = Path.Join(path, $"{diff}.lua");
            if(!File.Exists(scriptPath)) scriptPath = Path.Join(path, "script.lua");
            LoadLevelFromLines(File.ReadAllLines(Path.Join(path, $"{diff}.txt")), name, diff,
                loadMusic ? SoundManager.GetSoundFilePath(Path.Join(path, "music")) : "", scriptPath);
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public static void LoadLevelFromLines(string[] lines, string name, string diff, string musicPath,
            string scriptPath) {
            linePos = Game.editing ? editorLinePos : gameLinePos;
            currentLevel = new Level(lines, name, diff, File.Exists(scriptPath) ? scriptPath : null);
            Game.StartGame(musicPath);
        }
        public static string TextFromLevel(Level level) {
            string[] lines = new string[5];
            string hpDrain = level.metadata.hpDrain.ToString();
            string hpRestorage = level.metadata.hpRestorage.ToString();
            string diff = level.metadata.displayDifficulty;
            string author = level.metadata.author;
            string linesFreq = level.metadata.linesFrequency.ToString();
            string musicOffset = level.metadata.musicOffset.ToString();
            lines[0] = string.Join("", level.notes.Select(obj => obj.ToString().ToLower()));
            lines[1] = string.Join(':', level.notes.Select(obj => obj.StepToString()));
            lines[2] = string.Join(':', level.speeds.Select(speed => speed.speed));
            lines[3] = string.Join(':', level.speeds.Select(speed => speed.step));
            lines[4] = $"{hpDrain}:{hpRestorage}:{diff}:{author}:{linesFreq}:{musicOffset}";
            return string.Join('\n', lines);
        }
        
        public static List<LevelScore> ScoresFromLines(string[] lines) {
            List<LevelScore> list = new List<LevelScore>();
            for(int i = 0; i < lines.Length; i++) {
                string[] data = lines[i].Split(':');
                list.Add(new LevelScore(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]),
                    new int[] { int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]) }));
            }

            return list;
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public static string TextFromScore(LevelScore score) => string.Join(':',
            new int[] {
                score.score, score.accuracy, score.maxCombo, score.scores[0], score.scores[1], score.scores[2]
            });
        public static void SaveScore(string name, string diff, int score, int accuracy, int maxCombo, int[] scores) {
            accuracy = Math.Clamp(accuracy, 0, 100);
            
            string path = diff == "level" ? Path.Join("scores", $"{name}.txt") :
                Path.Join("scores", name, $"{diff}.txt");
            string text = File.Exists(path) ? File.ReadAllText(path) : "";
            text = $"{TextFromScore(new LevelScore(score, accuracy, maxCombo, scores))}\n{text}";
            Directory.CreateDirectory("scores");
            if(Path.GetFileName(Path.GetDirectoryName(path)) == currentLevel.metadata.name)
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
        }
        public static bool Cut() => Copy(true);
        public static void Copy() => Copy(false);
        private static bool Copy(bool cut) {
            bool changed = false;

            clipboard.Clear();

            IEnumerable<LevelNote> selectedNotes = GetSelectedNotes().OrderBy(obj => obj.step);

            if(!selectedNotes.Any()) return false;

            LevelNote firstNote = selectedNotes.First();

            foreach(LevelNote obj in GetSelectedNotes()) {
                clipboard.Add(new ClipboardLevelNote(obj.character, obj.step - firstNote.step,
                    obj.startPosition.X, obj.constructor));

                if(!cut) continue;

                currentLevel.objects.Remove(obj);

                changed = true;
            }

            return changed;
        }
        public static bool Paste() {
            bool changed = false;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach(ClipboardLevelNote obj in clipboard)
                if(currentLevel.objects.All(mapObj => mapObj.character != obj.character ||
                                                      mapObj.startPosition.X != obj.xPos ||
                                                      mapObj.step != obj.step + Game.roundedSteps ||
                                                      mapObj is LevelNote mapNote &&
                                                      mapNote.constructor != obj.constructor)) {
                    int newStep = obj.step + Game.roundedSteps;
                    
                    // Remove objects that are in the way to prevent overlapping
                    foreach(LevelObject lvlObj in currentLevel.objects.Where(lvlObj =>
                                                  obj.character == lvlObj.character &&
                                                  newStep == lvlObj.step &&
                                                  obj.xPos == lvlObj.startPosition.X))
                        lvlObj.remove = RemoveType.NoAnimation;
                    
                    currentLevel.objects.Add(obj.constructor(obj.character, newStep, currentLevel.speeds));

                    changed = true;
                }

            return changed;
        }
        public static bool Erase() {
            bool changed = false;
            
            IEnumerable<LevelNote> objects = currentLevel.notes.Where(obj =>
                selecting ? OffsetSelected(Calc.StepsToOffset(obj.step)) : obj.step == (int)Game.steps);
            
            foreach(LevelNote obj in objects) {
                obj.remove = RemoveType.NoAnimation;
                            
                changed = true;
            }

            return changed;
        }
        
        public static bool MoveVertical(int steps) {
            bool changed = false;
            
            IEnumerable<LevelObject> selectedObjects = GetSelectedNotes().OrderBy(obj => obj.step);

            if(!selectedObjects.Any()) return false;

            List<LevelNote> toMove =
                movingNotes.Count == 0 ? GetSelectedNotes().ToList() : new List<LevelNote>(movingNotes);

            LevelObject firstObj = selectedObjects.First();
            LevelObject lastObj = selectedObjects.Last();
            int first = (int)Calc.StepsToOffset(firstObj.step, currentLevel.speeds);
            int last = (int)Calc.StepsToOffset(lastObj.step, currentLevel.speeds);
            int firstOff = (int)Calc.StepsToOffset(firstObj.step + steps, currentLevel.speeds);
            int lastOff = (int)Calc.StepsToOffset(lastObj.step + steps, currentLevel.speeds);

            foreach(LevelNote obj in toMove) {
                obj.remove = RemoveType.NoAnimation;

                LevelNote note = obj.constructor(Game.GetNoteBinding(obj.key), obj.step + steps, currentLevel.speeds);
                currentLevel.objects.Add(note);
                movingNotes.Add(note);

                movingNotes.Remove(obj);
                
                changed = true;
            }

            _selectionStart = _selectionStart - first + firstOff;
            _selectionEnd = _selectionEnd - last + lastOff;
            
            if(steps > 0 && _selectionStart > linePos.Y + Game.roundedOffset - 1 ||
               steps < 0 && _selectionEnd < Game.roundedOffset - gameLinePos.Y + linePos.Y)
                Game.ScrollTime(steps);
            
            return changed;
        }
        public static bool MoveHorizontal(int characters) {
            bool changed = false;
            
            List<LevelNote> toMove =
                movingNotes.Count == 0 ? GetSelectedNotes().ToList() : new List<LevelNote>(movingNotes);


            foreach(LevelNote obj in toMove) {
                for(int i = 0; i < LevelObject.lines.Length; i++) {
                    // ReSharper disable once HeapView.BoxingAllocation
                    int charIndex = LevelObject.lines[i].IndexOf(Game.GetNoteBinding(obj.key));
                    if(charIndex < 0) {
                        if(i == LevelObject.lines.Length - 1) return false;
                        continue;
                    }
                    if(charIndex + characters < 0 || charIndex + characters >= LevelObject.lines[i].Length)
                        return false;
                    break;
                }
            }

            foreach(LevelNote obj in toMove) {
                obj.remove = RemoveType.NoAnimation;

                char character = Game.GetNoteBinding(obj.key);
                foreach(string line in LevelObject.lines) {
                    int charIndex = line.IndexOf(Game.GetNoteBinding(obj.key));
                    if(charIndex < 0) continue;
                    character = line[charIndex + characters];
                    break;
                }

                LevelNote note = obj.constructor(character, obj.step, currentLevel.speeds);
                currentLevel.objects.Add(note);
                movingNotes.Add(note);

                movingNotes.Remove(obj);
                
                changed = true;
            }
            
            return changed;
        }
    }
}
