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
                if(_currentAnimations is null) return;
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
            get => _currentAnimations.GetLength(0);
            set => UpdateMaxValues(value);
        }

        public AnimationSettings inAnimation { get; set; }
        public AnimationSettings outAnimation { get; set; }
        
        private Color foregroundColor => GetColor("fg");
        private Color backgroundColor => GetColor("bg");

        private int _value;
        private int _scaledValue;

        private Vector2i _size;
        private Animation[,] _currentAnimations;
        private Color[,] _currentColors;

        public ProgressBar(string id, int defaultValue, int maxValue, AnimationSettings inAnimation,
            AnimationSettings outAnimation, List<string> tags = null, Vector2i? position = null, Vector2i? size = null,
            Vector2f? anchor = null, Element parent = null) : base(id, tags, position, size, anchor, parent) {
            this.maxValue = maxValue;
            this.inAnimation = inAnimation;
            this.outAnimation = outAnimation;
            value = defaultValue;
        }

        public override void Update() {
            base.Update();
            if(!enabled || _currentAnimations is null) return;
            foreach(Animation animation in _currentAnimations) animation?.Update();
        }

        public override void Draw() {
            Color transparent = GetColor("transparent");
            Color bg = backgroundColor;
            Color fg = foregroundColor;
            RenderCharacter backgroundCharacter = new RenderCharacter('\0', bg, transparent);
            RenderCharacter foregroundCharacter = new RenderCharacter('\0', fg, transparent);
            Bounds? maskBounds = mask?.bounds;
            
            for(int x = 0; x < size.X; x++) {
                int globalX = position.X + x;
                if(maskBounds.HasValue && (globalX < maskBounds.Value.min.X || globalX > maskBounds.Value.max.X))
                    continue;
                
                bool progressReachedHere = x < _scaledValue;
                
                for(int y = 0; y < size.Y; y++) {
                    Vector2i pos = new Vector2i(globalX, position.Y + y);
                    
                    if(maskBounds.HasValue && (pos.Y < maskBounds.Value.min.Y || pos.Y > maskBounds.Value.max.Y))
                        continue;
                
                    Animation animation = _currentAnimations[x, y];
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
            _currentAnimations = new Animation[maxValue, size.Y];
            _currentColors = new Color[maxValue, size.Y];
        }

        private void RestartAnimationsAt(int x, AnimationSettings animationSettings) {
            for(int y = 0; y < _currentColors.GetLength(1); y++) {
                Animation animation = new Animation(animationSettings, null, new Dictionary<string, double> {
                        { "startR", _currentColors[x, y].R },
                        { "startG", _currentColors[x, y].G },
                        { "startB", _currentColors[x, y].B },
                        { "startA", _currentColors[x, y].A }
                    });
                animation.Restart();
                _currentAnimations[x, y]?.Stop();
                _currentAnimations[x, y] = animation;
            }
        }
    }
}
