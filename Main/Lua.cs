using System;

using MoonSharp.Interpreter;

using NLog;

using PRR;

using SFML.Graphics;
using SFML.System;

namespace PPR.Main {
    public static class Lua {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        public static Script script;
        private static Closure _update;
        private static Closure _tick;
        private static Closure _drawMap;
        private static Closure _drawUI;
        
        public static void ScriptSetup() {
            Script.WarmUp();
                    
            UserData.RegisterType<Scripts.Core>();
            UserData.RegisterType<Scripts.Main.Game>();
            UserData.RegisterType<Scripts.Main.Levels.Level>();
            UserData.RegisterType<Scripts.Main.Levels.Map>();
            UserData.RegisterType<Scripts.Rendering.Renderer>();
            UserData.RegisterType<Scripts.IO.File>();

            UserData.RegisterType<Renderer.Alignment>();
            UserData.RegisterType<Vector2i>();
            UserData.RegisterType<Vector2f>();
            UserData.RegisterType<Color>();
            UserData.RegisterType<RenderCharacter>();
        }
        
        public static void ClearScript() {
            script = null;
            _update = null;
            _tick = null;
            _drawMap = null;
            _drawUI = null;
            Scripts.Rendering.Renderer.scriptCharactersModifier = null;
        }
        
        public static void TryLoadScript(string path) {
            try {
                script = new Script(CoreModules.Preset_SoftSandbox);

                DynValue core = UserData.Create(new Scripts.Core());
                DynValue game = UserData.Create(new Scripts.Main.Game());
                DynValue level = UserData.Create(new Scripts.Main.Levels.Level());
                DynValue map = UserData.Create(new Scripts.Main.Levels.Map());
                DynValue renderer = UserData.Create(new Scripts.Rendering.Renderer());
                DynValue file = UserData.Create(new Scripts.IO.File());
                
                script.Globals["core"] = core;
                script.Globals["game"] = game;
                script.Globals["level"] = level;
                script.Globals["map"] = map;
                script.Globals["renderer"] = renderer;
                script.Globals["file"] = file;

                script.Globals["alignment"] = UserData.CreateStatic<Renderer.Alignment>();
                
                script.Globals["vector2i"] = (Func<int, int, Vector2i>)((x, y) => new Vector2i(x, y));
                script.Globals["vector2f"] = (Func<float, float, Vector2f>)((x, y) => new Vector2f(x, y));
                
                script.Globals["rgb"] = (Func<byte, byte, byte, Color>)((r, g, b) => new Color(r, g, b));
                script.Globals["rgba"] =
                    (Func<byte, byte, byte, byte, Color>)((r, g, b, a) => new Color(r, g, b, a));
                script.Globals["black"] = Color.Black;
                script.Globals["white"] = Color.White;
                script.Globals["red"] = Color.Red;
                script.Globals["green"] = Color.Green;
                script.Globals["blue"] = Color.Blue;
                script.Globals["yellow"] = Color.Yellow;
                script.Globals["magenta"] = Color.Magenta;
                script.Globals["cyan"] = Color.Cyan;
                script.Globals["transparent"] = Color.Transparent;
                
                script.Globals["renderCharacter"] =
                    (Func<char, Color, Color, RenderCharacter>)((character, background, foreground) =>
                        new RenderCharacter(character, background, foreground));

                script.Options.DebugPrint = message => {
                    Console.WriteLine(message);
                    logger.Debug(message);
                };
                
                script.DoFile(path);

                _update = DynValue.FromObject(script, script.Globals["update"]).Function;
                _tick = DynValue.FromObject(script, script.Globals["tick"]).Function;
                _drawMap = DynValue.FromObject(script, script.Globals["drawMap"]).Function;
                _drawUI = DynValue.FromObject(script, script.Globals["drawUI"]).Function;
            }
            catch(InterpreterException ex) {
                logger.Error(ex.DecoratedMessage);
            }
        }

        public static void Update() {
            if(_update == null) return;
            try {
                _update.Call();
            }
            catch(InterpreterException ex) {
                logger.Error(ex.DecoratedMessage);
            }
        }
        public static void Tick() {
            if(_tick == null) return;
            try {
                _tick.Call();
            }
            catch(InterpreterException ex) {
                logger.Error(ex.DecoratedMessage);
            }
        }
        public static void DrawMap() {
            if(_drawMap == null) return;
            try {
                _drawMap.Call();
            }
            catch(InterpreterException ex) {
                logger.Error(ex.DecoratedMessage);
            }
        }
        public static void DrawUI() {
            if(_drawUI == null) return;
            try {
                _drawUI.Call();
            }
            catch(InterpreterException ex) {
                logger.Error(ex.DecoratedMessage);
            }
        }
    }
}
