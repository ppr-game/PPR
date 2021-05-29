using System;
using System.Collections.Generic;

using NLog;

using PPR.UI.Animations;

using PRR;

using SFML.Graphics;
using SFML.System;

namespace PPR.UI.Elements {
    public class ProgressBar : Element {
        public override string type => "progressBar";

        public int value {
            get => _value;
            set {
                value = Math.Clamp(value, 0, maxValue);
                int difference = value - _value;

                if(difference == 0) return;
                if(difference > 0)
                    for(int x = _value; x < value; x++) {
                        Animation animation = _animations[x];
                        animation.speed = 1d;
                        if(!animation.playing) animation.Restart();
                    }
                else
                    for(int x = value; x < _value; x++) {
                        Animation animation = _animations[x];
                        animation.speed = -1d;
                        if(!animation.playing) animation.Restart(animation.time);
                    }

                _value = value;
                _scaledValue = (int)(value * ((float)size.X / maxValue));
            }
        }

        public int maxValue { get; set; }
        public string animation { get; set; }
        public float animationTime { get; set; }
        
        private Color foregroundColor => GetColor("fg");
        private Color backgroundColor => GetColor("bg");

        private int _value;
        private int _scaledValue;

        private Animation[] _animations;

        public ProgressBar(string id, int defaultValue, int maxValue, string animation, float animationTime,
            List<string> tags = null, Vector2i? position = null, Vector2i? size = null, Vector2f? anchor = null,
            Element parent = null) : base(id, tags, position, size, anchor, parent) {
            this.maxValue = maxValue;
            this.animation = animation;
            this.animationTime = animationTime;
            RecreateAnimations();
            value = defaultValue;
        }

        public override void Update() {
            base.Update();
            foreach(Animation animation in _animations) animation.Update();
        }

        public override void Draw() {
            Color transparent = GetColor("transparent");
            Color bg = backgroundColor;
            Color fg = foregroundColor;
            RenderCharacter backgroundCharacter = new RenderCharacter('\0', bg, transparent);
            RenderCharacter foregroundCharacter = new RenderCharacter('\0', fg, transparent);
            for(int x = 0; x < size.X; x++) {
                bool progressReachedHere = x < _scaledValue;
                
                Animation animation = _animations[x];
                Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> animationModifier =
                    animation.animationModifier;
                bool playAnimation = !(animationModifier is null);
                
                for(int y = 0; y < size.Y; y++) {
                    Vector2i pos = new Vector2i(position.X + x, position.Y + y);

                    if(playAnimation || !progressReachedHere) {
                        (Vector2i modBackgroundPos, RenderCharacter modBackgroundCharacter) =
                            ApplyAnimations(pos, backgroundCharacter);
                        Core.renderer.SetCharacter(modBackgroundPos, modBackgroundCharacter);
                    }

                    // ReSharper disable once InvertIf
                    if(playAnimation || progressReachedHere) {
                        (Vector2i pos, RenderCharacter character) foreground =
                            animationModifier?.Invoke(pos, foregroundCharacter) ?? (pos, foregroundCharacter);
                        foreground = ApplyAnimations(foreground.pos, foreground.character);
                        Core.renderer.SetCharacter(foreground.pos, foreground.character);
                    }
                }
            }
        }

        public void RecreateAnimations() {
            _animations = new Animation[maxValue];
            for(int i = 0; i < _animations.Length; i++) _animations[i] = 
                new Animation(animation, UI.Manager.animations[animation], animationTime, true,
                    null, null);
        }
    }
}
