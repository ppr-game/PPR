using System;
using System.Collections.Generic;

using MoonSharp.Interpreter;

using NLog;

using PPR.Lua.API.Console.Main;
using PPR.Lua.API.Console.Main.Managers;
using PPR.Lua.API.Scripts.IO;
using PPR.Lua.API.Scripts.Main.Levels;
using PPR.UI;
using PPR.UI.Animations;

using PRR;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;

namespace PPR.Lua {
    public static class Manager {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        public static Script script;
        private static Closure _update;
        private static Closure _tick;
        private static Closure _drawMap;
        private static Closure _drawUI;

        public static Script console;
        public static List<Script> consoles = new List<Script>();
        public static Dictionary<(object, string), List<Closure>> events = new Dictionary<(object, string), List<Closure>>();
        
        public static void ScriptSetup() {
            Script.WarmUp();
                    
            UserData.RegisterType<API.Scripts.Core>();
            UserData.RegisterType<API.Scripts.Main.Game>();
            
            UserData.RegisterType<Game>();
            UserData.RegisterType<Helper>();
            UserData.RegisterType<API.Console.UI.UI>();
            
            UserData.RegisterType<UI.Elements.Button>();
            UserData.RegisterType<UI.Elements.FilledPanel>();
            UserData.RegisterType<UI.Elements.Mask>();
            UserData.RegisterType<UI.Elements.Panel>();
            UserData.RegisterType<UI.Elements.ProgressBar>();
            UserData.RegisterType<UI.Elements.Slider>();
            UserData.RegisterType<UI.Elements.Text>();
                
            UserData.RegisterType<Animation>();
            
            UserData.RegisterType<SoundManager>();
            UserData.RegisterType<ScoreManager>();
            
            UserData.RegisterType<Level>();
            UserData.RegisterType<Map>();
            UserData.RegisterType<API.Scripts.Rendering.Renderer>();
            UserData.RegisterType<API.Console.Rendering.Renderer>();
            
            UserData.RegisterType<File>();

            UserData.RegisterType<Renderer.Alignment>();
            UserData.RegisterType<Vector2i>();
            UserData.RegisterType<Vector2f>();
            UserData.RegisterType<Color>();
            UserData.RegisterType<RenderCharacter>();
            UserData.RegisterType<SoundStatus>();
            UserData.RegisterType<Bounds>();
        }
        
        public static void ClearScript() {
            script = null;
            _update = null;
            _tick = null;
            _drawMap = null;
            _drawUI = null;
            API.Scripts.Rendering.Renderer.scriptBackgroundModifier = null;
            API.Scripts.Rendering.Renderer.scriptCharactersModifier = null;
        }
        
        public static void TryLoadScript(string path) {
            try {
                script = new Script(CoreModules.Preset_SoftSandbox);

                DynValue core = UserData.Create(new API.Scripts.Core());
                DynValue game = UserData.Create(new API.Scripts.Main.Game());
                DynValue level = UserData.Create(new Level());
                DynValue map = UserData.Create(new Map());
                DynValue renderer = UserData.Create(new API.Scripts.Rendering.Renderer());
                DynValue file = UserData.Create(new File());
                
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
                    logger.Info(message);
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

        public static void InitializeConsole() {
            console = new Script(CoreModules.Preset_SoftSandbox);
            InitializeConsole(console);
        }
        
        public static void InitializeConsole(Script script) {
            DynValue core = UserData.Create(new API.Scripts.Core());
            DynValue game = UserData.Create(new Game());
            DynValue helper = UserData.Create(new Helper());
            DynValue ui = UserData.Create(new API.Console.UI.UI());
            DynValue soundManager = UserData.Create(new SoundManager());
            DynValue scoreManager = UserData.Create(new ScoreManager());
            DynValue level = UserData.Create(new Level());
            DynValue map = UserData.Create(new Map());
            DynValue renderer = UserData.Create(new API.Console.Rendering.Renderer());
            DynValue file = UserData.Create(new File());
            
            script.Globals["core"] = core;
            script.Globals["game"] = game;
            script.Globals["helper"] = helper;
            script.Globals["ui"] = ui;
            script.Globals["soundManager"] = soundManager;
            script.Globals["scoreManager"] = scoreManager;
            script.Globals["level"] = level;
            script.Globals["map"] = map;
            script.Globals["renderer"] = renderer;
            script.Globals["file"] = file;

            script.Globals["alignment"] = UserData.CreateStatic<Renderer.Alignment>();
            script.Globals["soundStatus"] = UserData.CreateStatic<SoundStatus>();

            script.Options.DebugPrint = message => {
                Console.WriteLine(message);
                logger.Info(message);
            };
            
            script.Globals["vector2i"] = (Func<int, int, Vector2i>)((x, y) => new Vector2i(x, y));
            script.Globals["vector2f"] = (Func<float, float, Vector2f>)((x, y) => new Vector2f(x, y));
            
            /*script.Globals["rgb"] = (Func<byte, byte, byte, Color>)((r, g, b) => new Color(r, g, b));
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
            script.Globals["transparent"] = Color.Transparent;*/
            
            script.Globals["renderCharacter"] =
                (Func<char, Color, Color, RenderCharacter>)((character, background, foreground) =>
                    new RenderCharacter(character, background, foreground));
            
            consoles.Add(script);
        }
        
        public static void ExecuteCommand(string command) => console.DoString(command);
        
        public static void InvokeEvent(object caller, string name) {
            if(!events.TryGetValue((caller, name), out List<Closure> closures)) return;

            foreach(Closure closure in closures) closure.Call();
        }
        
        public static void InvokeEvent(object caller, string name, params object[] args) {
            if(!events.TryGetValue((caller, name), out List<Closure> closures)) return;

            foreach(Closure closure in new List<Closure>(closures)) closure.Call(args);
        }

        public static void SubscribeEvent(object caller, string name, Closure closure) {
            if(events.TryGetValue((caller, name), out List<Closure> closures)) closures.Add(closure);
            else events.Add((caller, name), new List<Closure> { closure });
        }

        public static void UnsubscribeAllEvents(Script script) {
            if(script == null) return;
            
            foreach(((object, string) _, List<Closure> closures) in events)
                foreach(Closure closure in new List<Closure>(closures))
                    if(closure.OwnerScript == script)
                        closures.Remove(closure);
        }
    }
}
