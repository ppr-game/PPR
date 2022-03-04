using System.Collections.Immutable;

using PER.Abstractions;
using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;
using PER.Common;
using PER.Common.Effects;
using PER.Common.Resources;

using PPR.Resources;
using PPR.Screens;

using PRR.Resources;

namespace PPR;

public class Game : GameBase {
    private const string SettingsPath = "config.json";
    private Settings _settings = new();

    private DrawTextEffect? _drawTextEffect;
    private BloomEffect? _bloomEffect;
    private GlitchEffect? _glitchEffect;

    protected override double deltaTime => Core.engine.deltaTime;
    protected override IRenderer renderer => Core.engine.renderer;

    public override void Unload() => _settings.Save(SettingsPath);

    public override void Load() {
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

        resources.TryAddResource(ColorsResource.GlobalId, new ColorsResource());

        _drawTextEffect = new DrawTextEffect();
        resources.TryAddResource(BloomEffect.GlobalId, new BloomEffect());
        _glitchEffect = new GlitchEffect(renderer);

        renderer.formattingEffects.Clear();
        renderer.formattingEffects.Add("none", null);
        renderer.formattingEffects.Add("glitch", _glitchEffect);

        resources.TryAddResource(MainMenuScreen.GlobalId, new MainMenuScreen());
        resources.TryAddResource(SettingsScreen.GlobalId, new SettingsScreen());
    }

    public override void Loaded() {
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

        if(renderer.open) renderer.Reset(rendererSettings);
        else Core.engine.Start(rendererSettings);
    }

    public override void Setup() {
        base.Setup();
        if(!Core.engine.resources.TryGetResource(MainMenuScreen.GlobalId, out MainMenuScreen? screen))
            return;
        SwitchScreen(screen);
    }

    public override void Update() {
        if(_drawTextEffect is not null) renderer.AddEffect(_drawTextEffect);
        if(_bloomEffect is not null) renderer.AddEffect(_bloomEffect);

        base.Update();
    }
}
