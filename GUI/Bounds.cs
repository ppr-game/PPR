using SFML.System;

namespace PPR.GUI {
    public readonly struct Bounds {
        public readonly Vector2i min;
        public readonly Vector2i max;
        
        public Bounds(Vector2i min, Vector2i max) {
            this.min = min;
            this.max = max;
        }
    }
}
