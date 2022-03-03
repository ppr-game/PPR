﻿using PER.Audio.Sfml;
using PER.Common.Resources;
using PER.Demo.Resources;

using PRR.Sfml;

namespace PER.Demo;

public static class Core {
    private static readonly Renderer renderer = new();
    public static Engine engine { get; } =
        new(new ResourcesManager(), new Game(), renderer, new InputManager(renderer), new AudioManager()) {
            tickInterval = 0.02d
        };

    private static void Main() => engine.Reload();
}
