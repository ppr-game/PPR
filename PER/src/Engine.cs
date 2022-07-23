using System;

using JetBrains.Annotations;

using NLog;

using PER.Abstractions;
using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Util;

namespace PER;

[PublicAPI]
public class Engine {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static readonly string version = Helper.GetVersion();
    public static readonly string abstractionsVersion = Helper.GetVersion(typeof(IGame));

    public IReadOnlyStopwatch clock => _clock;
    public double deltaTime { get; private set; }

    public TimeSpan tickInterval { get; set; }
    public IResources resources { get; }
    public IGame game { get; }
    public IRenderer renderer { get; }
    public IInput input { get; }
    public IAudio audio { get; }

    private readonly Stopwatch _clock = new();
    private TimeSpan _prevTime;
    private TimeSpan _lastTickTime;

    public Engine(IResources resources, IGame game, IRenderer renderer, IInput input, IAudio audio) {
        this.resources = resources;
        this.game = game;
        this.renderer = renderer;
        this.input = input;
        this.audio = audio;
    }

    public void Reload() {
        try {
            logger.Info($"PER v{version}");
            logger.Info("Reloading game");

            bool loaded = resources.loaded;
            if(loaded) {
                resources.Unload();
                game.Unload();
            }
            game.Load();
            resources.Load();
            RendererSettings rendererSettings = game.Loaded();
            if(loaded)
                input.Reset();

            if(renderer.open)
                renderer.Reset(rendererSettings);
            else
                Run(rendererSettings);
        }
        catch(Exception exception) {
            logger.Fatal(exception, "Uncaught exception! Please, report this file to the developer of the game.");
            throw;
        }
    }

    private void Run(RendererSettings rendererSettings) {
        logger.Info("Starting game");
        Setup(rendererSettings);
        while(Update())
            UpdateDeltaTime();
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
        input.Update();
        renderer.Clear();
        renderer.Update();
        game.Update();
        TryTick();
        renderer.Draw();
        return renderer.open;
    }

    private void TryTick() {
        if(tickInterval < TimeSpan.Zero) {
            _lastTickTime = clock.time;
            return;
        }

        while(clock.time - _lastTickTime >= tickInterval) {
            _lastTickTime += tickInterval;
            Tick(_lastTickTime);
        }
    }

    private void Tick(TimeSpan time) => game.Tick(time);

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

        logger.Info("nooooooo *dies*");
        LogManager.Shutdown();
    }
}
