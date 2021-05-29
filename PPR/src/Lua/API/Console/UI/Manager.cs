using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using MoonSharp.Interpreter;

using NCalc;

using PPR.Main.Levels;
using PPR.Properties;
using PPR.UI;
using PPR.UI.Animations;
using PPR.UI.Elements;

using PRR;

using SFML.Graphics;
using SFML.System;

using Renderer = PRR.Renderer;
using Text = PPR.UI.Elements.Text;

namespace PPR.Lua.API.Console.UI {
    public class UI {
        public static string currentSelectedLevel {
            get => PPR.UI.Manager.currSelectedLevel;
            set => PPR.UI.Manager.currSelectedLevel = value;
        }

        public static string currentSelectedDiff {
            get => PPR.UI.Manager.currSelectedDiff;
            set => PPR.UI.Manager.currSelectedDiff = value;
        }

        public static void Reload() {
            Bindings.Reload();
            ColorScheme.Reload();
        }

        public static void SetAnimationValue(string name, double value) => AnimationContext.customVars[name] = value;

        public static PPR.UI.Animations.Animation AnimateElement(string id, string animation, float time, bool endState, Closure endCallback,
            Dictionary<string, double> args) {
            Element element = null;
            if(id != null && !PPR.UI.Manager.currentLayout.elements.TryGetValue(id, out element))
                throw new ArgumentException($"Element {id} doesn't exist.");

            return PPR.UI.Manager.AnimateElement(element, animation, time, endState, endCallback, args);
        }

        public static bool StopElementAnimation(string id, PPR.UI.Animations.Animation animation) {
            Element element = null;
            if(id != null && !PPR.UI.Manager.currentLayout.elements.TryGetValue(id, out element))
                throw new ArgumentException($"Element {id} doesn't exist.");

            return PPR.UI.Manager.StopElementAnimation(element, animation);
        }

        public static bool StopElementAnimations(string id, string animation) {
            Element element = null;
            if(id != null && !PPR.UI.Manager.currentLayout.elements.TryGetValue(id, out element))
                throw new ArgumentException($"Element {id} doesn't exist.");

            return PPR.UI.Manager.StopElementAnimations(element, animation);
        }

        public static Element GetElement(string id) {
            Element element = null;
            if(id != null && !PPR.UI.Manager.currentLayout.elements.TryGetValue(id, out element))
                throw new ArgumentException($"Element {id} doesn't exist.");

            return element;
        }

        public static List<Element> GetElements(string tag) => PPR.UI.Manager.currentLayout.elements.Values
            .Where(elem => elem.tags.Contains(tag)).ToList();

        public static string GetPreviousMenu(string currentMenu) => currentMenu switch {
            "game" => "lastStats",
            "lastStats" => "levelSelect",
            "keybinds" => "settings",
            _ => "mainMenu"
        };

        public static Panel CreatePanel(string id, List<string> tags, int x, int y, int width, int height,
            float anchorX, float anchorY, string parent) {
            Element useParent = null;
            if(!string.IsNullOrWhiteSpace(parent) &&
               !PPR.UI.Manager.currentLayout.elements.TryGetValue(parent, out useParent))
                throw new ArgumentException($"Element {parent} doesn't exist.");

            Panel newPanel = new Panel(id, tags, new Vector2i(x, y), new Vector2i(width, height),
                new Vector2f(anchorX, anchorY), useParent);
            PPR.UI.Manager.currentLayout.AddElement(id, newPanel);
            return newPanel;
        }

        public static Mask CreateMask(string id, List<string> tags, int x, int y, int width, int height,
            float anchorX, float anchorY, string parent) {
            Element useParent = null;
            if(!string.IsNullOrWhiteSpace(parent) &&
               !PPR.UI.Manager.currentLayout.elements.TryGetValue(parent, out useParent))
                throw new ArgumentException($"Element {parent} doesn't exist.");

            Mask newMask = new Mask(id, tags, new Vector2i(x, y), new Vector2i(width, height),
                new Vector2f(anchorX, anchorY), useParent);
            PPR.UI.Manager.currentLayout.AddElement(id, newMask);
            return newMask;
        }

