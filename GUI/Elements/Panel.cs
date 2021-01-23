using System;

using PRR;

using SFML.System;

namespace PPR.GUI.Elements {
    public class Panel : UIElement {
        public override string type => "panel";

        public Panel(string uid, string id, Vector2i? position = null, Vector2i? size = null, Vector2f? anchor = null,
            UIElement parent = null) : base(uid, id, position, size, anchor, parent) { }
        
        public override void Draw(Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> transition) { }
    }
}
