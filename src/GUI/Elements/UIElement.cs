using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using MoonSharp.Interpreter;

using NLog;

using PPR.Main;

using PRR;

using SFML.Graphics;
using SFML.System;

namespace PPR.GUI.Elements {
    public abstract class UIElement {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public virtual string type => "none";

        public bool enabled {
            get => _enabled && (parent?.enabled ?? true);
            set {
                if(_enabled == value) return;
                _enabled = value;
                Lua.InvokeEvent(this, value ? "elementEnabled" : "elementDisabled", this);
            }
        }

        public string id { get; }
        public List<string> tags { get; set; }
        public virtual Vector2i position { get; set; }

        public Vector2i globalPosition =>
            // what the actual fuck, when i write `parent?.globalPosition.Y ?? 0` it says that Y can never be null
            // but when i write it as `parent == null ? 0 : parent.globalPosition.Y` it suggests me to write it as
            // `parent?.globalPosition.Y ?? 0` and continues to display the same warning
            new Vector2i(
            // ReSharper disable once MergeConditionalExpression
            (parent == null ? (int)(Core.renderer.width * anchor.X) : parent.globalPosition.X) + position.X +
            (int)((parent?.size.X ?? 0) * anchor.X),
            // ReSharper disable once MergeConditionalExpression
            (parent == null ? (int)(Core.renderer.height * anchor.Y) : parent.globalPosition.Y) + position.Y +
            (int)((parent?.size.Y ?? 0) * anchor.Y));

        public virtual Vector2i size { get; set; }
        
        public virtual Bounds bounds {
            get {
                Vector2i start = globalPosition;
                return new Bounds(start, start + size);
            }
        }

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
                animationEndTime = value?.time ?? 0f;
                animationEndState = value?.endState ?? enabled;
                animationEndCallback = value?.endCallback;
                _animationName = DynValue.NewString(value?.id);
                animationTime = 0f;
                
                if(animationEndState) enabled = true;
                Lua.InvokeEvent(this, "animationStarted", this, _animationName);
                
                animationStartTime = DateTime.UtcNow;
            }
        }

        protected Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> animationModifier =>
            animationPlaying ? _animation(animationTime / animationEndTime) : parent?.animationModifier;
        protected DateTime animationStartTime { get; set; }
        protected float animationTime { get; private set; }
        protected float animationEndTime { get; set; }
        protected bool animationEndState { get; set; }
        protected Closure animationEndCallback { get; set; }

        protected bool animationPlaying => animationTime < animationEndTime && _animation != null;
        protected bool animationStopped => animationTime >= animationEndTime && _animation != null;
            
        private DynValue _animationName;
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

        public virtual void Draw() { }
        
        public virtual void Update() => UpdateAnimation();

        private void UpdateAnimation() {
            animationTime = (float)(DateTime.UtcNow - animationStartTime).TotalSeconds;

            if(!animationStopped) return;
            _animation = null;
            if(!animationEndState) enabled = false;
            Lua.InvokeEvent(this, "animationFinished", this, _animationName);
            animationEndCallback?.Call();
        }

        protected Color GetColor(string colorName) {
            bool colorExists = ColorScheme.TryGetElementColor(new ElementColor(type, id, colorName), out Color color);

            if(colorExists) return color;

            for(int i = tags.Count - 1; i >= 0; i--) {
                colorExists = ColorScheme.TryGetTagColor(new ElementColor(type, tags[i], colorName), out color);
                if(colorExists) return color;
            }

            return color;
        }
    }
}
