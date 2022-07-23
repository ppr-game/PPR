using System;

using JetBrains.Annotations;

using PER.Abstractions.Rendering;

namespace PER.Abstractions;

[PublicAPI]
public interface IGame {
    public IScreen? currentScreen { get; }
    public void SwitchScreen(IScreen? screen);
    public void SwitchScreen(IScreen? screen, float fadeOutTime, float fadeInTime);
    public void FadeScreen(Action middleCallback);
    public void FadeScreen(float fadeOutTime, float fadeInTime, Action middleCallback);
    public void Unload();
    public void Load();
    public RendererSettings Loaded();
    public void Setup();
    public void Update();
    public void Tick(TimeSpan time);
    public void Finish();
}
