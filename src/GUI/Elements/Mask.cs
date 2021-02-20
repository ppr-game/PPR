using System;
using System.Collections.Generic;

using MoonSharp.Interpreter;

using PPR.Main;

using PRR;

using SFML.System;
using SFML.Window;

namespace PPR.GUI.Elements {
    public class Mask : Panel {
        public override string type => "mask";
        
        public bool exclusive { get; set; }
        public List<Closure> onScroll { get; set; }

        public Mask(string id, List<string> tags, Vector2i? position = null, Vector2i? size = null, Vector2f? anchor = null,
            UIElement parent = null, bool exclusive = false) : base(id, tags, position, size, anchor, parent) {
            this.exclusive = exclusive;
            DynValue dynId = DynValue.NewString(id);
            Core.renderer.window.MouseWheelScrolled += (_, scroll) => {
                if(!Core.renderer.mousePosition.InBounds(bounds)) return;
                foreach(Closure closure in onScroll)
                    closure?.Call(dynId, DynValue.NewNumber(scroll.Delta));
            };
        }

        // TODO: implement masks
    }
}
