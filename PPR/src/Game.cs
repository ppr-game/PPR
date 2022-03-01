using System.Collections.Immutable;
using System.Globalization;

using PER.Abstractions;
using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;
using PER.Util;

using PPR.Effects;
using PPR.Resources;
using PPR.Screens;

namespace PPR;

public class Game : IGame {
    private int _fps;
    private int _avgFPS;
    private int _tempAvgFPS;
    private int _tempAvgFPSCounter;

    private const string SettingsPath = "config.json";
    private Settings _settings = new();

    private DrawTextEffect? _drawTextEffect;
    private BloomEffect? _bloomEffect;
    private GlitchEffect? _glitchEffect;

    private IScreen? _currentScreen;

    public void Unload() => _settings.Save(SettingsPath);

    public void Load() {
        IResources resources = Core.engine.resources;

        _settings = Settings.Load(SettingsPath);

        ImmutableDictionary<string, ResourcePackData> availablePacks =
            resources.GetAvailablePacks().ToImmutableDictionary(data => data.name);
        foreach(string packName in _settings.packs) {
            if(!availablePacks.TryGetValue(packName, out ResourcePackData data))
                continue;
            resources.TryAddPack(data);
        }

        resources.TryAddResource("audio", new AudioResources());

        resources.TryAddResource("graphics/icon", new IconResource());
        resources.TryAddResource("graphics/font", new FontResource());
        resources.TryAddResource("graphics/effects/bloom", new BloomEffect());

        resources.TryAddResource("graphics/colors", new ColorsResource());

        resources.TryAddResource("graphics/layouts/mainMenu", new MainMenuScreen());
    }

    public void Loaded() {
        if(!Core.engine.resources.TryGetResource("graphics/font", out FontResource? font) ||
           font?.font is null) return;
        Core.engine.resources.TryGetResource("graphics/icon", out IconResource? icon);

        _drawTextEffect = new DrawTextEffect();
        _glitchEffect = new GlitchEffect();

        Core.engine.resources.TryGetResource("graphics/effects/bloom", out _bloomEffect);

        Core.engine.renderer.formattingEffects.Clear();
        Core.engine.renderer.formattingEffects.Add("NONE", null);
        Core.engine.renderer.formattingEffects.Add("GLITCH", _glitchEffect);

        _settings.Apply();

        RendererSettings rendererSettings = new() {
            title = "Press Press Revolution",
            width = 80,
            height = 60,
            framerate = _settings.framerate,
            fullscreen = _settings.fullscreen,
            font = font.font,
            icon = icon?.icon
        };

        if(Core.engine.renderer.open) Core.engine.renderer.Reset(rendererSettings);
        else Core.engine.Start(rendererSettings);
    }

    public void Setup() {
        IRenderer renderer = Core.engine.renderer;
        renderer.closed += (_, _) => renderer.Close();
        if(!Core.engine.resources.TryGetResource("graphics/layouts/mainMenu", out MainMenuScreen? mainMenuScreen))
            return;
        _currentScreen = mainMenuScreen;
        _currentScreen?.Enter();
    }

    public void Update() {
        _fps = (int)Math.Round(1d / Core.engine.deltaTime);
        _tempAvgFPS += _fps;
        _tempAvgFPSCounter++;
        if(_tempAvgFPSCounter >= _avgFPS) {
            _avgFPS = _tempAvgFPS / _tempAvgFPSCounter;
            _tempAvgFPS = 0;
            _tempAvgFPSCounter = 0;
        }

        if(_drawTextEffect is null || _bloomEffect is null) return;

        IRenderer renderer = Core.engine.renderer;

        renderer.AddEffect(_drawTextEffect);
        renderer.AddEffect(_bloomEffect);

        renderer.DrawText(new Vector2Int(renderer.width - 1, renderer.height - 1),
            $"{_fps.ToString(CultureInfo.InvariantCulture)}/{_avgFPS.ToString(CultureInfo.InvariantCulture)} FPS",
            Color.white, Color.transparent, HorizontalAlignment.Right);

        _currentScreen?.Update();
    }

    public void Tick() => _currentScreen?.Tick();

    public void Finish() { }
}
