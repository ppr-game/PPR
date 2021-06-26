using PER.Abstractions.Renderer;

using PRR;

namespace PER.Demo {
    public static class Core {
        public static Engine engine { get; private set; }
        
        private static void Main(string[] args) {
            engine = new Engine { game = new Game(), renderer = new Renderer(), tickInterval = 0.02d };
            engine.Start(new RendererSettings {
                title = "PER Demo Pog",
                width = 80,
                height = 60,
                framerate = 0,
                fullscreen = false,
                font = "resources",
                icon = null
            });
        }
    }
}
