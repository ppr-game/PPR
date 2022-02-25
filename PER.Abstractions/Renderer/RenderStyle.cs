using System;

namespace PER.Abstractions.Renderer {
    [Flags]
    public enum RenderStyle : byte {
        None = 0,
        All = 0b1111,
        AllPerFont = 0b0111,
        Bold = 0b1,
        Underline = 0b10,
        Strikethrough = 0b100,
        Italic = 0b1000
    }
}
