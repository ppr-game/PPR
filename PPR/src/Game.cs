using PER.Abstractions;
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

public class Game : ScreenGame {
    private const string SettingsPath = "config.json";
    private Settings _settings = new();

    private DrawTextEffect? _drawTextEffect;
    private BloomEffect? _bloomEffect;
    private GlitchEffect? _glitchEffect;

    protected override FrameTime? frameTime => _settings.showFps ? Core.engine.frameTime : null;
    protected override IRenderer renderer => Core.engine.renderer;

    // 59 instead of 60 because time measuring isn't exactly perfect
    // so it may be a little more or less than 60 every frame
    private static readonly TimeSpan fpsGood = TimeSpan.FromSeconds(1d / 59d);
    private static readonly TimeSpan fpsOk = TimeSpan.FromSeconds(1d / 30d);

    private Color _fpsGoodColor;
    private Color _fpsOkColor;
    private Color _fpsBadColor;

    public override void Unload() => _settings.Save(SettingsPath);

    public override void Load() {
        IResources resources = Core.engine.resources;

        _settings = Settings.Load(SettingsPath);

        resources.TryAddPacksByNames(_settings.packs);

        resources.TryAddResource(AudioResources.GlobalId, new AudioResources());

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
        resources.TryAddResource(LevelSelectScreen.GlobalId, new LevelSelectScreen(_settings, resources));
        resources.TryAddResource(NewLevelDialogBoxScreen.GlobalId, new NewLevelDialogBoxScreen());
        resources.TryAddResource(EffectsWarningDialogBoxScreen.GlobalId, new EffectsWarningDialogBoxScreen());
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
        if(!colors.colors.TryGetValue("fps_good", out _fpsGoodColor))
            _fpsGoodColor = Color.white;
        if(!colors.colors.TryGetValue("fps_ok", out _fpsOkColor))
            _fpsOkColor = Color.white;
        if(!colors.colors.TryGetValue("fps_bad", out _fpsBadColor))
            _fpsBadColor = Color.white;

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

    protected override Formatting FrameTimeFormatter(FrameTime frameTime, char flag) => flag switch {
        '1' or 'a' => new Formatting(frameTime.frameTime > fpsOk ? _fpsBadColor :
            frameTime.frameTime > fpsGood ? _fpsOkColor : _fpsGoodColor, Color.transparent),
        '2' or 'b' => new Formatting(frameTime.averageFrameTime > fpsOk ? _fpsBadColor :
            frameTime.averageFrameTime > fpsGood ? _fpsOkColor : _fpsGoodColor, Color.transparent),
        _ => base.FrameTimeFormatter(frameTime, flag)
    };
}
