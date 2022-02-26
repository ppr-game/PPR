using System;
using System.IO;
using System.Reflection;

using NLog;

using PER.Abstractions;
using PER.Abstractions.Audio;
using PER.Abstractions.Renderer;
using PER.Util;

namespace PER;

public class Engine {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static readonly string version = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion ?? "0.0.0";

    public event EventHandler? setupFinished;

    public static string resourcesPath => "resources";
    public static string graphicsPath => Path.Combine(resourcesPath, "graphics");
    public static string audioPath => Path.Combine(resourcesPath, "audio");

    public IReadOnlyStopwatch clock => _clock;
    public double deltaTime { get; private set; }

    public double tickInterval { get; set; }
    public IGame game { get; }
    public IRenderer renderer { get; }
    public IAudio audio { get; }

    private readonly Stopwatch _clock = new();
    private TimeSpan _prevTime;
    private double _tickAccumulator;

    public Engine(IGame game, IRenderer renderer, IAudio audio) {
        this.game = game;
        this.renderer = renderer;
        this.audio = audio;
    }

    public void Start(RendererSettings rendererSettings) {
        try {
            logger.Info($"PER v{version}");
            Setup(rendererSettings);
            while(Update()) UpdateDeltaTime();
            Finish();
        }
        catch(Exception exception) {
            logger.Error("Uncaught exception! Please, report the text below to the developer of the game.");
            logger.Fatal(exception);
            throw;
        }
    }

    private void Setup(RendererSettings rendererSettings) {
        _clock.Reset();

        renderer.Setup(rendererSettings);
        game.Setup();

        logger.Info("Setup finished");
        setupFinished?.Invoke(this, EventArgs.Empty);
    }

    private bool Update() {
        renderer.Clear();
        renderer.Update();
        game.Update();
        TryTick();
        renderer.Draw();
        return renderer.open;
    }

    private void TryTick() {
        if(tickInterval <= 0d) return;
        _tickAccumulator += deltaTime;

        while(_tickAccumulator >= tickInterval) {
            Tick();
            _tickAccumulator -= tickInterval;
        }
    }

    private void Tick() => game.Tick();

    private void UpdateDeltaTime() {
        TimeSpan time = clock.time;
        deltaTime = (clock.time - _prevTime).TotalSeconds;
        _prevTime = time;
    }

    private void Finish() {
        renderer.Finish();
        game.Finish();
        audio.Finish();
    }
}
