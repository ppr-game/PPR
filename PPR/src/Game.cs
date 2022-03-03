using System.Collections.Immutable;
using System.Globalization;

using PER.Abstractions;
using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;
using PER.Common.Effects;
using PER.Common.Resources;
using PER.Util;

using PPR.Resources;
using PPR.Screens;

using PRR.Resources;

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

        resources.TryAddResource(IconResource.GlobalId, new IconResource());
        resources.TryAddResource(FontResource.GlobalId, new FontResource());
        resources.TryAddResource(BloomEffect.GlobalId, new BloomEffect());

        resources.TryAddResource(ColorsResource.GlobalId, new ColorsResource());

        _drawTextEffect = new DrawTextEffect();
        _glitchEffect = new GlitchEffect(Core.engine.renderer);

        Core.engine.renderer.formattingEffects.Clear();
        Core.engine.renderer.formattingEffects.Add("none", null);
        Core.engine.renderer.formattingEffects.Add("glitch", _glitchEffect);

        resources.TryAddResource("layouts/mainMenu", new MainMenuScreen());
    }

    public void Loaded() {
        if(!Core.engine.resources.TryGetResource(FontResource.GlobalId, out FontResource? font) ||
           font?.font is null) return;
        Core.engine.resources.TryGetResource(IconResource.GlobalId, out IconResource? icon);

        Core.engine.resources.TryGetResource(BloomEffect.GlobalId, out _bloomEffect);

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
        if(!Core.engine.resources.TryGetResource("layouts/mainMenu", out MainMenuScreen? mainMenuScreen))
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

        _currentScreen?.Update();

        renderer.DrawText(new Vector2Int(renderer.width - 1, renderer.height - 1),
            $"{_fps.ToString(CultureInfo.InvariantCulture)}/{_avgFPS.ToString(CultureInfo.InvariantCulture)} FPS",
            _ => new Formatting(Color.white, Color.transparent), HorizontalAlignment.Right);
    }

    public void Tick() => _currentScreen?.Tick();

    public void Finish() { }
}
