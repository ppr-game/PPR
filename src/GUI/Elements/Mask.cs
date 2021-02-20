using System;
using System.Collections.Generic;

using MoonSharp.Interpreter;

using PPR.Main;

using PRR;

using SFML.System;
using SFML.Window;

namespace PPR.GUI.Elements {
    public sealed class OnScrollEventArgs : EventArgs {
        public string id { get; }
        public float delta { get; set; }

        public OnScrollEventArgs(string id, float delta) {
            this.id = id;
            this.delta = delta;
        }
    }
    
    public class Mask : Panel {
        public override string type => "mask";

        public override Bounds bounds {
            get {
                if(mask == null) return base.bounds;
                Bounds parentBounds = mask.bounds;
                Bounds baseBounds = base.bounds;
                baseBounds = new Bounds(new Vector2i(
                    Math.Max(baseBounds.min.X,parentBounds.min.X),
                    Math.Max(baseBounds.min.Y, parentBounds.min.Y)), new Vector2i(
                    Math.Max(baseBounds.max.X, parentBounds.max.X),
                    Math.Max(baseBounds.max.Y, parentBounds.max.Y)));
                return baseBounds;
            }
        }
        public event EventHandler<OnScrollEventArgs> onScroll;

        public Mask(string id, List<string> tags, Vector2i? position = null, Vector2i? size = null, Vector2f? anchor = null,
            UIElement parent = null) : base(id, tags, position, size, anchor, parent) {
            OnScrollEventArgs onScrollArgs = new OnScrollEventArgs(id, 0f);
            Core.renderer.window.MouseWheelScrolled += (_, scroll) => {
                if(!Core.renderer.mousePosition.InBounds(bounds)) return;
                onScrollArgs.delta = scroll.Delta;
                onScroll?.Invoke(this, onScrollArgs);
            };
        }
    }
}
