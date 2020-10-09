using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using MoonSharp.Interpreter;

using NCalc;

using PPR.Main;

using PRR;

using SFML.Graphics;
using SFML.System;

namespace PPR.Scripts.Rendering {
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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
        static readonly Random _random = new Random();
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
    [MoonSharpHideMember("scriptCharactersModifier")]
    public class Renderer {
        public static int getWidth => PPR.Core.renderer.width;
        public static int getHeight => PPR.Core.renderer.height;
        public static Vector2i getMousePosition => PPR.Core.renderer.mousePosition;
        public static Vector2f getMousePositionF => PPR.Core.renderer.mousePositionF;
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
                    charMod.creationTime = Game.timeFromStart.AsSeconds();
                    charMods.Add(charMod);
                }
                
                scriptCharactersModifier = (pos, character) => {
                    CharModExCtx context = new CharModExCtx {
                        x = pos.X,
                        y = pos.Y,
                        character = character.character,
                        bgR = character.background.R,
                        bgG = character.background.G,
                        bgB = character.background.B,
                        bgA = character.background.A,
                        fgR = character.foreground.R,
                        fgG = character.foreground.G,
                        fgB = character.foreground.B,
                        fgA = character.foreground.A,
                        roundedSteps = Game.roundedSteps,
                        steps = Game.steps,
                        offset = Game.roundedOffset
                    };
                    Vector2f modPos = (Vector2f)pos;
                    RenderCharacter modChar = character;
                    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                    foreach(CharacterModifier modifier in charMods) {
                        context.levelTime = Game.timeFromStart.AsSeconds();
                        context.startTime = modifier.creationTime;
                        context.time = context.levelTime - modifier.creationTime;
                        if(!modifier.condition(context)) continue;
                        modPos = new Vector2f(modifier.x?.Invoke(context) ?? modPos.X,
                            modifier.y?.Invoke(context) ?? modPos.Y);
                        modChar = new RenderCharacter(modifier.character?.Invoke(context)[0] ?? modChar.character,
                            new Color(modifier.bgR?.Invoke(context) ?? modChar.background.R,
                                modifier.bgG?.Invoke(context) ?? modChar.background.G,
                                modifier.bgB?.Invoke(context) ?? modChar.background.B,
                                modifier.bgA?.Invoke(context) ?? modChar.background.A),
                            new Color(modifier.fgR?.Invoke(context) ?? modChar.foreground.R,
                                modifier.fgG?.Invoke(context) ?? modChar.foreground.G,
                                modifier.fgB?.Invoke(context) ?? modChar.foreground.B,
                                modifier.fgA?.Invoke(context) ?? modChar.foreground.A));
                    }
                    return (modPos, modChar);
                };
            }
        }
        
        public static void DrawText(Vector2i position, string text, Color foregroundColor, Color backgroundColor,
            Color defaultBackground, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, text, foregroundColor, backgroundColor, defaultBackground,
            align, replacingSpaces, invertOnDarkBG, charactersModifier);
        
        public static void DrawText(Vector2i position, string[] lines, Color foregroundColor, Color backgroundColor,
            Color defaultBackground, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, lines, foregroundColor, backgroundColor,
            defaultBackground, align, replacingSpaces, invertOnDarkBG, charactersModifier);
        
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static void DrawLines(Vector2i position, string[] lines, Color foregroundColor, Color backgroundColor,
            Color defaultBackground, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawLines(position, lines, foregroundColor, backgroundColor,
            defaultBackground, align, replacingSpaces, invertOnDarkBG, charactersModifier);
        
        public static void SetCharacter(Vector2i position, RenderCharacter character, Color defaultBackground) =>
            PPR.Core.renderer.SetCharacter(position, character, defaultBackground);
        public static RenderCharacter GetCharacter(Vector2i position) => PPR.Core.renderer.GetCharacter(position);
        public static void SetCellColor(Vector2i position, Color foregroundColor, Color backgroundColor,
            Color defaultBackground) =>
            PPR.Core.renderer.SetCellColor(position, foregroundColor, backgroundColor, defaultBackground);
        public static Color LerpColors(Color a, Color b, float t) => PRR.Renderer.LerpColors(a, b, t);
        public static Color AnimateColor(float time, Color start, Color end, float rate) =>
            PRR.Renderer.AnimateColor(time, start, end, rate);
        public static Color BlendColors(Color bottom, Color top) => PRR.Renderer.BlendColors(bottom, top);
        
        public static void DrawText(Vector2i position, string text,
            PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, text, align,
            replacingSpaces, invertOnDarkBG, charactersModifier);
        
        public static void DrawText(Vector2i position, string text, Color color,
            PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, text, color, align, replacingSpaces,
            invertOnDarkBG, charactersModifier);
        
        public static void DrawText(Vector2i position, string text, Color foregroundColor, Color backgroundColor,
            PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, text, foregroundColor, backgroundColor,
            align, replacingSpaces, invertOnDarkBG, charactersModifier);
        
        public static void DrawText(Vector2i position, string[] lines, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, lines, align, replacingSpaces, invertOnDarkBG,
            charactersModifier);
        
        public static void DrawText(Vector2i position, string[] lines, Color color, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, lines, color, align, replacingSpaces, invertOnDarkBG,
            charactersModifier);
        
        public static void DrawText(Vector2i position, string[] lines, Color foregroundColor, Color backgroundColor,
            PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, lines, foregroundColor, backgroundColor, align,
            replacingSpaces,
            invertOnDarkBG, charactersModifier);
        
        public static void DrawLines(Vector2i position, string[] lines, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawLines(position, lines, align, replacingSpaces,
            invertOnDarkBG, charactersModifier);
        
        public static void DrawLines(Vector2i position, string[] lines, Color color, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawLines(position, lines, color, align, replacingSpaces,
            invertOnDarkBG, charactersModifier);
        
        public static void DrawLines(Vector2i position, string[] lines, Color foregroundColor,
            Color backgroundColor, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawLines(position, lines, foregroundColor, backgroundColor, align,
            replacingSpaces, invertOnDarkBG, charactersModifier);
    }
}
