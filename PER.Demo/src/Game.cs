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
                @"hello everyone! this is cf00FF00FFcbConfiGcfFFFFFFFFcb and today i'm gonna show you my engine!!
as you can see biuit works!!1!biu
cf000000FFbFF0000FFcthanks for watchingcfFFFFFFFFb00000000c everyone, uhit like, subscribe, good luck, bbye!!".Split('\n'),
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
