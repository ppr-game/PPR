﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Util;

using PPROld.UI;
using PPROld.UI.Elements;
using PPROld.Main.Managers;

using PRR.Sfml;

namespace PPROld.Main.Levels;

public struct LevelSelectLevel {
    public Button button;
    public Dictionary<string, LevelSelectDiff> diffs;
}

public struct LevelSelectDiff {
    public Button button;
    public List<LevelScore> scores;
    public LevelMetadata metadata;
}

public struct LevelScore {
    public readonly int score;
    public readonly int accuracy;
    public readonly int maxCombo;
    public readonly int[] scores;

    public LevelScore(int score, int accuracy, int maxCombo, int[] scores) {
        this.score = score;
        this.accuracy = accuracy;
        this.maxCombo = maxCombo;
        this.scores = scores;
    }
}

public struct LevelMetadata {
    public readonly string name;
    public readonly string diff;
    public readonly string displayDiff;
    public int hpDrain;
    public int hpRestorage;
    private float _difficulty;
    public float difficulty {
        get => _difficulty;
        set {
            _difficulty = value;
            displayDifficulty = value.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
    public string displayDifficulty { get; private set; }
    public readonly string author;
    public TimeSpan lengthSpan;
    public string length;
    public string totalLength;
    public int maxStep;
    public int linesFrequency;
    public int musicOffset;
    public readonly string bpm;
    public readonly bool skippable;
    public readonly int skipTime;
    public readonly int objectCount;
    public readonly int speedsCount;

    private LevelMetadata(string name, string diff, IReadOnlyList<string> meta, int objectCount,
        IReadOnlyList<char> chars, IReadOnlyList<int> steps, List<LevelSpeed> speeds) {
        this.name = name;
        this.diff = diff;
        displayDiff = diff == "level" || diff == null ? "DEFAULT" : diff.ToUpper();
        hpDrain = int.Parse(meta[0]);
        hpRestorage = int.Parse(meta[1]);
        //difficulty = meta[2];
        author = meta[3];
        linesFrequency = meta.Count > 4 ? int.Parse(meta[4]) : 4;
        musicOffset = meta.Count > 5 ? int.Parse(meta[5]) : 0;

        List<LightLevelObject> objects = new List<LightLevelObject>();
        for(int i = 0; i < Math.Min(chars.Count, steps.Count); i++)
            objects.Add(new LightLevelObject(chars[i], steps[i]));
        speeds.Sort((speed1, speed2) => speed1.step.CompareTo(speed2.step));

        lengthSpan = Calc.GetLevelLength(objects, speeds, musicOffset);
        length = Calc.TimeSpanToLength(lengthSpan);
        totalLength = Calc.TimeSpanToLength(Calc.GetTotalLevelLength(objects, speeds, musicOffset));

        _difficulty = Calc.GetDifficulty(objects, speeds, (int)lengthSpan.TotalMinutes);
        displayDifficulty = _difficulty.ToString("0.00", CultureInfo.InvariantCulture);

        int minStep = objects.Count > 0 ? Calc.GetFirstObject(objects).step : 0;
        int minTime = (int)Calc.StepsToMilliseconds(minStep, speeds);
        skipTime = minTime - 3000;
        skippable = skipTime > 3000;

        IEnumerable<int> onlyAbsoluteSpeeds = speeds.Select(speed => Math.Abs(speed.speed));
        int minBPM = onlyAbsoluteSpeeds.Min();
        int maxBPM = onlyAbsoluteSpeeds.Max();
        int avgBPM = (int)Calc.GetAverageBPM(speeds, objects.Count > 0 ? Calc.GetLastObject(objects).step : 0);
        bpm = minBPM == maxBPM ? avgBPM.ToString() : $"{minBPM.ToString()}-{maxBPM.ToString()} ({avgBPM.ToString()})";

        maxStep = objects.Count > 0 ? Calc.GetLastObject(objects).step : 0;

        this.objectCount = objectCount;
        speedsCount = speeds.Count;
    }
    public LevelMetadata(Level level, IReadOnlyList<string> meta, string name, string diff) : this(name, diff, meta,
        level.notes.Count(),
        level.notes.Select(obj => obj.character).ToList(),
        level.notes.Select(obj => obj.step).ToList(), level.speeds) { }
    private static List<LevelSpeed> SpeedsFromLists(IReadOnlyList<int> speeds, IEnumerable<int> speedsSteps) {
        List<LevelSpeed> combinedSpeeds = speedsSteps.Select((t, i) => new LevelSpeed(speeds[i], t)).ToList();
        combinedSpeeds.Sort((speed1, speed2) => speed1.step.CompareTo(speed2.step));
        return combinedSpeeds;
    }
    public LevelMetadata(IReadOnlyList<string> lines, string name, string diff) : this(name, diff,
        lines[4].Split(':'),
        lines[0].ToList().FindAll(obj => obj != LevelHoldNote.DisplayChar).Count, lines[0].ToList(),
        lines[1].Length > 0 ? lines[1].Split(':').Select(int.Parse).ToList() : new List<int>(),
        SpeedsFromLists(
            lines[2].Length > 0 ? lines[2].Split(':').Select(int.Parse).ToList() : new List<int>(),
            lines[3].Length > 0 ? lines[3].Split(':').Select(int.Parse).ToList() : new List<int>())) { }
}

public class Level {
    public LevelMetadata metadata;
    public List<LevelObject> objects { get; } = new List<LevelObject>();
    public IEnumerable<LevelNote> notes => objects.OfType<LevelNote>();
    public List<LevelSpeed> speeds { get; } = new List<LevelSpeed>();
    public string script { get; }

    public static bool IsLevelValid(IReadOnlyList<string> lines) {
        bool any;
        try {
            any = lines[2].Split(':').Select(int.Parse).Any();
            any = any && lines[3].Split(':').Select(int.Parse).Any();
        }
        catch(Exception) {
            return false;
        }
        return any;
    }
    public Level(IReadOnlyList<string> lines, string name, string diff, string script = null) {
        int[] objectsSteps = lines[1].Length > 0 ? lines[1].Split(':').Select(int.Parse).ToArray() : new int[0];
        int[] speeds = lines[2].Length > 0 ? lines[2].Split(':').Select(int.Parse).ToArray() : new int[0];
        int[] speedsStarts = lines[3].Length > 0 ? lines[3].Split(':').Select(int.Parse).ToArray() : new int[0];
        for(int i = 0; i < speedsStarts.Length; i++) {
            this.speeds.Add(new LevelSpeed(speeds[i], speedsStarts[i]));
            objects.Add(new LevelSpeedObject(speedsStarts[i], this.speeds));
        }
        for(int i = 0; i < objectsSteps.Length; i++) {
            int step = objectsSteps[i];
            char character = lines[0][i];
            if(character == LevelHoldNote.DisplayChar) continue;
            char nextCharacter = i == objectsSteps.Length - 1 ? '\n' : lines[0][i + 1];
            switch(nextCharacter) {
                case LevelHoldNote.DisplayChar:
                    objects.Add(new LevelHoldNote(character, step, this.speeds));
                    break;
                default:
                    objects.Add(new LevelHitNote(character, step, this.speeds));
                    break;
            }
        }
        objects.Sort((obj1, obj2) => obj1.step.CompareTo(obj2.step));
        this.speeds.Sort((speed1, speed2) => speed1.step.CompareTo(speed2.step));
        string[] meta = lines[4].Split(':');
        metadata = new LevelMetadata(this, meta, name, diff);
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

public readonly struct LightLevelObject {
    public readonly char character;
    public readonly int step;
    public LightLevelObject(char character, int step) {
        this.character = character;
        this.step = step;
    }
}

public readonly struct RecreatableLevelNote {
    public readonly char character;
    public readonly int step;
    public readonly Func<char, int, IEnumerable<LevelSpeed>, LevelNote> constructor;
    public RecreatableLevelNote(char character, int step,
        Func<char, int, IEnumerable<LevelSpeed>, LevelNote> constructor) {
        this.character = character;
        this.step = step;
        this.constructor = constructor;
    }
}

public readonly struct ClipboardLevelNote {
    public readonly char character;
    public readonly int step;
    public readonly int xPos;
    public readonly Func<char, int, IEnumerable<LevelSpeed>, LevelNote> constructor;
    public ClipboardLevelNote(char character, int step, int xPos,
        Func<char, int, IEnumerable<LevelSpeed>, LevelNote> constructor) {
        this.character = character;
        this.step = step;
        this.xPos = xPos;
        this.constructor = constructor;
    }
}

public enum RemoveType { None, Normal, NoAnimation }

public class LevelObject {
    public static readonly string[] lines = {
        "1234567890-=", // ReSharper disable once StringLiteralTypo
        "qwertyuiop[]", // ReSharper disable once StringLiteralTypo
        "asdfghjkl;'", // ReSharper disable once StringLiteralTypo
        "zxcvbnm,./"
    };
    public static Color[] linesColors { get; set; }
    public static Color[] linesDarkColors { get; set; }
    public Vector2Int startPosition { get; }
    public char character { get; }
    public virtual char visualCharacter => character;
    public int step { get; }
    protected Color color { get; set; }
    protected Color nextDirLayerColor { get; set; }

    public RemoveType remove { get; set; }
    public virtual bool removeAnimation => false;

    protected Vector2Int position;
    protected readonly int directionLayer;

    protected LevelObject(char character, int step, int x, IEnumerable<LevelSpeed> speeds) {
        // TODO: uppercase notes
        character = false ? char.ToUpper(character) : char.ToLower(character);
        this.character = character;

        List<LevelSpeed> existingSpeeds = new List<LevelSpeed>(speeds);
        existingSpeeds.Sort((spd1, spd2) => spd1.step.CompareTo(spd2.step));

        startPosition = new Vector2Int(x,
            Map.linePos.Y - (int)Calc.StepsToOffset(step, existingSpeeds));
        position = startPosition;

        this.step = step;
        directionLayer = Calc.StepsToDirectionLayer(step, existingSpeeds);
    }

    public virtual void Draw() {
        bool onHigherOrCurrentLayer = directionLayer - Game.currentDirectionLayer >= 0;
        bool onCurrentLayer = directionLayer == Game.currentDirectionLayer;

        if((!Game.editing || position.y <= Map.gameLinePos.Y && onHigherOrCurrentLayer) && // Editor stuff
           (onCurrentLayer || Core.engine.renderer.GetCharacter(position).character == '\0')) // Draw below on lower layers
            Core.engine.renderer.DrawCharacter(position, new RenderCharacter(visualCharacter,
                // TODO: transparent color
                Color.transparent, GetColor()));
    }

    public virtual void Tick() {
        if(Game.editing || !Game.StepPassedLine(step, 1)) return;

        remove = RemoveType.Normal;
    }

    public virtual void Step() => position = new Vector2Int(position.x, startPosition.y + Game.roundedOffset);

    public virtual LevelParticle GetRemoveAnimation(IEnumerable<LevelSpeed> speeds) =>
        new LevelParticle(step, startPosition.x, speeds) {
            startColor = color, endColor = Core.engine.renderer.background, speed = 3f
        };

    private Color GetColor() => GetColor(directionLayer, Game.currentDirectionLayer, color,
        nextDirLayerColor, Map.OffsetSelected(Calc.StepsToOffset(step)));

    protected static Color GetColor(int noteDirLayer, int curDirLayer, Color color, Color nextDirLayerColor,
        bool selected) {
        int difference = Math.Abs(noteDirLayer - curDirLayer);
        // TODO: selected_note color
        if(selected && difference <= 1) return Color.black;
        return difference switch {
            0 => color,
            1 => nextDirLayerColor,
            // TODO: transparent color
            _ => Color.transparent
        };
    }

    public virtual string StepToString() => step.ToString();

    public override string ToString() => character.ToString();
}

public class LevelSpeedObject : LevelObject {
    public const char DisplayChar = '>';
    public static Color speedColor;
    public static Color nextDirLayerSpeedColor;

    public LevelSpeedObject(int step, IEnumerable<LevelSpeed> speeds) :
        base(DisplayChar, step, 0, speeds) {
        color = speedColor;
        nextDirLayerColor = nextDirLayerSpeedColor;
    }

    public override string ToString() => new string(new char[]{ character, DisplayChar });
}

public class LevelParticle : LevelObject {
    private const char DisplayChar = Map.LineCharacter;

    public Color startColor { get; set; }
    public Color endColor { get; set; }
    public float speed { get; set; }

    private float _animationTime;

    public LevelParticle(int step, int x, IEnumerable<LevelSpeed> speeds) : base(DisplayChar, step, x, speeds) =>
        position = new Vector2Int(position.x, startPosition.y + Game.roundedOffset);

    public override void Draw() {
        Core.engine.renderer.DrawCharacter(position, new RenderCharacter(
            position.y == Map.linePos.Y ? DisplayChar : '\n',
            Color.LerpColors(startColor, endColor, _animationTime),
            // TODO: foreground color
            Color.LerpColors(startColor, Color.white, _animationTime)));
        if(_animationTime >= 1f) {
            remove = RemoveType.Normal;
            Map.unlockLineFlash = startPosition.x;
        }
        // TODO: delta time timer
        _animationTime += (float)Core.engine.deltaTime * speed;

        Map.repayLine = startPosition.x;
    }

    public override void Tick() { }

    public override void Step() { }
}

public abstract class LevelNote : LevelObject {
    protected float windowMultiplier { get; }
    protected abstract int perfectWindow { get; }
    protected abstract int okWindow { get; }
    public override bool removeAnimation => true;
    public abstract Func<char, int, IEnumerable<LevelSpeed>, LevelNote> constructor { get; }
    private Color removeColor { get; set; }
    public KeyCode key { get; protected set; }
    protected static int lastNoteDistanceFromLine;
    protected static bool useLastNoteDistance;
    protected int line;

    public static readonly Dictionary<KeyCode, int> closestSteps = new Dictionary<KeyCode, int>();

    protected LevelNote(char character, int step, int x, IEnumerable<LevelSpeed> speeds) : base(character, step, x,
        speeds) => windowMultiplier = (int)(Math.Abs(Calc.GetBPMAtStep(step, speeds)) / 600f);

    public abstract void Input();

    public override void Step() {
        base.Step();

        // TODO: music
        if(Game.editing && SoundManager.music.Status == SoundStatus.Playing && step == (int)Game.steps)
            PlayHitsound();

        if(!closestSteps.ContainsKey(key) || step < closestSteps[key]) closestSteps[key] = step;
    }

    protected abstract void PlayHitsound();

    public override LevelParticle GetRemoveAnimation(IEnumerable<LevelSpeed> speeds) =>
        new LevelParticle(step, startPosition.x, speeds) {
            startColor = removeColor, endColor = Core.engine.renderer.background, speed = 3f
        };

    protected void CheckHit() => CheckHit(GetHitIndex(Math.Abs(lastNoteDistanceFromLine)));
    private void CheckHit(int hitIndex) {
        switch(hitIndex) {
            case 2: PerfectHit();
                break;
            case 1: OkHit();
                break;
            default: Miss();
                break;
        }
    }

    private void PerfectHit() {
        Hit(10, 2, 1);
        // TODO: perfect_hit color
        removeColor = new Color(0f, 1f, 0f, 1f);
    }

    private void OkHit() {
        Hit(5, 1, 2);
        // TODO: hit color
        removeColor = new Color(1f, 1f, 0f, 1f);
    }

    private static void Hit(int score, int hitIndex, int health) {
        Game.health += Map.currentLevel.metadata.hpRestorage / health;
        ScoreManager.combo++;
        ScoreManager.maxCombo = Math.Max(ScoreManager.combo, ScoreManager.maxCombo);
        ScoreManager.score += score * ScoreManager.combo;
        int newValue = ScoreManager.scores[hitIndex] + 1;
        // TODO: lua
        //Lua.Manager.InvokeEvent(null, "scoresChanged", hitIndex + 1, newValue, ScoreManager.scores[hitIndex]);
        ScoreManager.scores[hitIndex] = newValue;
    }

    protected void Miss() {
        Game.health -= Map.currentLevel.metadata.hpDrain;
        ScoreManager.combo = 0;
        int newValue = ScoreManager.scores[0] + 1;
        // TODO: lua
        //Lua.Manager.InvokeEvent(null, "scoresChanged", 1, newValue, ScoreManager.scores[0]);
        ScoreManager.scores[0] = newValue;
        // TODO: miss color
        removeColor = new Color(1f, 0f, 0f, 1f);
    }

    protected void OnNoteHitOrMiss() {
        remove = RemoveType.Normal;
        closestSteps.Remove(key);
        Map.repayLine = startPosition.x;
        ScoreManager.RecalculateAccuracy();
    }

    private int GetHitIndex(int distanceFromLine) => distanceFromLine <= perfectWindow ? 2 :
        distanceFromLine <= okWindow ? 1 : 0;
}

public class LevelHitNote : LevelNote {
    protected override int perfectWindow => (int)windowMultiplier;
    protected override int okWindow => 1 + perfectWindow + (int)(0.5f * windowMultiplier);
    private int missStep => okWindow + 1;
    public override Func<char, int, IEnumerable<LevelSpeed>, LevelNote> constructor =>
        (character, step, speeds) => new LevelHitNote(character, step, speeds);

    private bool _prevKeyState;

    public LevelHitNote(char character, int step, IEnumerable<LevelSpeed> speeds) : base(character, step,
        Calc.GetXPosForCharacter(character), speeds) {
        key = char.ToUpper(character) switch {
            '1' => KeyCode.Num1,
            '2' => KeyCode.Num2,
            '3' => KeyCode.Num3,
            '4' => KeyCode.Num4,
            '5' => KeyCode.Num5,
            '6' => KeyCode.Num6,
            '7' => KeyCode.Num7,
            '8' => KeyCode.Num8,
            '9' => KeyCode.Num9,
            '0' => KeyCode.Num0,
            '-' => KeyCode.Hyphen,
            '=' => KeyCode.Equal,
            'Q' => KeyCode.Q,
            'W' => KeyCode.W,
            'E' => KeyCode.E,
            'R' => KeyCode.R,
            'T' => KeyCode.T,
            'Y' => KeyCode.Y,
            'U' => KeyCode.U,
            'I' => KeyCode.I,
            'O' => KeyCode.O,
            'P' => KeyCode.P,
            '[' => KeyCode.LBracket,
            ']' => KeyCode.RBracket,
            'A' => KeyCode.A,
            'S' => KeyCode.S,
            'D' => KeyCode.D,
            'F' => KeyCode.F,
            'G' => KeyCode.G,
            'H' => KeyCode.H,
            'J' => KeyCode.J,
            'K' => KeyCode.K,
            'L' => KeyCode.L,
            ';' => KeyCode.Semicolon,
            '\'' => KeyCode.Quote,
            'Z' => KeyCode.Z,
            'X' => KeyCode.X,
            'C' => KeyCode.C,
            'V' => KeyCode.V,
            'B' => KeyCode.B,
            'N' => KeyCode.N,
            'M' => KeyCode.M,
            ',' => KeyCode.Comma,
            '.' => KeyCode.Period,
            '/' => KeyCode.Slash,
            _ => key
        };

        char lineChar = char.ToLower(character);
        foreach(string line in lines) {
            if(line.Contains(lineChar)) break;
            this.line++;
        }

        color = linesColors[line];
        nextDirLayerColor = linesDarkColors[line];
    }

    public override void Tick() {
        if(remove != RemoveType.None || Game.editing || !Game.StepPassedLine(step, missStep)) return;

        Miss();
        OnNoteHitOrMiss();
    }

    public override void Input() {
        if(closestSteps.ContainsKey(key) && step > closestSteps[key]) {
            _prevKeyState = true;
            Map.unlockLineFlash = startPosition.x;
            return;
        }

        bool keyState = Game.auto ? Game.roundedSteps == step : Core.engine.renderer.input.KeyPressed(key);

        if(Game.StepPassedLine(step, -missStep)) {
            Map.repayLine = startPosition.x;
            Map.lockLineFlash = startPosition.x;

            if(keyState && !_prevKeyState) {
                lastNoteDistanceFromLine = step - Game.roundedSteps;
                useLastNoteDistance = true;
                CheckHit();
                PlayHitsound();
                OnNoteHitOrMiss();
            }
        }

        _prevKeyState = keyState;
    }

    protected override void PlayHitsound() => SoundManager.PlaySound(SoundType.Hit);
}

public class LevelHoldNote : LevelNote {
    public const char DisplayChar = '│';
    public override char visualCharacter => DisplayChar;
    protected override int perfectWindow => (int)windowMultiplier;
    protected override int okWindow => 1 + perfectWindow + (int)(0.5f * windowMultiplier);
    private int missStep => okWindow + 1;
    public override Func<char, int, IEnumerable<LevelSpeed>, LevelNote> constructor =>
        (character, step, speeds) => new LevelHoldNote(character, step, speeds);

    private bool _prevKeyState;

    public LevelHoldNote(char character, int step, IEnumerable<LevelSpeed> speeds) :
        base(character, step, Calc.GetXPosForCharacter(character), speeds) {
        key = char.ToUpper(character) switch {
            '1' => KeyCode.Num1,
            '2' => KeyCode.Num2,
            '3' => KeyCode.Num3,
            '4' => KeyCode.Num4,
            '5' => KeyCode.Num5,
            '6' => KeyCode.Num6,
            '7' => KeyCode.Num7,
            '8' => KeyCode.Num8,
            '9' => KeyCode.Num9,
            '0' => KeyCode.Num0,
            '-' => KeyCode.Hyphen,
            '=' => KeyCode.Equal,
            'Q' => KeyCode.Q,
            'W' => KeyCode.W,
            'E' => KeyCode.E,
            'R' => KeyCode.R,
            'T' => KeyCode.T,
            'Y' => KeyCode.Y,
            'U' => KeyCode.U,
            'I' => KeyCode.I,
            'O' => KeyCode.O,
            'P' => KeyCode.P,
            '[' => KeyCode.LBracket,
            ']' => KeyCode.RBracket,
            'A' => KeyCode.A,
            'S' => KeyCode.S,
            'D' => KeyCode.D,
            'F' => KeyCode.F,
            'G' => KeyCode.G,
            'H' => KeyCode.H,
            'J' => KeyCode.J,
            'K' => KeyCode.K,
            'L' => KeyCode.L,
            ';' => KeyCode.Semicolon,
            '\'' => KeyCode.Quote,
            'Z' => KeyCode.Z,
            'X' => KeyCode.X,
            'C' => KeyCode.C,
            'V' => KeyCode.V,
            'B' => KeyCode.B,
            'N' => KeyCode.N,
            'M' => KeyCode.M,
            ',' => KeyCode.Comma,
            '.' => KeyCode.Period,
            '/' => KeyCode.Slash,
            _ => key
        };

        char lineChar = char.ToLower(character);
        foreach(string line in lines) {
            if(line.Contains(lineChar)) break;
            this.line++;
        }

        color = linesColors[line];
        nextDirLayerColor = linesDarkColors[line];
    }

    public override void Tick() {
        if(remove != RemoveType.None || Game.editing || !Game.StepPassedLine(step,
               useLastNoteDistance ? Math.Abs(lastNoteDistanceFromLine) + 1 :
                   missStep)) return;

        Miss();
        OnNoteHitOrMiss();
    }

    public override void Input() {
        if(closestSteps.ContainsKey(key) && step > closestSteps[key]) {
            _prevKeyState = true;
            Map.unlockLineFlash = startPosition.x;
            return;
        }

        bool keyState = Game.auto ? Game.roundedSteps == step : Core.engine.renderer.input.KeyPressed(key);

        if(!keyState && _prevKeyState) useLastNoteDistance = false;

        if(Game.StepPassedLine(step, useLastNoteDistance ? -lastNoteDistanceFromLine : -missStep)) {
            Map.repayLine = startPosition.x;
            Map.lockLineFlash = startPosition.x;

            if(keyState) {
                if(!useLastNoteDistance) {
                    lastNoteDistanceFromLine = Math.Abs(step - Game.roundedSteps);
                    useLastNoteDistance = true;
                }

                CheckHit();
                PlayHitsound();
                OnNoteHitOrMiss();
            }
        }

        _prevKeyState = keyState;
    }

    protected override void PlayHitsound() => SoundManager.PlaySound(SoundType.Hold);

    public override string StepToString() => $"{step.ToString()}:-1";

    public override string ToString() => new string(new char[]{ character, DisplayChar });
}
