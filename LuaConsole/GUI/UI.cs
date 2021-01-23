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

using PRR;

using SFML.Graphics;
using SFML.System;

using Renderer = PRR.Renderer;
using Text = PPR.GUI.Elements.Text;

namespace PPR.LuaConsole.GUI {
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal struct TransExCtx {
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
    internal class Transition {
        public Func<TransExCtx, int> x;
        public Func<TransExCtx, int> y;
        public Func<TransExCtx, string> character;
        public Func<TransExCtx, byte> bgR;
        public Func<TransExCtx, byte> bgG;
        public Func<TransExCtx, byte> bgB;
        public Func<TransExCtx, byte> bgA;
        public Func<TransExCtx, byte> fgR;
        public Func<TransExCtx, byte> fgG;
        public Func<TransExCtx, byte> fgB;
        public Func<TransExCtx, byte> fgA;
        public Func<TransExCtx, bool> finished;
        public Clock clock;
    }
    [MoonSharpHideMember("scriptTransitions")]
    [MoonSharpHideMember("resetScriptTransitionClock")]
    public class UI {
        public static Dictionary<string, Action> restartScriptTransitionClock;
        public static Dictionary<string,
                Func<float, (Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>, bool)>> scriptTransitions;
        public static Dictionary<string, Dictionary<string, DynValue>> transitions {
            set {
                if(value == null) {
                    scriptTransitions = null;
                    return;
                }
                
                Dictionary<string, Transition> transes = new Dictionary<string, Transition>(value.Count);
                foreach((string name, Dictionary<string, DynValue> transition) in value) {
                    Transition trans = new Transition();
                    foreach((string key, DynValue dynValue) in transition)
                        switch(key) {
                            case "x": trans.x = new Expression(dynValue.String).ToLambda<TransExCtx, int>();
                                continue;
                            case "y": trans.y = new Expression(dynValue.String).ToLambda<TransExCtx, int>();
                                continue;
                            case "character":
                                trans.character = new Expression(dynValue.String).ToLambda<TransExCtx, string>();
                                continue;
                            case "finished":
                                trans.finished = new Expression(dynValue.String).ToLambda<TransExCtx, bool>();
                                continue;
                            default: {
                                if(dynValue.Type == DataType.Table)
                                    foreach(TablePair pair in dynValue.Table.Pairs) {
                                        Func<TransExCtx, byte> exp =
                                            new Expression(pair.Value.String).ToLambda<TransExCtx, byte>();
                                        switch(key) {
                                            case "background":
                                                switch(pair.Key.Number) {
                                                    case 1: trans.bgR = exp;
                                                        continue;
                                                    case 2: trans.bgG = exp;
                                                        continue;
                                                    case 3: trans.bgB = exp;
                                                        continue;
                                                    case 4: trans.bgA = exp;
                                                        continue;
                                                }
                                                break;
                                            case "foreground":
                                                switch(pair.Key.Number) {
                                                    case 1: trans.fgR = exp;
                                                        continue;
                                                    case 2: trans.fgG = exp;
                                                        continue;
                                                    case 3: trans.fgB = exp;
                                                        continue;
                                                    case 4: trans.fgA = exp;
                                                        continue;
                                                }
                                                break;
                                        }
                                    }
                                break;
                            }
                        }
                    trans.clock = new Clock();
                    transes.Add(name, trans);
                }

                // this is rly junk but whatever lmao
                restartScriptTransitionClock = new Dictionary<string, Action>();
                scriptTransitions = new Dictionary<string,
                    Func<float, (Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>, bool)>>();
                foreach((string name, Transition transition) in transes) {
                    restartScriptTransitionClock.Add(name, () => { transition.clock.Restart(); });
                    scriptTransitions.Add(name, speed => {
                        TransExCtx context = new TransExCtx();
                        return ((pos, character) => {
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
                            context.time = transition.clock.ElapsedTime.AsSeconds() * speed;
                            Vector2i modPos = pos;
                            RenderCharacter modChar = character;
                            modPos = new Vector2i(transition.x?.Invoke(context) ?? modPos.X,
                                transition.y?.Invoke(context) ?? modPos.Y);
                            modChar = new RenderCharacter(transition.character?.Invoke(context)[0] ?? modChar.character,
                                new Color(transition.bgR?.Invoke(context) ?? modChar.background.R,
                                    transition.bgG?.Invoke(context) ?? modChar.background.G,
                                    transition.bgB?.Invoke(context) ?? modChar.background.B,
                                    transition.bgA?.Invoke(context) ?? modChar.background.A),
                                new Color(transition.fgR?.Invoke(context) ?? modChar.foreground.R,
                                    transition.fgG?.Invoke(context) ?? modChar.foreground.G,
                                    transition.fgB?.Invoke(context) ?? modChar.foreground.B,
                                    transition.fgA?.Invoke(context) ?? modChar.foreground.A));
                            return (modPos, modChar);
                        }, transition.finished(context));
                    });
                }
            }
        }

        public static List<string> currentLayouts => PPR.GUI.UI.currentLayouts.ToList();

        public static string currentSelectedLevel {
            get => PPR.GUI.UI.currSelectedLevel;
            set => PPR.GUI.UI.currSelectedLevel = value;
        }

        public static string currentSelectedDiff {
            get => PPR.GUI.UI.currSelectedDiff;
            set => PPR.GUI.UI.currSelectedDiff = value;
        }

        public static void TransitionLayouts(string previous, string next,
            string fadeOutTransition, string fadeInTransition,
            float fadeOutSpeed, float fadeInSpeed) => PPR.GUI.UI.TransitionLayouts(previous, next,
            fadeOutTransition, fadeInTransition, fadeOutSpeed, fadeInSpeed);

        public static void SetElementText(string id, string text) {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach(string layout in PPR.GUI.UI.currentLayouts) {
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach((string _, UIElement element) in PPR.GUI.UI.layouts[layout].elements) {
                    if(element.id == id) {
                        switch(element) {
                            case Button button: button.text = text;
                                break;
                            case Text textElement: textElement.text = text;
                                break;
                        }
                    }
                }
            }
        }

        public static void SetUniqueElementText(string layout, string uid, string text) {
            if(!PPR.GUI.UI.layouts[layout].elements.TryGetValue(uid, out UIElement element)) return;
            switch(element) {
                case Button button: button.text = text;
                    break;
                case Text textElement: textElement.text = text;
                    break;
            }
        }

        public static string GetPreviousMenuForLayout(string layout) => layout switch {
            "game" => "lastStats",
            "lastStats" => "levelSelect",
            "keybinds" => "settings",
            _ => "mainMenu"
        };

        public static void CreatePanel(string layout, string uid, string id, int x, int y, int width, int height,
            float anchorX, float anchorY, string parent) {
            if(!PPR.GUI.UI.layouts.TryGetValue(layout, out Layout useLayout))
                throw new ArgumentException($"Layout {layout} doesn't exist.");
            UIElement useParent = null;
            if(!string.IsNullOrWhiteSpace(parent) && !useLayout.elements.TryGetValue(parent, out useParent))
                throw new ArgumentException($"Element {parent} doesn't exist in {layout}.");
            
            useLayout.elements.TryRemove(uid, out _);
            useLayout.elements.TryAdd(uid,
                new Panel(uid, id, new Vector2i(x, y), new Vector2i(width, height), new Vector2f(anchorX, anchorY),
                    useParent));
            useLayout.RegisterElementEvents(uid);
        }

        public static void CreateText(string layout, string uid, string id, int x, int y, float anchorX, float anchorY,
            string parent, string text, Renderer.Alignment align, bool replacingSpaces, bool invertOnDarkBackground) {
            if(!PPR.GUI.UI.layouts.TryGetValue(layout, out Layout useLayout))
                throw new ArgumentException($"Layout {layout} doesn't exist.");
            UIElement useParent = null;
            if(!string.IsNullOrWhiteSpace(parent) && !useLayout.elements.TryGetValue(parent, out useParent))
                throw new ArgumentException($"Element {parent} doesn't exist in {layout}.");

            useLayout.elements.TryRemove(uid, out _);
            useLayout.elements.TryAdd(uid,
                new Text(uid, id, new Vector2i(x, y), new Vector2f(anchorX, anchorY),
                    useParent, text, align, replacingSpaces, invertOnDarkBackground));
            useLayout.RegisterElementEvents(uid);
        }

        public static void CreateButton(string layout, string uid, string id, int x, int y, int width,
            float anchorX, float anchorY, string parent, string text, Renderer.Alignment align) {
            if(!PPR.GUI.UI.layouts.TryGetValue(layout, out Layout useLayout))
                throw new ArgumentException($"Layout {layout} doesn't exist.");
            UIElement useParent = null;
            if(!string.IsNullOrWhiteSpace(parent) && !useLayout.elements.TryGetValue(parent, out useParent))
                throw new ArgumentException($"Element {parent} doesn't exist in {layout}.");
            
            useLayout.elements.TryRemove(uid, out _);
            useLayout.elements.TryAdd(uid,
                new Button(uid, id, new Vector2i(x, y), width, new Vector2f(anchorX, anchorY), useParent, text, null,
                    align));
            useLayout.RegisterElementEvents(uid);
        }

        public static void SetButtonSelected(string id, bool selected) {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach(string layout in PPR.GUI.UI.currentLayouts) {
                foreach((string _, UIElement element) in PPR.GUI.UI.layouts[layout].elements) {
                    if(element.id == id && element is Button button) button.selected = selected;
                }
            }
        }

        public static void SetUniqueButtonSelected(string layout, string uid, bool selected) {
            if(!PPR.GUI.UI.layouts.TryGetValue(layout, out Layout useLayout))
                throw new ArgumentException($"Layout {layout} doesn't exist.");
            if(!useLayout.elements.TryGetValue(uid, out UIElement element))
                throw new ArgumentException($"Element {uid} doesn't exist in {layout}.");
            if(!(element is Button button))
                throw new ArgumentException($"Element {uid} is not a button.");

            button.selected = selected;
        }

        public static void SetElementEnabled(string id, bool enabled) {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach(string layout in PPR.GUI.UI.currentLayouts) {
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach((string _, UIElement element) in PPR.GUI.UI.layouts[layout].elements) {
                    if(element.id == id) element.enabled = enabled;
                }
            }
        }

        public static void SetUniqueElementEnabled(string layout, string uid, bool enabled) {
            if(!PPR.GUI.UI.layouts.TryGetValue(layout, out Layout useLayout))
                throw new ArgumentException($"Layout {layout} doesn't exist.");
            if(!useLayout.elements.TryGetValue(uid, out UIElement element))
                throw new ArgumentException($"Element {uid} doesn't exist in {layout}.");

            element.enabled = enabled;
        }

        public static string GetLevelNameFromButton(string layout, string uid) {
            if(!PPR.GUI.UI.layouts.TryGetValue(layout, out Layout useLayout))
                throw new ArgumentException($"Layout {layout} doesn't exist.");
            if(!useLayout.elements.TryGetValue(uid, out UIElement element))
                throw new ArgumentException($"Element {uid} doesn't exist in {layout}.");
            if(!(element is Button button))
                throw new ArgumentException($"Element {uid} is not a button.");

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach((string levelName, LevelSelectLevel level) in PPR.GUI.UI.levelSelectLevels)
                if(level.button == button)
                    return levelName;

            return null;
        }

        public static (string, string) GetLevelAndDiffNamesFromButton(string layout, string uid) {
            if(!PPR.GUI.UI.layouts.TryGetValue(layout, out Layout useLayout))
                throw new ArgumentException($"Layout {layout} doesn't exist.");
            if(!useLayout.elements.TryGetValue(uid, out UIElement element))
                throw new ArgumentException($"Element {uid} doesn't exist in {layout}.");
            if(!(element is Button button))
                throw new ArgumentException($"Element {uid} is not a button.");

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach((string levelName, LevelSelectLevel level) in PPR.GUI.UI.levelSelectLevels) {
                foreach((string diffName, LevelSelectDiff diff) in level.diffs)
                    if(diff.button == button)
                        return (levelName, diffName);

                return (levelName, null);
            }

            return (null, null);
        }

        public static (string, string, string, string, bool, string, string)
            GetLevelMetadata(string levelName, string diffName) {
            if(!PPR.GUI.UI.levelSelectLevels.TryGetValue(levelName, out LevelSelectLevel level))
                throw new ArgumentException($"Level {levelName} doesn't exist.");
            if(!level.diffs.TryGetValue(levelName, out LevelSelectDiff diff))
                throw new ArgumentException($"Difficulty {diff} doesn't exist in level {levelName}.");

            bool lua = File.Exists(Path.Join("levels", levelName, "script.lua")) ||
                       File.Exists(Path.Join("levels", levelName, $"{diffName}.lua"));
            return (diff.metadata.length, diff.metadata.displayDifficulty, diff.metadata.bpm, diff.metadata.author, lua,
                diff.metadata.objectCount.ToString(), diff.metadata.speedsCount.ToString());
        }

        public static List<(string, string, string, string[])> GetLevelScores(string levelName, string diffName) {
            if(!PPR.GUI.UI.levelSelectLevels.TryGetValue(levelName, out LevelSelectLevel level))
                throw new ArgumentException($"Level {levelName} doesn't exist.");
            if(!level.diffs.TryGetValue(levelName, out LevelSelectDiff diff))
                throw new ArgumentException($"Difficulty {diff} doesn't exist in level {levelName}.");

            return diff.scores.Select(score => (score.scoreStr, score.accuracyStr, score.maxComboStr,
                score.scores.Select(sc => sc.ToString()).ToArray())).ToList();
        }
    }
}
