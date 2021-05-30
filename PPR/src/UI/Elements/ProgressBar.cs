using System;
using System.Collections.Generic;

using PPR.UI.Animations;

using PRR;

using SFML.Graphics;
using SFML.System;

namespace PPR.UI.Elements {
    public class ProgressBar : Element {
        public override string type => "progressBar";

        public override Vector2i size {
            get => _size;
            set {
                _size = value;
                if(_animations is null) return;
                UpdateMaxValues(maxValue);
                _scaledValue = (int)(_value * ((float)size.X / maxValue));
            }
        }

        public int value {
            get => _value;
            set {
                value = Math.Clamp(value, 0, maxValue);
                int difference = value - _value;

                if(difference == 0) return;
                if(difference > 0)
                    for(int x = _value; x < value; x++)
                        RestartAnimationsAt(x, inAnimation);
                else
                    for(int x = value; x < _value; x++)
                        RestartAnimationsAt(x, outAnimation);

                _value = value;
                _scaledValue = (int)(value * ((float)size.X / maxValue));
            }
        }

        public int maxValue {
            get => _animations.GetLength(0);
            set => UpdateMaxValues(value);
        }

        public AnimationData inAnimation { get; set; }
        public AnimationData outAnimation { get; set; }
        
        private Color foregroundColor => GetColor("fg");
        private Color backgroundColor => GetColor("bg");

        private int _value;
        private int _scaledValue;

        private Vector2i _size;
        private Animation[,] _animations;
        private Color[,] _currentColors;

        public ProgressBar(string id, int defaultValue, int maxValue, AnimationData inAnimation,
            AnimationData outAnimation, List<string> tags = null, Vector2i? position = null, Vector2i? size = null,
            Vector2f? anchor = null, Element parent = null) : base(id, tags, position, size, anchor, parent) {
            this.maxValue = maxValue;
            this.inAnimation = inAnimation;
            this.outAnimation = outAnimation;
            value = defaultValue;
        }

        public override void Update() {
            base.Update();
            if(_animations is null) return;
            foreach(Animation animation in _animations) animation?.Update();
        }

        public override void Draw() {
            Color transparent = GetColor("transparent");
            Color bg = backgroundColor;
            Color fg = foregroundColor;
            RenderCharacter backgroundCharacter = new RenderCharacter('\0', bg, transparent);
            RenderCharacter foregroundCharacter = new RenderCharacter('\0', fg, transparent);
            for(int x = 0; x < size.X; x++) {
                bool progressReachedHere = x < _scaledValue;
                
                for(int y = 0; y < size.Y; y++) {
                    Vector2i pos = new Vector2i(position.X + x, position.Y + y);
                
                    Animation animation = _animations[x, y];
                    Func<Vector2i, RenderCharacter, (Vector2i, RenderCharacter)> animationModifier =
                        animation?.animationModifier;

                    RenderCharacter targetCharacter = progressReachedHere ? foregroundCharacter : backgroundCharacter;

                    (Vector2i pos, RenderCharacter character) mod =
                        animationModifier?.Invoke(pos, targetCharacter) ?? (pos, targetCharacter);
                    mod = ApplyAnimations(mod.pos, mod.character);
                    Core.renderer.SetCharacter(mod.pos, mod.character);
                    _currentColors[x, y] = mod.character.background;
                }
            }
        }

        private void UpdateMaxValues(int maxValue) {
            _animations = new Animation[maxValue, size.Y];
            _currentColors = new Color[maxValue, size.Y];
        }

        private void RestartAnimationsAt(int x, AnimationData animationData) {
            for(int y = 0; y < _currentColors.GetLength(1); y++) {
                Animation animation = new Animation(animationData.id, UI.Manager.animations[animationData.id],
                    animationData.time, true, null,
                    new Dictionary<string, double> {
                        { "startR", _currentColors[x, y].R },
                        { "startG", _currentColors[x, y].G },
                        { "startB", _currentColors[x, y].B },
                        { "startA", _currentColors[x, y].A }
                    });
                animation.Restart();
                _animations[x, y]?.Stop();
                _animations[x, y] = animation;
            }
        }
    }
}
