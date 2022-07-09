using System;

using JetBrains.Annotations;

using PER.Util;

namespace PER.Abstractions.Rendering;

[PublicAPI]
public readonly struct RenderCharacter {
    public char character { get; }
    public Color background { get; }
    public Color foreground { get; }
    public RenderStyle style { get; }

    public RenderCharacter(char character, Color background, Color foreground,
        RenderStyle style = RenderStyle.None) {
        this.character = character;
        this.background = background;
        this.foreground = foreground;
        this.style = style;
    }

    public bool Equals(RenderCharacter other) => character == other.character &&
                                                 background.Equals(other.background) &&
                                                 foreground.Equals(other.foreground);

    public override bool Equals(object? obj) => obj is RenderCharacter other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(character, background, foreground);

    public static bool operator ==(RenderCharacter left, RenderCharacter right) => left.Equals(right);
    public static bool operator !=(RenderCharacter left, RenderCharacter right) => !left.Equals(right);
}
