using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using MoonSharp.Interpreter;

using NCalc;

using PPR.GUI;
using PPR.GUI.Elements;
using PPR.Main.Levels;
using PPR.Properties;

using PRR;

using SFML.Graphics;
using SFML.System;

using Renderer = PRR.Renderer;
using Text = PPR.GUI.Elements.Text;

namespace PPR.LuaConsole.GUI {
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct AnimExCtx {
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
        // ReSharper disable once InconsistentNaming
        private static readonly Random _random = new Random();
        public int randomInt(int min, int max) => _random.Next(min, max);
        public double random(double min, double max) => _random.NextDouble() * (max - min) + min;
        public double posRandom(int x, int y) => posRandom(x, y, 0d);
        public double posRandom(int x, int y, double @default) =>
            PPR.GUI.UI.positionRandoms.TryGetValue(new Vector2i(x, y), out float value) ? value : @default;
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
    internal class Animation {
        public Func<AnimExCtx, int> x;
        public Func<AnimExCtx, int> y;
        public Func<AnimExCtx, string> character;
        public Func<AnimExCtx, byte> bgR;
        public Func<AnimExCtx, byte> bgG;
        public Func<AnimExCtx, byte> bgB;
        public Func<AnimExCtx, byte> bgA;
        public Func<AnimExCtx, byte> fgR;
        public Func<AnimExCtx, byte> fgG;
        public Func<AnimExCtx, byte> fgB;
        public Func<AnimExCtx, byte> fgA;
        public Clock clock;
    }
    [MoonSharpHideMember("scriptAnimations")]
    public class UI {
        public static Dictionary<string,
                Func<float, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>>> scriptAnimations;
        public static Dictionary<string, Dictionary<string, DynValue>> animations {
            set {
                if(value == null) {
                    scriptAnimations = null;
                    return;
                }
                
                Dictionary<string, Animation> anims = new Dictionary<string, Animation>(value.Count);
                foreach((string name, Dictionary<string, DynValue> animation) in value) {
                    Animation anim = new Animation();
                    foreach((string key, DynValue dynValue) in animation)
                        switch(key) {
                            case "x": anim.x = new Expression(dynValue.String).ToLambda<AnimExCtx, int>();
                                continue;
                            case "y": anim.y = new Expression(dynValue.String).ToLambda<AnimExCtx, int>();
                                continue;
                            case "character":
                                anim.character = new Expression(dynValue.String).ToLambda<AnimExCtx, string>();
                                continue;
                            default: {
                                if(dynValue.Type == DataType.Table)
                                    foreach(TablePair pair in dynValue.Table.Pairs) {
                                        Func<AnimExCtx, byte> exp =
                                            new Expression(pair.Value.String).ToLambda<AnimExCtx, byte>();
                                        switch(key) {
                                            case "background":
                                                switch(pair.Key.Number) {
                                                    case 1: anim.bgR = exp;
                                                        continue;
                                                    case 2: anim.bgG = exp;
                                                        continue;
                                                    case 3: anim.bgB = exp;
                                                        continue;
                                                    case 4: anim.bgA = exp;
                                                        continue;
                                                }
                                                break;
                                            case "foreground":
                                                switch(pair.Key.Number) {
                                                    case 1: anim.fgR = exp;
                                                        continue;
                                                    case 2: anim.fgG = exp;
                                                        continue;
                                                    case 3: anim.fgB = exp;
                                                        continue;
                                                    case 4: anim.fgA = exp;
                                                        continue;
                                                }
                                                break;
                                        }
                                    }
                                break;
                            }
                        }
                    anim.clock = new Clock();
                    anims.Add(name, anim);
                }

                scriptAnimations = new Dictionary<string,
                    Func<float, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>>>();
                foreach((string name, Animation animation) in anims) {
                    scriptAnimations.Add(name, time => {
                        AnimExCtx context = new AnimExCtx();
                        return (pos, character) => {
                            context.x = pos.X;
                            context.y = pos.Y;
                            context.character = character.character;
                            context.bgR = character.background.R;
                            context.bgG = character.background.G;
                            context.bgB = character.background.B;
                            context.bgA = character.background.A;
                            context.fgR = character.foreground.R;
                            context.fgG = character.foreground.G;
                            context.fgB = character.foreground.B;
                            context.fgA = character.foreground.A;
                            context.time = time;
                            Vector2i modPos = pos;
                            RenderCharacter modChar = character;
                            modPos = new Vector2i(animation.x?.Invoke(context) ?? modPos.X,
                                animation.y?.Invoke(context) ?? modPos.Y);
                            modChar = new RenderCharacter(animation.character?.Invoke(context)[0] ?? modChar.character,
                                new Color(animation.bgR?.Invoke(context) ?? modChar.background.R,
                                    animation.bgG?.Invoke(context) ?? modChar.background.G,
                                    animation.bgB?.Invoke(context) ?? modChar.background.B,
                                    animation.bgA?.Invoke(context) ?? modChar.background.A),
                                new Color(animation.fgR?.Invoke(context) ?? modChar.foreground.R,
                                    animation.fgG?.Invoke(context) ?? modChar.foreground.G,
                                    animation.fgB?.Invoke(context) ?? modChar.foreground.B,
                                    animation.fgA?.Invoke(context) ?? modChar.foreground.A));
                            return (modPos, modChar);
                        };
                    });
                }
            }
        }

        public static string currentSelectedLevel {
            get => PPR.GUI.UI.currSelectedLevel;
            set => PPR.GUI.UI.currSelectedLevel = value;
        }

        public static string currentSelectedDiff {
            get => PPR.GUI.UI.currSelectedDiff;
            set => PPR.GUI.UI.currSelectedDiff = value;
        }

        public static void Reload() {
            Bindings.Reload();
            ColorScheme.Reload();
        }

        public static void AnimateElement(string id, string animation, float time, bool endState, Closure endCallback) {
            UIElement element = null;
            if(id != null && !PPR.GUI.UI.currentLayout.elements.TryGetValue(id, out element))
                throw new ArgumentException($"Element {id} doesn't exist.");

            PPR.GUI.UI.AnimateElement(element, animation, time, endState, endCallback);
        }

        public static UIElement GetElement(string id) {
            UIElement element = null;
            if(id != null && !PPR.GUI.UI.currentLayout.elements.TryGetValue(id, out element))
                throw new ArgumentException($"Element {id} doesn't exist.");

            return element;
        }

        public static List<UIElement> GetElements(string tag) => PPR.GUI.UI.currentLayout.elements.Values
            .Where(elem => elem.tags.Contains(tag)).ToList();

        public static string GetPreviousMenu(string currentMenu) => currentMenu switch {
            "game" => "lastStats",
            "lastStats" => "levelSelect",
            "keybinds" => "settings",
            _ => "mainMenu"
        };

        public static Panel CreatePanel(string id, List<string> tags, int x, int y, int width, int height,
            float anchorX, float anchorY, string parent) {
            UIElement useParent = null;
            if(!string.IsNullOrWhiteSpace(parent) &&
               !PPR.GUI.UI.currentLayout.elements.TryGetValue(parent, out useParent))
                throw new ArgumentException($"Element {parent} doesn't exist.");

            Panel newPanel = new Panel(id, tags, new Vector2i(x, y), new Vector2i(width, height),
                new Vector2f(anchorX, anchorY), useParent);
            PPR.GUI.UI.currentLayout.AddElement(id, newPanel);
            return newPanel;
        }

        public static Mask CreateMask(string id, List<string> tags, int x, int y, int width, int height,
            float anchorX, float anchorY, string parent) {
            UIElement useParent = null;
            if(!string.IsNullOrWhiteSpace(parent) &&
               !PPR.GUI.UI.currentLayout.elements.TryGetValue(parent, out useParent))
                throw new ArgumentException($"Element {parent} doesn't exist.");

            Mask newMask = new Mask(id, tags, new Vector2i(x, y), new Vector2i(width, height),
                new Vector2f(anchorX, anchorY), useParent);
            PPR.GUI.UI.currentLayout.AddElement(id, newMask);
            return newMask;
        }

        public static Text CreateText(string id, List<string> tags, int x, int y, float anchorX, float anchorY,
            string parent, string text, Renderer.Alignment align, bool replacingSpaces, bool invertOnDarkBackground) {
            UIElement useParent = null;
            if(!string.IsNullOrWhiteSpace(parent) &&
               !PPR.GUI.UI.currentLayout.elements.TryGetValue(parent, out useParent))
                throw new ArgumentException($"Element {parent} doesn't exist.");

            Text newText = new Text(id, tags, new Vector2i(x, y), new Vector2f(anchorX, anchorY), useParent, text,
                align, replacingSpaces, invertOnDarkBackground);
            PPR.GUI.UI.currentLayout.AddElement(id, newText);
            return newText;
        }

        public static Button CreateButton(string id, List<string> tags, int x, int y, int width,
            float anchorX, float anchorY, string parent, string text, Renderer.Alignment align) {
            UIElement useParent = null;
            if(!string.IsNullOrWhiteSpace(parent) &&
               !PPR.GUI.UI.currentLayout.elements.TryGetValue(parent, out useParent))
                throw new ArgumentException($"Element {parent} doesn't exist.");

            Button newButton = new Button(id, tags, new Vector2i(x, y), width, new Vector2f(anchorX, anchorY),
                useParent, text, null, align);
            PPR.GUI.UI.currentLayout.AddElement(id, newButton);
            return newButton;
        }

        public static void DeleteElement(string id) => PPR.GUI.UI.currentLayout.RemoveElement(id);
        public static void DeleteElementWithIndex(int index) => PPR.GUI.UI.currentLayout.RemoveElement(index);

        public static bool ElementExists(string id) => PPR.GUI.UI.currentLayout.elements.ContainsKey(id);

        public static string GetLevelNameFromButton(string id) {
            if(!PPR.GUI.UI.currentLayout.elements.TryGetValue(id, out UIElement element))
                throw new ArgumentException($"Element {id} doesn't exist.");
            if(!(element is Button button))
                throw new ArgumentException($"Element {id} is not a button.");

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach((string levelName, LevelSelectLevel level) in PPR.GUI.UI.levelSelectLevels)
                if(level.button == button)
                    return levelName;

            return null;
        }

        public static DynValue GetLevelAndDiffNamesFromButton(string id) {
            if(!PPR.GUI.UI.currentLayout.elements.TryGetValue(id, out UIElement element))
                throw new ArgumentException($"Element {id} doesn't exist.");
            if(!(element is Button button))
                throw new ArgumentException($"Element {id} is not a button.");

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach((string levelName, LevelSelectLevel level) in PPR.GUI.UI.levelSelectLevels) {
                foreach((string diffName, LevelSelectDiff diff) in level.diffs)
                    if(diff.button == button)
                        return DynValue.NewTuple(DynValue.NewString(levelName), DynValue.NewString(diffName));
            }

            return DynValue.NewTuple(DynValue.Nil, DynValue.Nil);
        }

        public static DynValue GetLevelMetadata(string levelName, string diffName) {
            if(!PPR.GUI.UI.levelSelectLevels.TryGetValue(levelName, out LevelSelectLevel level))
                throw new ArgumentException($"Level {levelName} doesn't exist.");
            if(!level.diffs.TryGetValue(diffName, out LevelSelectDiff diff))
                throw new ArgumentException($"Difficulty {diffName} doesn't exist in level {levelName}.");

            bool lua = File.Exists(Path.Join("levels", levelName, "script.lua")) ||
                       File.Exists(Path.Join("levels", levelName, $"{diffName}.lua"));

            return DynValue.NewTuple(DynValue.NewString(diff.metadata.length),
                DynValue.NewString(diff.metadata.displayDifficulty),
                DynValue.NewString(diff.metadata.bpm),
                DynValue.NewString(diff.metadata.author),
                DynValue.NewBoolean(lua),
                DynValue.NewString(diff.metadata.objectCount.ToString()),
                DynValue.NewString(diff.metadata.speedsCount.ToString())
            );
        }

        public static List<DynValue> GetLevelScores(Script script, string levelName, string diffName) {
            if(!PPR.GUI.UI.levelSelectLevels.TryGetValue(levelName, out LevelSelectLevel level))
                throw new ArgumentException($"Level {levelName} doesn't exist.");
            if(!level.diffs.TryGetValue(diffName, out LevelSelectDiff diff))
                throw new ArgumentException($"Difficulty {diffName} doesn't exist in level {levelName}.");

            return diff.scores.Select(score => DynValue.NewTable(script, DynValue.NewNumber(score.score),
                DynValue.NewNumber(score.accuracy), DynValue.NewNumber(score.maxCombo),
                DynValue.FromObject(script, score.scores))).ToList();
        }
    }
}
