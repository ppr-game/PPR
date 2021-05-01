using System.Collections.Generic;

using SFML.System;

namespace PPR.UI.Elements {
    public class Panel : Element {
        public override string type => "panel";

        public Panel(string id, List<string> tags, Vector2i? position, Vector2i? size, Vector2f? anchor,
            Element parent) : base(id, tags, position, size, anchor, parent) { }
    }
}
