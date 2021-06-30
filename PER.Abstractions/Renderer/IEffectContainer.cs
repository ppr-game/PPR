using PER.Util;

namespace PER.Abstractions.Renderer {
    public interface IEffectContainer {
        public IEffect effect { get; set; }

        public void ApplyModifiers(ref (Vector2 position, RenderCharacter character) character) {
            if(effect is not null && effect.hasModifiers)
                character = effect.ApplyModifiers(character.position, character.character);
        }
    }
}
