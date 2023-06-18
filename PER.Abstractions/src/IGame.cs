using System;

using JetBrains.Annotations;

using PER.Abstractions.Rendering;

namespace PER.Abstractions;

[PublicAPI]
public interface IGame : IUpdatable, ITickable {
    public IScreen? currentScreen { get; }
    public void SwitchScreen(IScreen? screen, Action? middleCallback = null);
    public void SwitchScreen(IScreen? screen, float fadeOutTime, float fadeInTime, Action? middleCallback = null);
    public void FadeScreen(Action middleCallback);
    public void FadeScreen(float fadeOutTime, float fadeInTime, Action middleCallback);
    public void Unload();
    public void Load();
    public RendererSettings Loaded();
    public void Setup();
    public void Finish();
}
