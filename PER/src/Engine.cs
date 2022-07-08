using System;
using System.Reflection;

using NLog;

using PER.Abstractions;
using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;
using PER.Util;

namespace PER;

public class Engine {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static readonly string version = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion ?? "0.0.0";

    public IReadOnlyStopwatch clock => _clock;
    public double deltaTime { get; private set; }

    public double tickInterval { get; set; }
    public IResources resources { get; }
    public IGame game { get; }
    public IRenderer renderer { get; }
    public IInput input { get; }
    public IAudio audio { get; }

    private readonly Stopwatch _clock = new();
    private TimeSpan _prevTime;
    private double _tickAccumulator;

    public Engine(IResources resources, IGame game, IRenderer renderer, IInput input, IAudio audio) {
        this.resources = resources;
        this.game = game;
        this.renderer = renderer;
        this.input = input;
        this.audio = audio;
    }

    public bool Reload() {
        try {
            bool loaded = resources.loaded;
            if(loaded) {
                resources.Unload();
                game.Unload();
            }
            game.Load();
            resources.Load();
            game.Loaded();
            if(loaded)
                input.Reset();
            return true;
        }
        catch(Exception exception) {
            logger.Error("Uncaught exception! Please, report the text below to the developer of the game.");
            logger.Fatal(exception);
            throw;
        }
    }

    public void Start(RendererSettings rendererSettings) {
        logger.Info($"PER v{version}");
        Setup(rendererSettings);
        while(Update()) UpdateDeltaTime();
        Finish();
    }

    private void Setup(RendererSettings rendererSettings) {
        _clock.Reset();

        renderer.Setup(rendererSettings);
        input.Reset();
        game.Setup();

        logger.Info("Setup finished");
    }

    private bool Update() {
        renderer.Clear();
        renderer.Update();
        input.Update();
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
        resources.Unload();
        game.Unload();
        input.Finish();
        renderer.Finish();
        game.Finish();
        audio.Finish();
    }
}
