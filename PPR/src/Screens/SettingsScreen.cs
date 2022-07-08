using System.Globalization;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Util;

using PRR.UI;
using PRR.UI.Resources;

namespace PPR.Screens;

public class SettingsScreen : ScreenResourceBase {
    public const string GlobalId = "layouts/settings";

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInput input => Core.engine.input;
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
        { "header.packs", typeof(LayoutResourceText) },
        { "pack.description", typeof(LayoutResourceText) },
        { "packs", typeof(LayoutResourceScrollablePanel) },
        { "template_pack.toggle", typeof(LayoutResourceButton) },
        { "template_pack.up", typeof(LayoutResourceButton) },
        { "template_pack.down", typeof(LayoutResourceButton) },
        { "back", typeof(LayoutResourceButton) }
    };

    private bool _reload;

    private readonly Settings _settings;

    public SettingsScreen(Settings settings) => _settings = settings;

    public override bool Load(string id, IResources resources) {
        if(!base.Load(id, resources)) return false;

        LoadAudio();
        LoadVideo();
        LoadAdvanced();

        if(elements["back"] is Button back) back.onClick += (_, _) => {
            if(_reload) Core.engine.Reload();
            if(Core.engine.resources.TryGetResource(MainMenuScreen.GlobalId, out MainMenuScreen? screen))
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
            _reload = true;
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

        if(elements["pack.description"] is Text text) text.text = "";
        OpenPacks();

        _reload = false;
    }

    private void OpenPacks() {
        if(elements["packs"] is not ScrollablePanel panel ||
           elements["template_pack.toggle"] is not Button toggleTemplate ||
           elements["template_pack.up"] is not Button upTemplate ||
           elements["template_pack.down"] is not Button downTemplate) return;
        toggleTemplate.enabled = false;
        upTemplate.enabled = false;
        downTemplate.enabled = false;

        GenerateResourcePackSelector(panel, toggleTemplate, upTemplate, downTemplate,
            Core.engine.resources.GetUnloadedAvailablePacks(),
            Core.engine.resources.loadedPacks);
    }

    private void GenerateResourcePackSelector(ScrollablePanel panel, Button toggleTemplate, Button upTemplate,
        Button downTemplate, IEnumerable<ResourcePackData> unloadedPacks, IEnumerable<ResourcePackData> loadedPacks) {
        List<ResourcePackData> availablePacks = new();
        availablePacks.AddRange(loadedPacks);
        availablePacks.AddRange(unloadedPacks.Reverse());
        GenerateResourcePackSelector(panel, toggleTemplate, upTemplate, downTemplate, availablePacks,
            loadedPacks.ToHashSet());
    }

    private void GenerateResourcePackSelector(ScrollablePanel panel, Button toggleTemplate, Button upTemplate,
        Button downTemplate, IList<ResourcePackData> availablePacks, ISet<ResourcePackData> loadedPacks) {
        panel.elements.Clear();

        int maxY = availablePacks.Count - 1;
        for(int i = 0; i < availablePacks.Count; i++) {
            int y = maxY - i;

            ResourcePackData current = availablePacks[i];
            string name = current.name;
            bool loaded = loadedPacks.Contains(current);
            bool canUnload = loadedPacks.Count > 1 && name != Core.engine.resources.defaultPackName;

            bool canToggle = canUnload || !loaded;
            bool canMoveUp = y > 0 && name != Core.engine.resources.defaultPackName;
            bool canMoveDown = y < maxY && availablePacks[i - 1].name != Core.engine.resources.defaultPackName;

            (Button toggleButton, Button moveUpButton, Button moveDownButton) =
                CreatePackListEntryButtons(i, panel, toggleTemplate, upTemplate, downTemplate, y, availablePacks, loadedPacks,
                    current, loaded, canToggle, canMoveUp, canMoveDown);

            panel.elements.Add(toggleButton);
            panel.elements.Add(moveUpButton);
            panel.elements.Add(moveDownButton);
        }
    }

    private (Button toggleButton, Button moveUpButton, Button moveDownButton) CreatePackListEntryButtons(int index,
        ScrollablePanel panel, Button toggleTemplate, Button upTemplate, Button downTemplate, int y,
        IList<ResourcePackData> availablePacks, ISet<ResourcePackData> loadedPacks, ResourcePackData pack, bool loaded,
        bool canToggle, bool canMoveUp, bool canMoveDown) {
        int height = Math.Max(Math.Max(toggleTemplate.size.y, upTemplate.size.y), downTemplate.size.y);

        Button toggleButton = new(renderer, Core.engine.input, Core.engine.audio) {
            position = panel.position + toggleTemplate.position + new Vector2Int(0, y * height + panel.scroll),
            size = toggleTemplate.size,
            text = pack.name.Length > toggleTemplate.size.x ? pack.name[..toggleTemplate.size.x] : pack.name,
            style = toggleTemplate.style,
            inactiveColor = toggleTemplate.inactiveColor,
            idleColor = toggleTemplate.idleColor,
            hoverColor = toggleTemplate.hoverColor,
            clickColor = toggleTemplate.clickColor,
            toggled = loaded
        };
        toggleButton.onClick += (_, _) => {
            if(!canToggle) return;
            if(loaded) loadedPacks.Remove(pack);
            else loadedPacks.Add(pack);
            UpdatePacks();
        };
        toggleButton.onHover += (_, _) => {
            if(elements["pack.description"] is Text text)
                text.text = pack.meta.description;
        };

        Button moveUpButton = new(renderer, Core.engine.input, Core.engine.audio) {
            position = panel.position + upTemplate.position + new Vector2Int(0, y * height + panel.scroll),
            size = upTemplate.size,
            text = upTemplate.text,
            style = upTemplate.style,
            inactiveColor = upTemplate.inactiveColor,
            idleColor = upTemplate.idleColor,
            hoverColor = upTemplate.hoverColor,
            clickColor = upTemplate.clickColor,
            active = canMoveUp
        };
        moveUpButton.onClick += (_, _) => {
            availablePacks.RemoveAt(index);
            availablePacks.Insert(index + 1, pack);
            UpdatePacks();
        };

        Button moveDownButton = new(renderer, Core.engine.input, Core.engine.audio) {
            position = panel.position + downTemplate.position + new Vector2Int(0, y * height + panel.scroll),
            size = downTemplate.size,
            text = downTemplate.text,
            style = downTemplate.style,
            inactiveColor = downTemplate.inactiveColor,
            idleColor = downTemplate.idleColor,
            hoverColor = downTemplate.hoverColor,
            clickColor = downTemplate.clickColor,
            active = canMoveDown
        };
        moveDownButton.onClick += (_, _) => {
            availablePacks.RemoveAt(index);
            availablePacks.Insert(index - 1, pack);
            UpdatePacks();
        };

        return (toggleButton, moveUpButton, moveDownButton);

        void UpdatePacks() {
            _settings.packs = availablePacks.Where(loadedPacks.Contains).Select(packData => packData.name).ToArray();
            _reload = true;
            GenerateResourcePackSelector(panel, toggleTemplate, upTemplate, downTemplate, availablePacks, loadedPacks);
        }
    }

    public override void Close() => _reload = false;

    public override void Update() {
        foreach((string _, Element element) in elements)
            element.Update(Core.engine.clock);
    }

    public override void Tick() { }
}
