using System.Collections.Immutable;
using System.Globalization;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Util;

using PRR.UI;
using PRR.UI.Resources;

namespace PPR.Screens;

public class SettingsScreen : MenuWithCoolBackgroundAnimationScreenResourceBase {
    public const string GlobalId = "layouts/settings";

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInput input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "settings";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "frameLeft", typeof(LayoutResourceText) },
        { "frameRight", typeof(LayoutResourceText) },
        { "header.audio", typeof(LayoutResourceText) },
        { "volume.master.value", typeof(LayoutResourceText) },
        { "volume.master", typeof(LayoutResourceSlider) },
        { "volume.master.label", typeof(LayoutResourceText) },
        { "volume.unfocusedMaster.value", typeof(LayoutResourceText) },
        { "volume.unfocusedMaster", typeof(LayoutResourceSlider) },
        { "volume.unfocusedMaster.label", typeof(LayoutResourceText) },
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
        { "packs", typeof(LayoutResourceListBox<ResourcePackData>) },
        { "back", typeof(LayoutResourceButton) }
    };

    protected override IEnumerable<KeyValuePair<string, Type>> dependencyTypes {
        get {
            foreach(KeyValuePair<string, Type> pair in base.dependencyTypes)
                yield return pair;
            yield return new KeyValuePair<string, Type>(ResourcePackSelectorTemplate.GlobalId,
                typeof(ResourcePackSelectorTemplate));
        }
    }

    protected override IEnumerable<KeyValuePair<string, string>> paths {
        get {
            foreach(KeyValuePair<string, string> pair in base.paths)
                yield return pair;
            yield return new KeyValuePair<string, string>("frameLeft.text", $"{layoutsPath}/{layoutName}Left.txt");
            yield return new KeyValuePair<string, string>("frameRight.text", $"{layoutsPath}/{layoutName}Right.txt");
        }
    }

    private bool _reload;

    private readonly Settings _settings;

    private IList<ResourcePackData> _availablePacks = ImmutableList<ResourcePackData>.Empty;
    private ISet<ResourcePackData> _loadedPacks = ImmutableHashSet<ResourcePackData>.Empty;

    public SettingsScreen(Settings settings, IResources resources) {
        _settings = settings;
        resources.TryAddResource(ResourcePackSelectorTemplate.GlobalId, new ResourcePackSelectorTemplate());
    }

    public override void Load(string id) {
        base.Load(id);

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

        Slider volumeUnfocusedMaster = GetElement<Slider>("volume.unfocusedMaster");
        volumeUnfocusedMaster.onValueChanged += (_, _) => {
            _settings.unfocusedMasterVolume = volumeUnfocusedMaster.value;
            GetElement<Text>("volume.unfocusedMaster.value").text =
                _settings.unfocusedMasterVolume.ToString(culture);
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
        base.Open();

        GetElement<Slider>("volume.master").value = _settings.masterVolume;
        GetElement<Slider>("volume.unfocusedMaster").value = _settings.unfocusedMasterVolume;
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
        _loadedPacks = Core.engine.resources.loadedPacks.ToHashSet();
        List<ResourcePackData> availablePacks = new();
        availablePacks.AddRange(_loadedPacks);
        availablePacks.AddRange(Core.engine.resources.GetUnloadedAvailablePacks().Reverse());
        _availablePacks = availablePacks;
        GeneratePacksList();
    }

    private void UpdatePacks() {
        _settings.packs = _availablePacks.Where(_loadedPacks.Contains).Select(packData => packData.name).ToArray();
        _reload = true;
        GeneratePacksList();
    }

    private void GeneratePacksList() {
        ListBox<ResourcePackData> packs = GetElement<ListBox<ResourcePackData>>("packs");
        packs.Clear();
        foreach(ResourcePackData item in _availablePacks)
            packs.Add(item);
    }

    private class ResourcePackSelectorTemplate : ListBoxTemplateResourceBase<ResourcePackData> {
        // ok and?
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public const string GlobalId = "layouts/templates/resourcePackItem";

        protected override IRenderer renderer => Core.engine.renderer;
        protected override IInput input => Core.engine.input;
        protected override IAudio audio => Core.engine.audio;
        protected override string layoutName => "resourcePackItem";
        protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
            { "toggle", typeof(LayoutResourceButton) },
            { "up", typeof(LayoutResourceButton) },
            { "down", typeof(LayoutResourceButton) }
        };

        protected override IEnumerable<KeyValuePair<string, Type>> dependencyTypes {
            get {
                foreach(KeyValuePair<string, Type> pair in base.dependencyTypes)
                    yield return pair;
                yield return new KeyValuePair<string, Type>(SettingsScreen.GlobalId, typeof(SettingsScreen));
            }
        }

        private SettingsScreen? _screen;

        public override void Load(string id) {
            base.Load(id);
            _screen = GetDependency<SettingsScreen>(SettingsScreen.GlobalId);
        }

        private class Template : TemplateBase {
            private readonly ResourcePackSelectorTemplate _resource;

            public Template(ResourcePackSelectorTemplate resource) : base(resource) => _resource = resource;

            public override void UpdateWithItem(int index, ResourcePackData item, int width) {
                if(_resource._screen is null)
                    return;
                SettingsScreen screen = _resource._screen;

                int maxY = screen._availablePacks.Count - 1;
                int y = maxY - index;

                string name = item.name;
                bool loaded = screen._loadedPacks.Contains(item);
                bool canUnload = screen._loadedPacks.Count > 1 && name != Core.engine.resources.defaultPackName;

                bool canToggle = canUnload || !loaded;
                bool canMoveUp = y > 0 && name != Core.engine.resources.defaultPackName;
                bool canMoveDown = y < maxY &&
                    screen._availablePacks[index - 1].name != Core.engine.resources.defaultPackName;

                Button toggleButton = GetElement<Button>("toggle");
                toggleButton.text =
                    item.name.Length > toggleButton.size.x ? item.name[..toggleButton.size.x] : item.name;
                toggleButton.toggled = loaded;
                toggleButton.onClick += (_, _) => {
                    if(!canToggle)
                        return;
                    if(loaded)
                        screen._loadedPacks.Remove(item);
                    else
                        screen._loadedPacks.Add(item);
                    screen.UpdatePacks();
                };
                toggleButton.onHover += (_, _) => {
                    screen.GetElement<Text>("pack.description").text = item.meta.description;
                };

                Button moveUpButton = GetElement<Button>("up");
                moveUpButton.active = canMoveUp;
                moveUpButton.onClick += (_, _) => {
                    screen._availablePacks.RemoveAt(index);
                    screen._availablePacks.Insert(index + 1, item);
                    screen.UpdatePacks();
                };

                Button moveDownButton = GetElement<Button>("down");
                moveDownButton.active = canMoveDown;
                moveDownButton.onClick += (_, _) => {
                    screen._availablePacks.RemoveAt(index);
                    screen._availablePacks.Insert(index - 1, item);
                    screen.UpdatePacks();
                };
            }

            public override void MoveTo(Vector2Int origin, int index) {
                if(_resource._screen is null) {
                    base.MoveTo(origin, index);
                    return;
                }
                SettingsScreen screen = _resource._screen;
                int maxY = screen._availablePacks.Count - 1;
                int y = maxY - index;
                y *= height;
                foreach((string id, Element element) in idElements)
                    element.position = _resource.GetElement(id).position + origin + new Vector2Int(0, y);
            }
        }

        public override IListBoxTemplateFactory<ResourcePackData>.Template CreateTemplate() => new Template(this);
    }

    public override void Close() {
        base.Close();
        _reload = false;
    }

    public override void Update(TimeSpan time) {
        base.Update(time);
        foreach((string _, Element element) in elements)
            element.Update(time);
    }

    public override void Tick(TimeSpan time) { }
}
