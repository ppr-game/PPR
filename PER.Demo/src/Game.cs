using System.Globalization;

using PER.Abstractions;
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
        }
        
        public void Tick() { }
        
        public void Stop() { }
    }
}
