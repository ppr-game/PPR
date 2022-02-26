﻿using PER.Abstractions.Renderer;

using PRR;
using PRR.Sfml;

namespace PER.Demo;

public static class Core {
    public static Engine engine { get; } = new(new Game(), new Renderer()) { tickInterval = 0.02d };

    private static void Main() => engine.Start(new RendererSettings {
        title = "PER Demo Pog",
        width = 80,
        height = 60,
        framerate = 0,
        fullscreen = false,
        font = new Font("resources"),
        icon = null
    });
}
