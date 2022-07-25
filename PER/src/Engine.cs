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

    public FrameTime frameTime { get; } = new();

    public TimeSpan tickInterval { get; set; }
    public IResources resources { get; }
    public IGame game { get; }
    public IRenderer renderer { get; }
    public IInput input { get; }
    public IAudio audio { get; }

    private readonly Stopwatch _clock = new();
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

            if(resources.loaded) {
                resources.Unload();
                game.Unload();
            }
            game.Load();
            resources.Load();
            RendererSettings rendererSettings = game.Loaded();

            if(renderer.open && renderer.Reset(rendererSettings))
                input.Reset();
            else
                Run(rendererSettings);
        }
        catch(Exception exception) {
            logger.Fatal(exception, "Uncaught exception! Please, report this file to the developer of the game.");
            throw;
        }
    }

    public void IncrementalReload() {
        logger.Info("Starting incremental reload");
        resources.Reload();
        RendererSettings rendererSettings = game.Loaded();
        if(renderer.open && renderer.Reset(rendererSettings))
            input.Reset();
    }

    private void Run(RendererSettings rendererSettings) {
        logger.Info("Starting game");
        Setup(rendererSettings);
        while(Update(_clock.time)) { }
        Finish();
    }

    private void Setup(RendererSettings rendererSettings) {
        _clock.Reset();

        renderer.Setup(rendererSettings);
        input.Reset();
        game.Setup();

        logger.Info("Setup finished");
    }

    private bool Update(TimeSpan time) {
        input.Update();
        renderer.Clear();
        renderer.Update();
        game.Update(time);
        TryTick(time);
        renderer.Draw();
        frameTime.Update(time);
        return renderer.open;
    }

    private void TryTick(TimeSpan time) {
        if(tickInterval < TimeSpan.Zero) {
            _lastTickTime = time;
            return;
        }

        while(time - _lastTickTime >= tickInterval) {
            _lastTickTime += tickInterval;
            Tick(_lastTickTime);
        }
    }

    private void Tick(TimeSpan time) => game.Tick(time);

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
