using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

using PER.Abstractions;
using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Demo.Effects;
using PER.Demo.Resources;
using PER.Util;

using PRR.UI;

namespace PER.Demo;

public class Game : IGame {
    private int _fps;
    private int _avgFPS;
    private int _tempAvgFPS;
    private int _tempAvgFPSCounter;

    private const string SettingsPath = "config.json";
    private Settings _settings = new();

    private Dictionary<string, Color> _colors = new();

    private DrawTextEffect? _drawTextEffect;
    private BloomEffect? _bloomEffect;
    private GlitchEffect? _glitchEffect;

    private readonly List<Element> _ui = new();
    private readonly List<Element> _packSelector = new();
    private ProgressBar? _testProgressBar;

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

        resources.TryAddResource("graphics/font", new FontResource());
        resources.TryAddResource("graphics/effects/bloom", new BloomEffect());
        resources.TryAddResource("graphics/effects/maxBlendBloom", new MaxBlendBloomEffect());

        resources.TryAddResource("graphics/colors", new ColorsResource());
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

        if(Core.engine.resources.TryGetResource("graphics/colors", out ColorsResource? colors))
            _colors = colors!.colors;

        _settings.Apply();

        if(Core.engine.renderer.open) Core.engine.renderer.font = font.font;
        else {
            Core.engine.Start(new RendererSettings {
                title = "PER Demo Pog",
                width = 80,
                height = 60,
                framerate = 0,
                fullscreen = false,
                font = font.font,
                icon = icon?.icon
            });
        }
    }

    public void Setup() {
        IRenderer renderer = Core.engine.renderer;
        IInputManager input = Core.engine.input;
        IAudio audio = Core.engine.audio;

        renderer.closed += (_, _) => renderer.Close();

        _ui.Add(new Panel(renderer) {
            enabled = false,
            position = new Vector2Int(30, 30),
            size = new Vector2Int(10, 3),
            character = new RenderCharacter('a', Color.transparent, Color.white)
        });

        _ui.Add(new Text(renderer) { position = new Vector2Int(0, 30), text = "hi ui test" });

        Button testButton1 = new(renderer, input, audio) {
            position = new Vector2Int(0, 32),
            size = new Vector2Int(6, 1),
            text = "button",
            idleColor = _colors["button_mainMenu.play_idle"],
            hoverColor = _colors["button_mainMenu.play_hover"],
            clickColor = _colors["button_mainMenu.play_click"]
        };
        testButton1.onClick += (_, _) => {
            testButton1.toggled = !testButton1.toggled;
            _ui[0].enabled = testButton1.toggled;
        };
        _ui.Add(testButton1);

        int counter = 0;
        Button testButton2 = new(renderer, input, audio) {
            position = new Vector2Int(0, 34),
            size = new Vector2Int(6, 1),
            text = counter.ToString()
        };
        testButton2.onClick += (_, _) => {
            counter++;
            testButton2.text = counter.ToString();
        };
        _ui.Add(testButton2);

        _ui.Add(new Button(renderer, input, audio) {
            position = new Vector2Int(0, 36),
            size = new Vector2Int(16, 2),
            text = "big\nbutton"
        });

        _ui.Add(new Button(renderer, input, audio) {
            active = false, position = new Vector2Int(0, 39),
            size = new Vector2Int(16, 2),
            text = "big inactive\nbutton"
        });

        _ui.Add(new Button(renderer, input, audio) {
            active = false,
            toggled = true,
            position = new Vector2Int(0, 42),
            size = new Vector2Int(16, 1),
            text = "inactive toggled"
        });

        _ui.Add(new Button(renderer, input, audio) {
            position = new Vector2Int(0, 44),
            size = new Vector2Int(16, 2),
            text = "big glitch\nbutton",
            effect = _glitchEffect
        });

        Text testSliderText = new(renderer) {
            position = new Vector2Int(21, 47),
            text = ":troll:"
        };
        _ui.Add(testSliderText);

        Slider testSlider = new(renderer, input, audio) {
            position = new Vector2Int(0, 47),
            width = 21,
            minValue = 0f,
            maxValue = 1f
        };
        testSlider.onValueChanged += (_, _) => {
            testSliderText.text = testSlider.value.ToString(CultureInfo.InvariantCulture);
            _settings.volume = testSlider.value;
        };
        testSlider.value = _settings.volume;
        _ui.Add(testSlider);

        Button packsButton = new(renderer, input, audio) {
            position = new Vector2Int(0, 49),
            size = new Vector2Int(5, 1),
            text = "packs",
            toggled = false
        };
        packsButton.onClick += (_, _) => {
            packsButton.toggled = !packsButton.toggled;
            if(packsButton.toggled)
                GenerateResourcePackSelector(new Vector2Int(30, 20), 30,
                    Core.engine.resources.GetUnloadedAvailablePacks().Select(data => data.name),
                    Core.engine.resources.loadedPacks.Select(data => data.name));
            else _packSelector.Clear();
        };
        _ui.Add(packsButton);

        Button applyButton = new(renderer, input, audio) {
            position = new Vector2Int(0, 51),
            size = new Vector2Int(5, 1),
            text = "apply"
        };
        applyButton.onClick += (_, _) => {
            _settings.Apply();
        };
        _ui.Add(applyButton);

        Button reloadButton = new(renderer, input, audio) {
            position = new Vector2Int(6, 51),
            size = new Vector2Int(6, 1),
            text = "reload"
        };
        reloadButton.onClick += (_, _) => {
            Core.engine.Reload();
        };
        _ui.Add(reloadButton);

        _testProgressBar = new ProgressBar(renderer) {
            position = new Vector2Int(0, 58),
            size = new Vector2Int(80, 2)
        };
        _ui.Add(_testProgressBar);
    }

    private void GenerateResourcePackSelector(Vector2Int position, int width, IEnumerable<string> unloadedPacks,
        IEnumerable<string> loadedPacks) {
        List<string> availablePacks = new();
        availablePacks.AddRange(loadedPacks);
        availablePacks.AddRange(unloadedPacks.Reverse());
        GenerateResourcePackSelector(position, width, availablePacks, loadedPacks.ToHashSet());
    }

    private void GenerateResourcePackSelector(Vector2Int position, int width, IList<string> availablePacks,
        ISet<string> loadedPacks) {
        _packSelector.Clear();

        int maxY = availablePacks.Count - 1;
        int y = maxY;

        for(int i = 0; i < availablePacks.Count; i++) {
            string name = availablePacks[i];
            bool loaded = loadedPacks.Contains(name);
            bool canUnload = loadedPacks.Count > 1 && name != Core.engine.resources.defaultPackName;

            bool canToggle = canUnload || !loaded;
            bool canMoveUp = y > 0 && name != Core.engine.resources.defaultPackName;
            bool canMoveDown = y < maxY && availablePacks[i - 1] != Core.engine.resources.defaultPackName;

            (Button toggleButton, Button moveUpButton, Button moveDownButton) =
                CreatePackListEntryButtons(i, position, y, width, availablePacks, loadedPacks, name, loaded,
                    canToggle, canMoveUp, canMoveDown);

            _packSelector.Add(toggleButton);
            _packSelector.Add(moveUpButton);
            _packSelector.Add(moveDownButton);

            y--;
        }

        Button applyButton = new(Core.engine.renderer, Core.engine.input, Core.engine.audio) {
            position = position + new Vector2Int(0, maxY + 2),
            size = new Vector2Int(width, 1),
            text = "apply"
        };
        applyButton.onClick += (_, _) => {
            _settings.packs = availablePacks.Where(loadedPacks.Contains).ToArray();
        };
        _packSelector.Add(applyButton);
    }

    private (Button toggleButton, Button moveUpButton, Button moveDownButton) CreatePackListEntryButtons(int index,
        Vector2Int position, int y, int width, IList<string> availablePacks, ISet<string> loadedPacks,
        string name, bool loaded, bool canToggle, bool canMoveUp, bool canMoveDown) {
        Button toggleButton = new(Core.engine.renderer, Core.engine.input, Core.engine.audio) {
            position = position + new Vector2Int(0, y),
            size = new Vector2Int(width - 2, 1),
            text = name,
            toggled = loaded,
            active = canToggle
        };
        toggleButton.onClick += (_, _) => {
            if(loaded) loadedPacks.Remove(name);
            else loadedPacks.Add(name);
            GenerateResourcePackSelector(position, width, availablePacks, loadedPacks);
        };

        Button moveUpButton = new(Core.engine.renderer, Core.engine.input, Core.engine.audio) {
            position = position + new Vector2Int(width - 2, y),
            size = new Vector2Int(1, 1),
            text = "▲",
            active = canMoveUp
        };
        moveUpButton.onClick += (_, _) => {
            availablePacks.RemoveAt(index);
            availablePacks.Insert(index + 1, name);
            GenerateResourcePackSelector(position, width, availablePacks, loadedPacks);
        };

        Button moveDownButton = new(Core.engine.renderer, Core.engine.input, Core.engine.audio) {
            position = position + new Vector2Int(width - 1, y),
            size = new Vector2Int(1, 1),
            text = "▼",
            active = canMoveDown
        };
        moveDownButton.onClick += (_, _) => {
            availablePacks.RemoveAt(index);
            availablePacks.Insert(index - 1, name);
            GenerateResourcePackSelector(position, width, availablePacks, loadedPacks);
        };

        return (toggleButton, moveUpButton, moveDownButton);
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
        IInputManager input = Core.engine.input;

        renderer.AddEffect(_drawTextEffect);
        renderer.AddEffect(_bloomEffect);

        renderer.DrawText(new Vector2Int(0, 0),
            $"{_fps.ToString(CultureInfo.InvariantCulture)}/{_avgFPS.ToString(CultureInfo.InvariantCulture)} FPS",
            Color.white, Color.transparent);

        if(input.KeyPressed(KeyCode.F)) return;

        renderer.DrawText(new Vector2Int(0, 1),
            @"hello everyone! this is cf00FF00FFcbConfiGcfFFFFFFFFcb and today i'm gonna show you my eGLITCHeengineeNONEe!!
as you can see biuit works!!1!biu
cf000000FFbFF0000FFcthanks for watchingcfFFFFFFFFb00000000c everyone, uhit like, subscribe, good luck, bbye!!".Split('\n'),
            Color.white, Color.transparent);

        renderer.DrawText(new Vector2Int(0, 4),
            "more test", Color.black, new Color(0f, 1f, 0f, 1f));

        renderer.DrawText(new Vector2Int(0, 5),
            "ieven morei test", new Color(1f, 0f, 1f, 0.5f), new Color(0f, 1f, 0f, 1f));

        renderer.DrawText(new Vector2Int(10, 4),
            "per-text effects test", Color.white, Color.transparent, HorizontalAlignment.Left, RenderStyle.None,
            RenderOptions.Default, _glitchEffect);

        for(RenderStyle style = RenderStyle.None; style <= RenderStyle.All; style++) {
            renderer.DrawText(new Vector2Int(0, 6 + (int)style),
                "styles test", Color.white, Color.transparent, HorizontalAlignment.Left, style);
        }

        renderer.DrawText(new Vector2Int(39, 6),
            "left test even", Color.white, Color.transparent);
        renderer.DrawText(new Vector2Int(39, 7),
            "left test odd", Color.white, Color.transparent);
        renderer.DrawText(new Vector2Int(39, 8),
            "middle test even", Color.white, Color.transparent, HorizontalAlignment.Middle);
        renderer.DrawText(new Vector2Int(39, 9),
            "middle test odd", Color.white, Color.transparent, HorizontalAlignment.Middle);
        renderer.DrawText(new Vector2Int(39, 10),
            "-right test even", Color.white, Color.transparent, HorizontalAlignment.Right);
        renderer.DrawText(new Vector2Int(39, 11),
            "-right test odd", Color.white, Color.transparent, HorizontalAlignment.Right);

        if(_testProgressBar is not null &&
           input.mousePosition.InBounds(_testProgressBar.bounds) &&
           input.MouseButtonPressed(MouseButton.Left)) {
            _testProgressBar.value = input.normalizedMousePosition.x;
        }

        DrawUi();
    }

    private void DrawUi() {
        foreach(Element element in _ui) element.Update(Core.engine.clock);
        for(int i = 0; i < _packSelector.Count; i++) {
            Element element = _packSelector[i];
            element.Update(Core.engine.clock);
        }
    }

    public void Tick() { }

    public void Finish() { }
}
