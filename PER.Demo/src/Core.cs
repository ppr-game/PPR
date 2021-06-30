using PER.Abstractions.Renderer;

using PRR;
using PRR.Sfml;

namespace PER.Demo {
    public static class Core {
        public static Engine engine { get; private set; }
        
        private static void Main() {
            engine = new Engine { game = new Game(), renderer = new Renderer(), tickInterval = 0.02d };
            engine.Start(new RendererSettings {
                title = "PER Demo Pog",
                width = 80,
                height = 60,
                framerate = 0,
                fullscreen = false,
                font = new Font("resources"),
                icon = null
            });
        }
    }
}
