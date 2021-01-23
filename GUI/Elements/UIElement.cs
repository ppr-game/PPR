using System;
using System.Diagnostics.CodeAnalysis;

using PRR;

using SFML.System;

namespace PPR.GUI.Elements {
    public abstract class UIElement {
        public virtual string type => "none";

        public bool enabled {
            get => _enabled && (parent?.enabled ?? true);
            set => _enabled = value;
        }

        public string uid { get; }
        public string id { get; }
        public virtual Vector2i position { get; set; }
        
        public Vector2i globalPosition {
            // what the actual fuck, when i write `parent?.globalPosition.Y ?? 0` it says that Y can never be null
            // but when i write it as `parent == null ? 0 : parent.globalPosition.Y` it suggests me to write it as
            // `parent?.globalPosition.Y ?? 0` and continues to display the same warning
            get => new Vector2i(
                // ReSharper disable once MergeConditionalExpression
                (parent == null ? (int)(Core.renderer.width * anchor.X) : parent.globalPosition.X) + position.X +
                (int)((parent?.size.X ?? 0) * anchor.X),
                // ReSharper disable once MergeConditionalExpression
                (parent == null ? (int)(Core.renderer.height * anchor.Y) : parent.globalPosition.Y) + position.Y +
                (int)((parent?.size.Y ?? 0) * anchor.Y));
            set => position = new Vector2i(
                value.X - position.X - (int)((parent?.size.X ?? 0) * anchor.X),
                value.Y - position.Y - (int)((parent?.size.Y ?? 0) * anchor.Y));
        }

        public virtual Vector2i size { get; set; }
        public virtual Vector2f anchor { get; set; }

        public virtual UIElement parent {
            get => _parent;
            set {
                _parent = value;
                mask = value is Mask maskParent ? maskParent : value?.mask;
            }
        }

        public virtual Mask mask { get; private set; }

        private UIElement _parent;
        private bool _enabled = true;

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        protected UIElement(string uid, string id, Vector2i? position = null, Vector2i? size = null, Vector2f? anchor = null,
            UIElement parent = null) {
            this.uid = uid;
            this.id = id;
            this.position = position ?? parent?.position ?? new Vector2i();
            this.size = size ?? parent?.size ?? new Vector2i(1, 1);
            this.anchor = anchor ?? parent?.anchor ?? new Vector2f();
            this.parent = parent;
        }

        public abstract void Draw(Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> transition);
    }
}