        public static Panel CreateFilledPanel(string id, List<string> tags, int x, int y, int width, int height,
            float anchorX, float anchorY, string parent) {
            Element useParent = null;
            if(!string.IsNullOrWhiteSpace(parent) &&
               !PPR.UI.Manager.currentLayout.elements.TryGetValue(parent, out useParent))
                throw new ArgumentException($"Element {parent} doesn't exist.");

            FilledPanel newFilledPanel = new FilledPanel(id, tags, new Vector2i(x, y), new Vector2i(width, height),
                    new Vector2f(anchorX, anchorY), useParent);
            PPR.UI.Manager.currentLayout.AddElement(id, newFilledPanel);
            return newFilledPanel;
        }

        public static Text CreateText(string id, List<string> tags, int x, int y, float anchorX, float anchorY,
            string parent, string text, Renderer.Alignment align, bool replacingSpaces, bool invertOnDarkBackground) {
            Element useParent = null;
            if(!string.IsNullOrWhiteSpace(parent) &&
               !PPR.UI.Manager.currentLayout.elements.TryGetValue(parent, out useParent))
                throw new ArgumentException($"Element {parent} doesn't exist.");

            Text newText = new Text(id, tags, new Vector2i(x, y), new Vector2f(anchorX, anchorY), useParent, text,
                align, replacingSpaces, invertOnDarkBackground);
            PPR.UI.Manager.currentLayout.AddElement(id, newText);
            return newText;
        }

        public static Button CreateButton(string id, List<string> tags, int x, int y, int width,
            float anchorX, float anchorY, string parent, string text, Renderer.Alignment align) {
            Element useParent = null;
            if(!string.IsNullOrWhiteSpace(parent) &&
               !PPR.UI.Manager.currentLayout.elements.TryGetValue(parent, out useParent))
                throw new ArgumentException($"Element {parent} doesn't exist.");

            Button newButton = new Button(id, tags, new Vector2i(x, y), width, new Vector2f(anchorX, anchorY),
                useParent, text, null, align);
            PPR.UI.Manager.currentLayout.AddElement(id, newButton);
            return newButton;
        }

        public static void DeleteElement(string id) => PPR.UI.Manager.currentLayout.RemoveElement(id);
        public static void DeleteElementWithIndex(int index) => PPR.UI.Manager.currentLayout.RemoveElement(index);

        public static bool ElementExists(string id) => PPR.UI.Manager.currentLayout.elements.ContainsKey(id);

        public static string GetLevelNameFromButton(string id) {
            if(!PPR.UI.Manager.currentLayout.elements.TryGetValue(id, out Element element))
                throw new ArgumentException($"Element {id} doesn't exist.");
            if(!(element is Button button))
                throw new ArgumentException($"Element {id} is not a button.");

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach((string levelName, LevelSelectLevel level) in PPR.UI.Manager.levelSelectLevels)
                if(level.button == button)
                    return levelName;

            return null;
        }

        public static DynValue GetLevelAndDiffNamesFromButton(string id) {
            if(!PPR.UI.Manager.currentLayout.elements.TryGetValue(id, out Element element))
                throw new ArgumentException($"Element {id} doesn't exist.");
            if(!(element is Button button))
                throw new ArgumentException($"Element {id} is not a button.");

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach((string levelName, LevelSelectLevel level) in PPR.UI.Manager.levelSelectLevels) {
                foreach((string diffName, LevelSelectDiff diff) in level.diffs)
                    if(diff.button == button)
                        return DynValue.NewTuple(DynValue.NewString(levelName), DynValue.NewString(diffName));
            }

            return DynValue.NewTuple(DynValue.Nil, DynValue.Nil);
        }

        public static DynValue GetLevelMetadata(string levelName, string diffName) {
            if(!PPR.UI.Manager.levelSelectLevels.TryGetValue(levelName, out LevelSelectLevel level))
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
            if(!PPR.UI.Manager.levelSelectLevels.TryGetValue(levelName, out LevelSelectLevel level))
                throw new ArgumentException($"Level {levelName} doesn't exist.");
            if(!level.diffs.TryGetValue(diffName, out LevelSelectDiff diff))
                throw new ArgumentException($"Difficulty {diffName} doesn't exist in level {levelName}.");

            return diff.scores.Select(score => DynValue.NewTable(script, DynValue.NewNumber(score.score),
                DynValue.NewNumber(score.accuracy), DynValue.NewNumber(score.maxCombo),
                DynValue.FromObject(script, score.scores))).ToList();
        }
    }
}
