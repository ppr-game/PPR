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
                @"hello everyone! this is ConfiG and today i'm gonna show you my engine!!
as you can see it works!!1!
thanks for watching everyone, hit like, subscribe, good luck, bye!!".Split('\n'),
                new Color(0f, 1f, 0f, 1f), Color.transparent);
            
            Core.engine.renderer.DrawText(new Vector2Int(0, 4),
                "more test", Color.black, new Color(0f, 1f, 0f, 1f));
            
            for(RenderStyle style = RenderStyle.None; style <= RenderStyle.All; style++) {
                Core.engine.renderer.DrawText(new Vector2Int(0, 5 + (int)style),
                    "styles test", Color.white, Color.transparent, HorizontalAlignment.Left, style);
            }
        }
        
        public void Tick() { }
        
        public void Stop() { }
    }
}
