using System;
using System.Collections.Generic;
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

        public string id { get; }
        public List<string> tags { get; }
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

        public UIAnimation? animation {
            set {
                _animation = value?.animation;
                animationTime = -value?.delay ?? 0f;
                animationEndTime = value?.time ?? 0f;
                animationEndState = value?.endState ?? enabled;
                enabled = value?.startState ?? enabled;
            }
        }

        protected Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> animationModifier =>
            animationPlaying ? _animation(animationTime / animationEndTime) : parent?.animationModifier;
        protected float animationTime { get; set; }
        protected float animationEndTime { get; set; }
        protected bool animationEndState { get; set; }

        protected bool animationPlaying =>
            animationTime >= 0f && animationTime < animationEndTime && _animation != null;
        protected bool animationStopped =>
            animationTime >= 0f && animationTime >= animationEndTime && _animation != null;

        private Func<float, Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)>> _animation;
        private UIElement _parent;
        private bool _enabled = true;

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        protected UIElement(string id, List<string> tags = null, Vector2i? position = null, Vector2i? size = null,
            Vector2f? anchor = null, UIElement parent = null) {
            this.id = id;
            this.tags = tags ?? new List<string>();
            this.position = position ?? parent?.position ?? new Vector2i();
            Vector2i tempSize = size ?? parent?.size ?? new Vector2i(1, 1);
            this.anchor = anchor ?? parent?.anchor ?? new Vector2f();
            this.parent = parent;

            if(tempSize.X < 0) tempSize.X = Core.renderer.width;
            if(tempSize.Y < 0) tempSize.Y = Core.renderer.height;
            this.size = tempSize;
        }

        public virtual void Draw() => UpdateAnimation();

        private void UpdateAnimation() {
            if(animationStopped) {
                _animation = null;
                enabled = animationEndState;
            }
            else if(_animation != null) animationTime += Core.deltaTime;
        }
    }
}
