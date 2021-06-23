using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using MoonSharp.Interpreter;

using PPR.UI.Animations;

using PRR;

using SFML.Graphics;
using SFML.System;

namespace PPR.UI.Elements {
    public abstract class Element {
        public virtual string type => "none";

        public bool enabled {
            get => _enabled && (parent?.enabled ?? true);
            set {
                if(_enabled == value) return;
                _enabled = value;
                Lua.Manager.InvokeEvent(this, value ? "elementEnabled" : "elementDisabled", this);
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
                Vector2i size = this.size;
                if(size.X > 0) size.X--;
                if(size.Y > 0) size.Y--;
                return new Bounds(start, start + size);
            }
        }

        public virtual Vector2f anchor { get; set; }

        public virtual Element parent {
            get => _parent;
            set {
                _parent = value;
                mask = value is Mask maskParent ? maskParent : value?.mask;
            }
        }

        public virtual Mask mask { get; private set; }

        public virtual Bounds maskBounds {
            get {
                if(mask is null)
                    return new Bounds(new Vector2i(0, 0),
                        new Vector2i(Core.renderer.width - 1, Core.renderer.height - 1));

                return mask.bounds;
            }
        }

        public IReadOnlyList<Animation> animations => _animations;

        private readonly List<Animation> _animations = new List<Animation>();
        private readonly List<Animation> _animationsToRemove = new List<Animation>();
        
        private Element _parent;
        private bool _enabled = true;

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        protected Element(string id, List<string> tags = null, Vector2i? position = null, Vector2i? size = null,
            Vector2f? anchor = null, Element parent = null) {
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
        
        [MoonSharpHidden]
        public virtual void Update() {
            if(!enabled) return;
            UpdateAnimations();
        }

        [MoonSharpHidden]
        public virtual void Draw() { }

        public void AddAnimation(Animation animation) {
            if(animation == null) return;
            if(animation.settings.endState) enabled = true;
            Lua.Manager.InvokeEvent(this, "animationStarted", this, animation.dynValueId);
            _animations.Add(animation);
        }
        
        private void UpdateAnimations() {
            foreach(Animation animation in _animations) {
                if(!animation.started) animation.Restart();
                
                animation.Update();
                
                if(animation.playing) continue;
                animation.Stop();
                AnimationStopped(animation);
            }
            
            RemoveRemovedAnimations();
        }

        public bool StopAnimation(Animation animation) {
            bool wasPlaying = _animations.Contains(animation);
            if(!wasPlaying) return false;
            
            animation.Stop();
            AnimationStopped(animation);
            RemoveRemovedAnimations();
            return true;
        }

        private void AnimationStopped(Animation animation) {
            _animationsToRemove.Add(animation);
                
            if(!animation.settings.endState) enabled = false;
            Lua.Manager.InvokeEvent(this, "animationFinished", this, animation.dynValueId);
            animation.endCallback?.Call();
        }

        private void RemoveRemovedAnimations() {
            foreach(Animation animation in _animationsToRemove) _animations.Remove(animation);
            _animationsToRemove.Clear();
        }

        [SuppressMessage("ReSharper", "ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator")]
        protected (Vector2i, RenderCharacter) ApplyAnimations(Vector2i pos, RenderCharacter character) {
            (Vector2i pos, RenderCharacter character) mod = (pos, character);
            if(parent != null) mod = parent.ApplyAnimations(pos, character);
            foreach(Animation animation in _animations) {
                Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> animMod = animation.animationModifier;
                if(animMod == null) continue;
                mod = animMod(mod.pos, mod.character);
            }
            return mod;
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
