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
        
        public bool exclusive { get; set; }
        public event EventHandler<OnScrollEventArgs> onScroll;

        public Mask(string id, List<string> tags, Vector2i? position = null, Vector2i? size = null, Vector2f? anchor = null,
            UIElement parent = null, bool exclusive = false) : base(id, tags, position, size, anchor, parent) {
            this.exclusive = exclusive;
            OnScrollEventArgs onScrollArgs = new OnScrollEventArgs(id, 0f);
            Core.renderer.window.MouseWheelScrolled += (_, scroll) => {
                if(!Core.renderer.mousePosition.InBounds(bounds)) return;
                onScrollArgs.delta = scroll.Delta;
                onScroll?.Invoke(this, onScrollArgs);
            };
        }

        // TODO: implement masks
    }
}
