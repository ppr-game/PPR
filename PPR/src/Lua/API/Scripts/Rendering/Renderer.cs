using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using MoonSharp.Interpreter;

using NCalc;

using PER.Abstractions.Renderer;

using PPR.Main;
using PPR.UI;

using PRR;

using SFML.Graphics;
using SFML.System;

namespace PPR.Lua.API.Scripts.Rendering {
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal struct BackgroundModExCtx {
        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }
        public byte a { get; set; }
        public float time { get; set; }
        public float startTime { get; set; }
        public float levelTime { get; set; }
        public float roundedSteps { get; set; }
        public float steps { get; set; }
        public float offset { get; set; }
        private static readonly Random _random = new Random();
        public int randomInt(int min, int max) => _random.Next(min, max);
        public double random(double min, double max) => _random.NextDouble() * (max - min) + min;
        public double lerp(double a, double b, double t) => t <= 0 ? a : t >= 1 ? b : a + (b - a) * t;
        public double abs(double value) => Math.Abs(value);
        public double ceil(double value) => ceiling(value);
        // ReSharper disable once MemberCanBeMadeStatic.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public double ceiling(double value) => Math.Ceiling(value);
        public double clamp(double value, double min, double max) => Math.Clamp(value, min, max);
        public double floor(double value) => Math.Floor(value);
        public double max(double a, double b) => Math.Max(a, b);
        public double min(double a, double b) => Math.Min(a, b);
        public double pow(double a, double b) => Math.Pow(a, b);
        public double round(double value) => Math.Round(value);
        public double sign(double value) => Math.Sign(value);
        public double sqrt(double value) => Math.Sqrt(value);
    }
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal struct CharModExCtx {
        public int x { get; set; }
        public int y { get; set; }
        public char character { get; set; }
        public byte bgR { get; set; }
        public byte bgG { get; set; }
        public byte bgB { get; set; }
        public byte bgA { get; set; }
        public byte fgR { get; set; }
        public byte fgG { get; set; }
        public byte fgB { get; set; }
        public byte fgA { get; set; }
        public float time { get; set; }
        public float startTime { get; set; }
        public float levelTime { get; set; }
        public float roundedSteps { get; set; }
        public float steps { get; set; }
        public float offset { get; set; }
        // ReSharper disable once InconsistentNaming
        private static readonly Random _random = new Random();
        public int randomInt(int min, int max) => _random.Next(min, max);
        public double random(double min, double max) => _random.NextDouble() * (max - min) + min;
        public static double posRandom(int x, int y) => posRandom(x, y, 0d);
        public static double posRandom(int x, int y, double @default) =>
            UI.Manager.positionRandoms.TryGetValue(new Vector2i(x, y), out float value) ? value : @default;
        public double lerp(double a, double b, double t) => t <= 0 ? a : t >= 1 ? b : a + (b - a) * t;
        public double abs(double value) => Math.Abs(value);
        public double ceil(double value) => ceiling(value);
        // ReSharper disable once MemberCanBeMadeStatic.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public double ceiling(double value) => Math.Ceiling(value);
        public double clamp(double value, double min, double max) => Math.Clamp(value, min, max);
        public double floor(double value) => Math.Floor(value);
        public double max(double a, double b) => Math.Max(a, b);
        public double min(double a, double b) => Math.Min(a, b);
        public double pow(double a, double b) => Math.Pow(a, b);
        public double round(double value) => Math.Round(value);
        public double sign(double value) => Math.Sign(value);
        public double sqrt(double value) => Math.Sqrt(value);
    }
    internal class BackgroundModifier {
        public Func<BackgroundModExCtx, bool> condition;
        public Func<BackgroundModExCtx, byte> r;
        public Func<BackgroundModExCtx, byte> g;
        public Func<BackgroundModExCtx, byte> b;
        public Func<BackgroundModExCtx, byte> a;
        public float creationTime;
    }
    internal class CharacterModifier {
        public Func<CharModExCtx, bool> condition;
        public Func<CharModExCtx, float> x;
        public Func<CharModExCtx, float> y;
        public Func<CharModExCtx, string> character;
        public Func<CharModExCtx, byte> bgR;
        public Func<CharModExCtx, byte> bgG;
        public Func<CharModExCtx, byte> bgB;
        public Func<CharModExCtx, byte> bgA;
        public Func<CharModExCtx, byte> fgR;
        public Func<CharModExCtx, byte> fgG;
        public Func<CharModExCtx, byte> fgB;
        public Func<CharModExCtx, byte> fgA;
        public float creationTime;
    }
    [MoonSharpHideMember("scriptBackgroundModifier")]
    [MoonSharpHideMember("scriptCharactersModifier")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Renderer {
        public static int getWidth => PPR.Core.renderer.width;
        public static int getHeight => PPR.Core.renderer.height;
        public static Vector2i getMousePosition => PPR.Core.renderer.mousePosition;
        public static Vector2f getAccurateMousePosition => PPR.Core.renderer.accurateMousePosition;
        public static Func<Color, Color> scriptBackgroundModifier;
        public static List<Dictionary<string, DynValue>> backgroundModifier {
            set {
                if(value == null) {
                    PPR.Core.renderer.ResetBackground();
                    scriptBackgroundModifier = null;
                    return;
                }
                
                List<BackgroundModifier> bgMods = new List<BackgroundModifier>(value.Count);
                foreach(Dictionary<string, DynValue> modifier in value) {
                    BackgroundModifier bgMod = new BackgroundModifier();
                    foreach((string key, DynValue dynValue) in modifier) {
                        switch(key) {
                            case "condition":
                                bgMod.condition = new Expression(dynValue.String).ToLambda<BackgroundModExCtx, bool>();
                                continue;
                            case "r":
                                bgMod.r = new Expression(dynValue.String).ToLambda<BackgroundModExCtx, byte>();
                                continue;
                            case "g":
                                bgMod.g = new Expression(dynValue.String).ToLambda<BackgroundModExCtx, byte>();
                                continue;
                            case "b":
                                bgMod.b = new Expression(dynValue.String).ToLambda<BackgroundModExCtx, byte>();
                                continue;
                            case "a":
                                bgMod.a = new Expression(dynValue.String).ToLambda<BackgroundModExCtx, byte>();
                                continue;
                        }
                    }
                    bgMod.creationTime = Game.levelTime.AsSeconds();
                    bgMods.Add(bgMod);
                }

                scriptBackgroundModifier = color => {
                    BackgroundModExCtx context = new BackgroundModExCtx {
                        r = color.R,
                        g = color.G,
                        b = color.B,
                        a = color.A,
                        roundedSteps = Game.roundedSteps,
                        steps = Game.steps,
                        offset = Game.roundedOffset
                    };
                    Color modColor = color;
                    foreach(BackgroundModifier modifier in bgMods) {
                        context.levelTime = Game.levelTime.AsSeconds();
                        context.startTime = modifier.creationTime;
                        context.time = context.levelTime - modifier.creationTime;
                        if(!modifier.condition(context)) continue;
                        modColor = new Color(modifier.r?.Invoke(context) ?? modColor.R,
                            modifier.g?.Invoke(context) ?? modColor.G,
                            modifier.b?.Invoke(context) ?? modColor.B,
                            modifier.a?.Invoke(context) ?? modColor.A);
                    }
                    return modColor;
                };
            }
        }
        public static Func<Vector2i, RenderCharacter, (Vector2f, RenderCharacter)> scriptCharactersModifier;
        public static List<Dictionary<string, DynValue>> charactersModifier {
            set {
                if(value == null) {
                    scriptCharactersModifier = null;
                    return;
                }
                
                List<CharacterModifier> charMods = new List<CharacterModifier>(value.Count);
                foreach(Dictionary<string, DynValue> modifier in value) {
                    CharacterModifier charMod = new CharacterModifier();
                    foreach((string key, DynValue dynValue) in modifier)
                        switch(key) {
                            case "condition":
                                charMod.condition = new Expression(dynValue.String).ToLambda<CharModExCtx, bool>();
                                continue;
                            case "x": charMod.x = new Expression(dynValue.String).ToLambda<CharModExCtx, float>();
                                continue;
                            case "y": charMod.y = new Expression(dynValue.String).ToLambda<CharModExCtx, float>();
                                continue;
                            case "character":
                                charMod.character = new Expression(dynValue.String).ToLambda<CharModExCtx, string>();
                                continue;
                            default: {
                                if(dynValue.Type == DataType.Table)
                                    foreach(TablePair pair in dynValue.Table.Pairs) {
                                        Func<CharModExCtx, byte> exp =
                                            new Expression(pair.Value.String).ToLambda<CharModExCtx, byte>();
                                        switch(key) {
                                            case "background":
                                                switch(pair.Key.Number) {
                                                    case 1: charMod.bgR = exp;
                                                        continue;
                                                    case 2: charMod.bgG = exp;
                                                        continue;
                                                    case 3: charMod.bgB = exp;
                                                        continue;
                                                    case 4: charMod.bgA = exp;
                                                        continue;
                                                }
                                                break;
                                            case "foreground":
                                                switch(pair.Key.Number) {
                                                    case 1: charMod.fgR = exp;
                                                        continue;
                                                    case 2: charMod.fgG = exp;
                                                        continue;
                                                    case 3: charMod.fgB = exp;
                                                        continue;
                                                    case 4: charMod.fgA = exp;
                                                        continue;
                                                }
                                                break;
                                        }
                                    }
                                break;
                            }
                        }
                    charMod.creationTime = Game.levelTime.AsSeconds();
                    charMods.Add(charMod);
                }
                
                scriptCharactersModifier = (pos, character) => {
                    CharModExCtx context = new CharModExCtx {
                        x = pos.X,
                        y = pos.Y,
                        character = character.character,
                        bgR = character.background.r,
                        bgG = character.background.g,
                        bgB = character.background.b,
                        bgA = character.background.a,
                        fgR = character.foreground.r,
                        fgG = character.foreground.g,
                        fgB = character.foreground.b,
                        fgA = character.foreground.a,
                        roundedSteps = Game.roundedSteps,
                        steps = Game.steps,
                        offset = Game.roundedOffset
                    };
                    Vector2f modPos = (Vector2f)pos;
                    RenderCharacter modChar = character;
                    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                    foreach(CharacterModifier modifier in charMods) {
                        context.levelTime = Game.levelTime.AsSeconds();
                        context.startTime = modifier.creationTime;
                        context.time = context.levelTime - modifier.creationTime;
                        if(!modifier.condition(context)) continue;
                        modPos = new Vector2f(modifier.x?.Invoke(context) ?? modPos.X,
                            modifier.y?.Invoke(context) ?? modPos.Y);
                        modChar = new RenderCharacter(modifier.character?.Invoke(context)[0] ?? modChar.character,
                            new Color(modifier.bgR?.Invoke(context) ?? modChar.background.r,
                                modifier.bgG?.Invoke(context) ?? modChar.background.g,
                                modifier.bgB?.Invoke(context) ?? modChar.background.b,
                                modifier.bgA?.Invoke(context) ?? modChar.background.a),
                            new Color(modifier.fgR?.Invoke(context) ?? modChar.foreground.r,
                                modifier.fgG?.Invoke(context) ?? modChar.foreground.g,
                                modifier.fgB?.Invoke(context) ?? modChar.foreground.b,
                                modifier.fgA?.Invoke(context) ?? modChar.foreground.a));
                    }
                    return (modPos, modChar);
                };
            }
        }

        public static IReadOnlyDictionary<Vector2i, float> positionRandoms => UI.Manager.positionRandoms;
        public static void RegenPositionRandoms() => UI.Manager.RegenPositionRandoms();
        
        public static void DrawText(int x, int y, string text, Color? foregroundColor = null,
            Color? backgroundColor = null, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(new Vector2i(x, y), text,
            foregroundColor ?? ColorScheme.GetColor("foreground"),
            backgroundColor ?? ColorScheme.GetColor("transparent"), align, replacingSpaces, invertOnDarkBG,
            charactersModifier);
        
        public static void DrawLines(int x, int y, string[] lines, Color? foregroundColor = null,
            Color? backgroundColor = null, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawLines(new Vector2i(x, y), lines,
            foregroundColor ?? ColorScheme.GetColor("foreground"),
            backgroundColor ?? ColorScheme.GetColor("transparent"), align, replacingSpaces, invertOnDarkBG,
            charactersModifier);

        public static void SetCharacter(int x, int y, RenderCharacter character) =>
            PPR.Core.renderer.DrawCharacter(new Vector2i(x, y), character);
        public static RenderCharacter GetCharacter(Vector2i position) => PPR.Core.renderer.GetCharacter(position);
        public static void SetCellColor(int x, int y, Color foregroundColor, Color backgroundColor) =>
            PPR.Core.renderer.SetCharacterColor(new Vector2i(x, y), foregroundColor, backgroundColor);
        public static Color LerpColors(Color a, Color b, float t) => PRR.Renderer.LerpColors(a, b, t);
        public static Color AnimateColor(float time, Color start, Color end, float rate) =>
            PRR.Renderer.AnimateColor(time, start, end, rate);
        public static Color BlendColors(Color bottom, Color top) => PRR.Renderer.OverlayColors(bottom, top);
    }
}
