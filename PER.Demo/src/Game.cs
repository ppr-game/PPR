using System;
using System.Globalization;

using PER.Abstractions;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Demo.Effects;
using PER.Util;

namespace PER.Demo {
    public class Game : IGame {
        private int _fps;
        private int _avgFPS;
        private int _tempAvgFPS;
        private int _tempAvgFPSCounter;

        private readonly DrawTextEffect _drawTextEffect = new();
        private readonly BloomEffect _bloomEffect = new();
        private readonly GlitchEffect _glitchEffect = new();

        public void Setup() {
            Core.engine.renderer.formattingEffects.Add("NONE", null);
            Core.engine.renderer.formattingEffects.Add("GLITCH", _glitchEffect);
        }

        public void Update() {
            _fps = (int)Math.Round(1d / Core.engine.deltaTime);
            _tempAvgFPS += _fps;
            _tempAvgFPSCounter++;
            if(_tempAvgFPSCounter >= _avgFPS) {
                _avgFPS = _tempAvgFPS / _tempAvgFPSCounter;
                _tempAvgFPS = 0;
                _tempAvgFPSCounter = 0;
            }
            
            Core.engine.renderer.AddEffect(_drawTextEffect);
            Core.engine.renderer.AddEffect(_bloomEffect);
            
            Core.engine.renderer.DrawText(new Vector2Int(0, 0),
                $"{_fps.ToString(CultureInfo.InvariantCulture)}/{_avgFPS.ToString(CultureInfo.InvariantCulture)} FPS", 
                Color.white, Color.transparent);

            if(Core.engine.renderer.input.KeyPressed(KeyCode.F)) return;
            
            Core.engine.renderer.DrawText(new Vector2Int(0, 1),
                @"hello everyone! this is cf00FF00FFcbConfiGcfFFFFFFFFcb and today i'm gonna show you my eGLITCHeengineeNONEe!!
as you can see biuit works!!1!biu
cf000000FFbFF0000FFcthanks for watchingcfFFFFFFFFb00000000c everyone, uhit like, subscribe, good luck, bbye!!".Split('\n'),
                Color.white, Color.transparent);
            
            Core.engine.renderer.DrawText(new Vector2Int(0, 4),
                "more test", Color.black, new Color(0f, 1f, 0f, 1f));
            
            Core.engine.renderer.DrawText(new Vector2Int(0, 5),
                "ieven morei test", new Color(1f, 0f, 1f, 0.5f), new Color(0f, 1f, 0f, 1f));
            
            Core.engine.renderer.DrawText(new Vector2Int(10, 4),
                "per-text effects test", Color.white, Color.transparent, HorizontalAlignment.Left, RenderStyle.None,
                RenderOptions.Default, _glitchEffect);
            
            for(RenderStyle style = RenderStyle.None; style <= RenderStyle.All; style++) {
                Core.engine.renderer.DrawText(new Vector2Int(0, 6 + (int)style),
                    "styles test", Color.white, Color.transparent, HorizontalAlignment.Left, style);
            }
            
            Core.engine.renderer.DrawText(new Vector2Int(39, 6),
                "left test even", Color.white, Color.transparent);
            Core.engine.renderer.DrawText(new Vector2Int(39, 7),
                "left test odd", Color.white, Color.transparent);
            Core.engine.renderer.DrawText(new Vector2Int(39, 8),
                "middle test even", Color.white, Color.transparent, HorizontalAlignment.Middle);
            Core.engine.renderer.DrawText(new Vector2Int(39, 9),
                "middle test odd", Color.white, Color.transparent, HorizontalAlignment.Middle);
            Core.engine.renderer.DrawText(new Vector2Int(39, 10),
                "right testeven", Color.white, Color.transparent, HorizontalAlignment.Right);
            Core.engine.renderer.DrawText(new Vector2Int(39, 11),
                "right testodd", Color.white, Color.transparent, HorizontalAlignment.Right);
        }
        
        public void Tick() { }
        
        public void Finish() { }
    }
}
