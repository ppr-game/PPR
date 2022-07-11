using System.Globalization;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Util;

using PRR.UI;

namespace PPR.Screens;

public class SettingsScreen : MenuWithCoolBackgroundAnimationScreenResourceBase {
    public const string GlobalId = "layouts/settings";

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInput input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "settings";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "frame", typeof(LayoutResourceText) },
        { "header.audio", typeof(LayoutResourceText) },
        { "volume.master.value", typeof(LayoutResourceText) },
        { "volume.master", typeof(LayoutResourceSlider) },
        { "volume.master.label", typeof(LayoutResourceText) },
        { "volume.music.value", typeof(LayoutResourceText) },
        { "volume.music", typeof(LayoutResourceSlider) },
        { "volume.music.label", typeof(LayoutResourceText) },
        { "volume.sfx.value", typeof(LayoutResourceText) },
        { "volume.sfx", typeof(LayoutResourceSlider) },
        { "volume.sfx.label", typeof(LayoutResourceText) },
        { "header.video", typeof(LayoutResourceText) },
        { "bloom", typeof(LayoutResourceButton) },
        { "fullscreen", typeof(LayoutResourceButton) },
        { "fpsLimit.value", typeof(LayoutResourceText) },
        { "fpsLimit", typeof(LayoutResourceSlider) },
        { "fpsLimit.label", typeof(LayoutResourceText) },
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

    public override void Load(string id, IResources resources) {
        base.Load(id, resources);

        LoadAudio();
        LoadVideo();
        LoadAdvanced();

        GetElement<Button>("back").onClick += (_, _) => {
            if(_reload)
                Core.engine.Reload();
            if(Core.engine.resources.TryGetResource(MainMenuScreen.GlobalId, out MainMenuScreen? screen))
                Core.engine.game.SwitchScreen(screen);
        };
    }

    // ReSharper disable once CognitiveComplexity
    private void LoadAudio() {
        CultureInfo culture = CultureInfo.InvariantCulture;

        Slider volumeMaster = GetElement<Slider>("volume.master");
        volumeMaster.onValueChanged += (_, _) => {
            _settings.masterVolume = volumeMaster.value;
            GetElement<Text>("volume.master.value").text = _settings.masterVolume.ToString(culture);
            _settings.ApplyVolumes();
        };

        Slider volumeMusic = GetElement<Slider>("volume.music");
        volumeMusic.onValueChanged += (_, _) => {
            _settings.musicVolume = volumeMusic.value;
            GetElement<Text>("volume.music.value").text = _settings.musicVolume.ToString(culture);
            _settings.ApplyVolumes();
        };

        Slider volumeSfx = GetElement<Slider>("volume.sfx");
        volumeSfx.onValueChanged += (_, _) => {
            _settings.sfxVolume = volumeSfx.value;
            GetElement<Text>("volume.sfx.value").text = _settings.sfxVolume.ToString(culture);
            _settings.ApplyVolumes();
        };
    }

    private void LoadVideo() {
        CultureInfo culture = CultureInfo.InvariantCulture;

        Button bloom = GetElement<Button>("bloom");
        bloom.onClick += (_, _) => {
            _settings.bloom = !_settings.bloom;
            bloom.toggled = _settings.bloom;
        };

        Button fullscreen = GetElement<Button>("fullscreen");
        fullscreen.onClick += (_, _) => {
            _settings.fullscreen = !_settings.fullscreen;
            fullscreen.toggled = _settings.fullscreen;
            _reload = true;
        };

        Slider fpsLimit = GetElement<Slider>("fpsLimit");
        fpsLimit.onValueChanged += (_, _) => {
            _settings.fpsLimit = (int)fpsLimit.value * 60;
            Core.engine.renderer.framerate = _settings.fpsLimit;
            GetElement<Text>("fpsLimit.value").text = (int)fpsLimit.value switch {
                (int)ReservedFramerates.Vsync => "VSync",
                (int)ReservedFramerates.Unlimited => "Unlimited",
                _ => _settings.fpsLimit.ToString(culture)
            };
        };

        Button uppercaseNotes = GetElement<Button>("uppercaseNotes");
        uppercaseNotes.onClick += (_, _) => {
            _settings.uppercaseNotes = !_settings.uppercaseNotes;
            uppercaseNotes.toggled = _settings.uppercaseNotes;
        };
    }

    private void LoadAdvanced() {
        Button showFps = GetElement<Button>("showFps");
        showFps.onClick += (_, _) => {
            _settings.showFps = !_settings.showFps;
            showFps.toggled = _settings.showFps;
        };
    }

    public override void Open() {
        GetElement<Slider>("volume.master").value = _settings.masterVolume;
        GetElement<Slider>("volume.music").value = _settings.musicVolume;
        GetElement<Slider>("volume.sfx").value = _settings.sfxVolume;
        GetElement<Button>("bloom").toggled = _settings.bloom;
        GetElement<Button>("fullscreen").toggled = _settings.fullscreen;
        GetElement<Slider>("fpsLimit").value = _settings.fpsLimit / 60f;
        GetElement<Button>("uppercaseNotes").toggled = _settings.uppercaseNotes;
        GetElement<Button>("showFps").toggled = _settings.showFps;

        GetElement<Text>("pack.description").text = "";
        OpenPacks();

        _reload = false;
    }

    private void OpenPacks() {
        ScrollablePanel panel = GetElement<ScrollablePanel>("packs");
        Button toggleTemplate = GetElement<Button>("template_pack.toggle");
        Button upTemplate = GetElement<Button>("template_pack.up");
        Button downTemplate = GetElement<Button>("template_pack.down");

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

        Button toggleButton = Button.Clone(toggleTemplate);
        toggleButton.enabled = true;
        toggleButton.position = panel.position + toggleTemplate.position + new Vector2Int(0, y * height + panel.scroll);
        toggleButton.text = pack.name.Length > toggleTemplate.size.x ? pack.name[..toggleTemplate.size.x] : pack.name;
        toggleButton.toggled = loaded;
        toggleButton.onClick += (_, _) => {
            if(!canToggle) return;
            if(loaded) loadedPacks.Remove(pack);
            else loadedPacks.Add(pack);
            UpdatePacks();
        };
        toggleButton.onHover += (_, _) => {
            GetElement<Text>("pack.description").text = pack.meta.description;
        };

        Button moveUpButton = Button.Clone(upTemplate);
        moveUpButton.enabled = true;
        moveUpButton.position = panel.position + upTemplate.position + new Vector2Int(0, y * height + panel.scroll);
        moveUpButton.active = canMoveUp;
        moveUpButton.onClick += (_, _) => {
            availablePacks.RemoveAt(index);
            availablePacks.Insert(index + 1, pack);
            UpdatePacks();
        };

        Button moveDownButton = Button.Clone(downTemplate);
        moveDownButton.enabled = true;
        moveDownButton.position = panel.position + downTemplate.position + new Vector2Int(0, y * height + panel.scroll);
        moveDownButton.active = canMoveDown;
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
        base.Update();
        foreach((string _, Element element) in elements)
            element.Update(Core.engine.clock);
    }

    public override void Tick() { }
}
