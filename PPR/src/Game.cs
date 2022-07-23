using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Common;
using PER.Common.Effects;
using PER.Common.Resources;
using PER.Util;

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

    protected override double deltaTime => _settings.showFps ? Core.engine.deltaTime : 0d;
    protected override IRenderer renderer => Core.engine.renderer;

    public override void Unload() => _settings.Save(SettingsPath);

    public override void Load() {
        IResources resources = Core.engine.resources;

        _settings = Settings.Load(SettingsPath);

        resources.TryAddPacksByNames(_settings.packs);

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

        resources.TryAddResource(DialogBoxPaletteResource.GlobalId, new DialogBoxPaletteResource());

        resources.TryAddResource(MainMenuScreen.GlobalId, new MainMenuScreen());
        resources.TryAddResource(LevelSelectScreen.GlobalId, new LevelSelectScreen(resources));
        resources.TryAddResource(NewLevelDialogBoxScreen.GlobalId, new NewLevelDialogBoxScreen());
        resources.TryAddResource(SettingsScreen.GlobalId, new SettingsScreen(_settings, resources));
    }

    public override RendererSettings Loaded() {
        if(!Core.engine.resources.TryGetResource(FontResource.GlobalId, out FontResource? font) || font.font is null)
            throw new InvalidOperationException("Missing font.");
        Core.engine.resources.TryGetResource(IconResource.GlobalId, out IconResource? icon);

        if(!Core.engine.resources.TryGetResource(ColorsResource.GlobalId, out ColorsResource? colors) ||
            !colors.colors.TryGetValue("background", out Color backgroundColor))
            throw new InvalidOperationException("Missing colors or background color.");
        renderer.background = backgroundColor;

        Core.engine.resources.TryGetResource(BloomEffect.GlobalId, out _bloomEffect);

        _settings.ApplyVolumes();
        Conductor.Start();

        return new RendererSettings {
            title = "Press Press Revolution",
            width = 80,
            height = 60,
            framerate = _settings.fpsLimit,
            fullscreen = _settings.fullscreen,
            font = font.font,
            icon = icon?.icon
        };
    }

    public override void Setup() {
        base.Setup();
        _settings.ApplyVolumes(); // apply volumes again because the first time the window isn't created yet
        Core.engine.renderer.focusChanged += (_, _) => _settings.ApplyVolumes();
        if(!Core.engine.resources.TryGetResource(MainMenuScreen.GlobalId, out MainMenuScreen? screen))
            return;
        SwitchScreen(screen);
    }

    public override void Update(TimeSpan time) {
        if(_drawTextEffect is not null) renderer.AddEffect(_drawTextEffect);
        if(_settings.bloom && _bloomEffect is not null) renderer.AddEffect(_bloomEffect);

        Conductor.Update();
        base.Update(time);
    }
}
