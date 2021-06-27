using System.Globalization;

using PER.Abstractions;
using PER.Abstractions.Renderer;
using PER.Util;

namespace PER.Demo {
    public class Game : IGame {
        public void Setup() { }

        public void Loop() {
            Core.engine.renderer.DrawText(new Vector2Int(0, 0),
                $"{(1d / Core.engine.deltaTime).ToString(CultureInfo.InvariantCulture)} FPS", 
                Color.white, Color.transparent);
            
            Core.engine.renderer.DrawText(new Vector2Int(0, 1),
                "hello everyone! this is \fcf00FF00FFcb\fConfiG\fcfFFFFFFFFcb\f and today i'm gonna show you my engine!!\nas you can see \fbiu\fit works!!1!\fbiu\f\n\fcf000000FFbFF0000FFc\fthanks for watching\fcfFFFFFFFFb00000000c\f everyone, \fu\fhit like, subscribe, good luck, \fb\fbye!!".Split('\n'),
                Color.white, Color.transparent);
            
            Core.engine.renderer.DrawText(new Vector2Int(0, 4),
                "more test", Color.black, new Color(0f, 1f, 0f, 1f));
            
            for(RenderStyle style = RenderStyle.None; style <= RenderStyle.All; style++) {
                Core.engine.renderer.DrawText(new Vector2Int(0, 5 + (int)style),
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
        
        public void Stop() { }
    }
}
