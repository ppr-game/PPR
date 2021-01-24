using System;
using System.Collections.Generic;

using PRR;

using SFML.System;

namespace PPR.GUI.Elements {
    public class Mask : Panel {
        public override string type => "mask";
        
        public bool exclusive { get; set; }

        public Mask(string id, List<string> tags, Vector2i? position = null, Vector2i? size = null, Vector2f? anchor = null,
            UIElement parent = null, bool exclusive = false) : base(id, tags, position, size, anchor, parent) =>
            this.exclusive = exclusive;

        // TODO: implement masks
    }
}
