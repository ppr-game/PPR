using JetBrains.Annotations;

namespace PER.Abstractions;

[PublicAPI]
public interface IGame {
    public IScreen? currentScreen { get; }
    public void SwitchScreen(IScreen? screen);
    public void SwitchScreen(IScreen? screen, float fadeOutTime, float fadeInTime);
    public void Unload();
    public void Load();
    public void Loaded();
    public void Setup();
    public void Update();
    public void Tick();
    public void Finish();
}
