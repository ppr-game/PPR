using System;

namespace PER.Abstractions.Renderer {
    [Flags]
    public enum RenderOptions : byte {
        Default = BackgroundAlphaBlending,
        None = 0,
        BackgroundAlphaBlending = 0b1,
        InvertedBackgroundAsForegroundColor = 0b10
    }
}
