using System;

using PER.Util;

namespace PER.Abstractions.Renderer {
    public readonly struct RenderCharacter {
        public readonly char character;
        public readonly Color background;
        public readonly Color foreground;
        
        public RenderCharacter(char character, Color background, Color foreground) {
            this.character = character;
            this.background = background;
            this.foreground = foreground;
        }

        public RenderCharacter(char character, RenderCharacter oldChar) {
            this.character = character;
            background = oldChar.background;
            foreground = oldChar.foreground;
        }

        public RenderCharacter(Color background, Color foreground, RenderCharacter oldChar) {
            character = oldChar.character;
            this.background = background;
            this.foreground = foreground;
        }

        public bool Equals(RenderCharacter other) => character == other.character &&
                                                     background.Equals(other.background) &&
                                                     foreground.Equals(other.foreground);

        public override bool Equals(object obj) => obj is RenderCharacter other && Equals(other);
        
        public override int GetHashCode() => HashCode.Combine(character, background, foreground);
        
        public static bool operator ==(RenderCharacter left, RenderCharacter right) => left.Equals(right);
        public static bool operator !=(RenderCharacter left, RenderCharacter right) => !left.Equals(right);
    }
}
