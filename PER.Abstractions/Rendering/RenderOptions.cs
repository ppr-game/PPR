﻿using System;

using JetBrains.Annotations;

namespace PER.Abstractions.Rendering;

#pragma warning disable CA1069
[PublicAPI]
[Flags]
public enum RenderOptions : byte {
    Default = BackgroundAlphaBlending,
    None = 0,
    BackgroundAlphaBlending = 0b1,
    InvertedBackgroundAsForegroundColor = 0b10
}
#pragma warning restore CA1069
