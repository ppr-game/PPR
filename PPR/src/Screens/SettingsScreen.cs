using System.Globalization;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;

using PRR.UI;
using PRR.UI.Resources;

namespace PPR.Screens;

public class SettingsScreen : ScreenResourceBase {
    public const string GlobalId = "leyouts/setings";

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInputManager input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "settings";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "frame", typeof(LayoutResourceText) },
        { "header.audio", typeof(LayoutResourceText) },
        { "volume.master", typeof(LayoutResourceSlider) },
        { "volume.master.label", typeof(LayoutResourceText) },
        { "volume.master.value", typeof(LayoutResourceText) },
        { "volume.music", typeof(LayoutResourceSlider) },
        { "volume.music.label", typeof(LayoutResourceText) },
        { "volume.music.value", typeof(LayoutResourceText) },
        { "volume.sfx", typeof(LayoutResourceSlider) },
        { "volume.sfx.label", typeof(LayoutResourceText) },
        { "volume.sfx.value", typeof(LayoutResourceText) },
        { "header.video", typeof(LayoutResourceText) },
        { "bloom", typeof(LayoutResourceButton) },
        { "fullscreen", typeof(LayoutResourceButton) },
        { "fpsLimit", typeof(LayoutResourceSlider) },
        { "fpsLimit.label", typeof(LayoutResourceText) },
        { "fpsLimit.value", typeof(LayoutResourceText) },
        { "uppercaseNotes", typeof(LayoutResourceButton) },
        { "header.advanced", typeof(LayoutResourceText) },
        { "showFps", typeof(LayoutResourceButton) },
        { "back", typeof(LayoutResourceButton) },
        { "apply", typeof(LayoutResourceButton) }
    };

    private readonly Settings _settings;

    public SettingsScreen(Settings settings) => _settings = settings;

    public override bool Load(string id, IResources resources) {
        if(!base.Load(id, resources)) return false;

        LoadAudio();
        LoadVideo();
        LoadAdvanced();

        if(elements["back"] is Button back) back.onClick += (_, _) => {
            if(Core.engine.resources.TryGetResource(MainMenuScreen.GlobalId, out MainMenuScreen? screen))
                Core.engine.game.SwitchScreen(screen);
        };

        if(elements["apply"] is Button apply) apply.onClick += (_, _) => {
            Core.engine.Reload();
            if(Core.engine.resources.TryGetResource(GlobalId, out SettingsScreen? screen))
                Core.engine.game.SwitchScreen(screen);
        };

        return true;
    }

    // ReSharper disable once CognitiveComplexity
    private void LoadAudio() {
        CultureInfo culture = CultureInfo.InvariantCulture;

        if(elements["volume.master"] is Slider volumeMaster)
            volumeMaster.onValueChanged += (_, _) => {
                _settings.masterVolume = volumeMaster.value;
                if(elements["volume.master.value"] is Text label) label.text = _settings.masterVolume.ToString(culture);
                _settings.ApplyVolumes();
            };

        if(elements["volume.music"] is Slider volumeMusic)
            volumeMusic.onValueChanged += (_, _) => {
                _settings.musicVolume = volumeMusic.value;
                if(elements["volume.music.value"] is Text label) label.text = _settings.musicVolume.ToString(culture);
                _settings.ApplyVolumes();
            };

        if(elements["volume.sfx"] is Slider volumeSfx)
            volumeSfx.onValueChanged += (_, _) => {
                _settings.sfxVolume = volumeSfx.value;
                if(elements["volume.sfx.value"] is Text label) label.text = _settings.sfxVolume.ToString(culture);
                _settings.ApplyVolumes();
            };
    }

    private void LoadVideo() {
        CultureInfo culture = CultureInfo.InvariantCulture;

        if(elements["bloom"] is Button bloom) bloom.onClick += (_, _) => {
            _settings.bloom = !_settings.bloom;
            bloom.toggled = _settings.bloom;
        };

        if(elements["fullscreen"] is Button fullscreen) fullscreen.onClick += (_, _) => {
            _settings.fullscreen = !_settings.fullscreen;
            fullscreen.toggled = _settings.fullscreen;
            if(elements["apply"] is Button apply) apply.active = true;
        };

        if(elements["fpsLimit"] is Slider fpsLimit)
            fpsLimit.onValueChanged += (_, _) => {
                _settings.fpsLimit = (int)fpsLimit.value * 60;
                if(elements["fpsLimit.value"] is Text label)
                    label.text = (int)fpsLimit.value switch {
                        (int)ReservedFramerates.Vsync => "VSync",
                        (int)ReservedFramerates.Unlimited => "Unlimited",
                        _ => _settings.fpsLimit.ToString(culture)
                    };
                Core.engine.renderer.framerate = _settings.fpsLimit;
            };

        if(elements["uppercaseNotes"] is Button uppercaseNotes) uppercaseNotes.onClick += (_, _) => {
            _settings.uppercaseNotes = !_settings.uppercaseNotes;
            uppercaseNotes.toggled = _settings.uppercaseNotes;
        };
    }

    private void LoadAdvanced() {
        if(elements["showFps"] is Button showFps) showFps.onClick += (_, _) => {
            _settings.showFps = !_settings.showFps;
            showFps.toggled = _settings.showFps;
        };
    }

    public override void Open() {
        if(elements["volume.master"] is Slider volumeMaster) volumeMaster.value = _settings.masterVolume;
        if(elements["volume.music"] is Slider volumeMusic) volumeMusic.value = _settings.musicVolume;
        if(elements["volume.sfx"] is Slider volumeSfx) volumeSfx.value = _settings.sfxVolume;
        if(elements["bloom"] is Button bloom) bloom.toggled = _settings.bloom;
        if(elements["fullscreen"] is Button fullscreen) fullscreen.toggled = _settings.fullscreen;
        if(elements["fpsLimit"] is Slider fpsLimit) fpsLimit.value = _settings.fpsLimit / 60f;
        if(elements["uppercaseNotes"] is Button uppercaseNotes) uppercaseNotes.toggled = _settings.uppercaseNotes;
        if(elements["showFps"] is Button showFps) showFps.toggled = _settings.showFps;

        if(elements["apply"] is Button apply) apply.active = false;
    }

    public override void Close() { }

    public override void Update() {
        foreach((string _, Element element) in elements) element.Update(Core.engine.clock);
    }

    public override void Tick() { }
}
