using System;

using PRR;

using SFML.System;

namespace PPR.GUI.Elements {
    public class Mask : Panel {
        public override string type => "mask";
        
        public bool exclusive { get; set; }

        public Mask(string uid, string id, Vector2i? position = null, Vector2i? size = null, Vector2f? anchor = null,
            UIElement parent = null, bool exclusive = false) : base(uid, id, position, size, anchor, parent) =>
            this.exclusive = exclusive;

        // TODO: implement masks
        public override void Draw(Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> transition) { }
    }
}
