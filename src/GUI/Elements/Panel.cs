using System.Collections.Generic;

using SFML.System;

namespace PPR.GUI.Elements {
    public class Panel : UIElement {
        public override string type => "panel";

        public Panel(string id, List<string> tags, Vector2i? position, Vector2i? size, Vector2f? anchor,
            UIElement parent) : base(id, tags, position, size, anchor, parent) { }
    }
}
