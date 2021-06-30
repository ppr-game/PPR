using PER.Util;

namespace PER.Abstractions.Renderer {
    public interface IEffectContainer {
        public IEffect effect { get; set; }

        public void ApplyModifiers(ref (Vector2 position, RenderCharacter character) mod) {
            if(effect is not null && effect.hasModifiers)
                mod = effect.ApplyModifiers(mod.position, mod.character);
        }
    }
}
