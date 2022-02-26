using System;

namespace PER.Abstractions.Renderer;

public readonly struct RendererSettings {
    public string title { get; init; }
    public int width { get; init; }
    public int height { get; init; }
    public int framerate { get; init; }
    public bool fullscreen { get; init; }
    public IFont font { get; init; }
    public string? icon { get; init; }

    public RendererSettings(IRenderer renderer) {
        title = renderer.title;
        width = renderer.width;
        height = renderer.height;
        framerate = renderer.framerate;
        fullscreen = renderer.fullscreen;
        font = renderer.font ??
               throw new ArgumentNullException(nameof(renderer.font), "Failed to create renderer settings.");
        icon = renderer.icon;
    }
}
