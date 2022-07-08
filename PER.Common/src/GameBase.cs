using System.Globalization;

using PER.Abstractions;
using PER.Abstractions.Rendering;
using PER.Common.Effects;
using PER.Util;

namespace PER.Common;

public abstract class GameBase : IGame {
    private const float StartupWaitTime = 0.5f;
    private const float StartupFadeTime = 2f;
    private const float ShutdownFadeTime = 2f;
    private const float FadeTime = 0.3f;

    private int _fps;
    private int _avgFPS;
    private int _tempAvgFPS;
    private int _tempAvgFPSCounter;

    protected abstract double deltaTime { get; }
    protected abstract IRenderer renderer { get; }

    public IScreen? currentScreen { get; private set; }
    private readonly FadeEffect _screenFade = new();

    public void SwitchScreen(IScreen? screen) {
        if(currentScreen is null) SwitchScreen(screen, StartupWaitTime, StartupFadeTime);
        else if(screen is null) SwitchScreen(screen, ShutdownFadeTime, 0f);
        else SwitchScreen(screen, FadeTime, FadeTime);
    }

    public void SwitchScreen(IScreen? screen, float fadeOutTime, float fadeInTime) =>
        _screenFade.Start(fadeOutTime, fadeInTime, () => {
            currentScreen?.Close();
            currentScreen = screen;
            currentScreen?.Open();
            if(currentScreen is null) renderer.Close();
        });

    public abstract void Unload();
    public abstract void Load();
    public abstract void Loaded();

    public virtual void Setup() => renderer.closed += (_, _) => SwitchScreen(null);

    public virtual void Update() {
        if(_screenFade.fading) renderer.AddEffect(_screenFade);

        currentScreen?.Update();

        _fps = (int)Math.Round(1d / deltaTime);
        _tempAvgFPS += _fps;
        _tempAvgFPSCounter++;
        if(_tempAvgFPSCounter >= _avgFPS) {
            _avgFPS = _tempAvgFPS / _tempAvgFPSCounter;
            _tempAvgFPS = 0;
            _tempAvgFPSCounter = 0;
        }

        if(deltaTime == 0d || currentScreen == null) return;
        renderer.DrawText(new Vector2Int(renderer.width - 1, renderer.height - 1),
            $"{_fps.ToString(CultureInfo.InvariantCulture)}/{_avgFPS.ToString(CultureInfo.InvariantCulture)} FPS",
            _ => new Formatting(Color.white, Color.transparent), HorizontalAlignment.Right);
    }

    public virtual void Tick() => currentScreen?.Tick();

    public virtual void Finish() { }
}
