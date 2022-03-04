using System.Globalization;

using PER.Abstractions;
using PER.Abstractions.Renderer;
using PER.Common.Effects;
using PER.Util;

namespace PER.Common;

public abstract class GameBase : IGame {
    private int _fps;
    private int _avgFPS;
    private int _tempAvgFPS;
    private int _tempAvgFPSCounter;

    protected abstract double deltaTime { get; }
    protected abstract IRenderer renderer { get; }

    public IScreen? currentScreen { get; private set; }
    private readonly FadeEffect _screenFade = new();

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

    public virtual void Setup() => renderer.closed += (_, _) => renderer.Close();

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

        renderer.DrawText(new Vector2Int(renderer.width - 1, renderer.height - 1),
            $"{_fps.ToString(CultureInfo.InvariantCulture)}/{_avgFPS.ToString(CultureInfo.InvariantCulture)} FPS",
            _ => new Formatting(Color.white, Color.transparent), HorizontalAlignment.Right);
    }

    public virtual void Tick() => currentScreen?.Tick();

    public virtual void Finish() { }
}
