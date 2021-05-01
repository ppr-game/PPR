using System;
using System.Collections.Generic;

using MoonSharp.Interpreter;

using PPR.Main;

using SFML.System;

namespace PPR.UI.Elements {
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
                    Math.Min(baseBounds.max.X, parentBounds.max.X),
                    Math.Min(baseBounds.max.Y, parentBounds.max.Y)));
                return baseBounds;
            }
        }

        public Mask(string id, List<string> tags, Vector2i? position = null, Vector2i? size = null, Vector2f? anchor = null,
            Element parent = null) : base(id, tags, position, size, anchor, parent) =>
            Core.renderer.window.MouseWheelScrolled += (_, scroll) => {
            if(!enabled || !Core.renderer.mousePosition.InBounds(bounds)) return;
            Lua.Manager.InvokeEvent(this, "maskScrolled", this, DynValue.NewNumber(scroll.Delta));
        };
    }
}
