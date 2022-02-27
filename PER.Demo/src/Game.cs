using System;
using System.Collections.Generic;
using System.Globalization;

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

    private DrawTextEffect? _drawTextEffect;
    private BloomEffect? _bloomEffect;
    private GlitchEffect? _glitchEffect;

    private readonly List<Element> _ui = new();
    private ProgressBar? _testProgressBar;

    public void Unload() { }

    public void Load() {
        IResources resources = Core.engine.resources;

        if(Core.engine.renderer.open) {
            foreach(ResourcePackData packData in resources.GetAvailablePacks())
                resources.TryAddPack(packData);
        }
        else {
            foreach(ResourcePackData packData in resources.GetAvailablePacks()) {
                if(packData.name != "Default") continue;
                resources.TryAddPack(packData);
                break;
            }
        }

        resources.TryAddResource("audio", new AudioResources());

        resources.TryAddResource("graphics/font", new FontResource());
        resources.TryAddResource("graphics/effects/bloom", new BloomEffect());
        resources.TryAddResource("graphics/effects/maxBlendBloom", new BloomEffect());
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
        IAudio audio = Core.engine.audio;

        _ui.Add(new Panel(renderer) {
            enabled = false,
            position = new Vector2Int(30, 30),
            size = new Vector2Int(10, 3),
            character = new RenderCharacter('a', Color.transparent, Color.white)
        });

        _ui.Add(new Text(renderer) { position = new Vector2Int(0, 30), text = "hi ui test" });

        Button testButton1 = new(renderer) {
            audio = audio,
            position = new Vector2Int(0, 32),
            size = new Vector2Int(6, 1),
            text = "button"
        };
        testButton1.onClick += (_, _) => {
            testButton1.toggled = !testButton1.toggled;
            _ui[0].enabled = testButton1.toggled;
        };
        _ui.Add(testButton1);

        int counter = 0;
        Button testButton2 = new(renderer) {
            audio = audio,
            position = new Vector2Int(0, 34),
            size = new Vector2Int(6, 1),
            text = counter.ToString()
        };
        testButton2.onClick += (_, _) => {
            counter++;
            testButton2.text = counter.ToString();
        };
        _ui.Add(testButton2);

        _ui.Add(new Button(renderer) {
            audio = audio,
            position = new Vector2Int(0, 36),
            size = new Vector2Int(16, 2),
            text = "big\nbutton"
        });

        _ui.Add(new Button(renderer) {
            audio = audio,
            active = false, position = new Vector2Int(0, 39),
            size = new Vector2Int(16, 2),
            text = "big inactive\nbutton"
        });

        _ui.Add(new Button(renderer) {
            audio = audio,
            active = false,
            toggled = true,
            position = new Vector2Int(0, 42),
            size = new Vector2Int(16, 1),
            text = "inactive toggled"
        });

        _ui.Add(new Button(renderer) {
            audio = audio,
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

        Slider testSlider = new(renderer) {
            audio = audio,
            position = new Vector2Int(0, 47),
            width = 21,
            minValue = 0f,
            maxValue = 1f
        };
        testSlider.onValueChanged += (_, _) => {
            testSliderText.text = testSlider.value.ToString(CultureInfo.InvariantCulture);

            if(Core.engine.audio.TryGetMixer("master", out IAudioMixer? mixer))
                mixer.volume = testSlider.value;

            Core.engine.audio.UpdateVolumes();
        };
        testSlider.value = 0.2f;
        _ui.Add(testSlider);

        Button testButton3 = new(renderer) {
            audio = audio,
            position = new Vector2Int(0, 49),
            size = new Vector2Int(6, 1),
            text = "reload"
        };
        testButton3.onClick += (_, _) => {
            Core.engine.Reload();
        };
        _ui.Add(testButton3);

        _testProgressBar = new ProgressBar(renderer) {
            position = new Vector2Int(0, 58),
            size = new Vector2Int(80, 2)
        };
        _ui.Add(_testProgressBar);
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

        renderer.DrawText(new Vector2Int(0, 0),
            $"{_fps.ToString(CultureInfo.InvariantCulture)}/{_avgFPS.ToString(CultureInfo.InvariantCulture)} FPS",
            Color.white, Color.transparent);

        if(renderer.input?.KeyPressed(KeyCode.F) ?? false) return;

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
           (renderer.input?.mousePosition.InBounds(_testProgressBar.bounds) ?? false) &&
           renderer.input.MouseButtonPressed(MouseButton.Left)) {
            _testProgressBar.value = renderer.input.normalizedMousePosition.x;
        }

        DrawUi();
    }

    private void DrawUi() {
        foreach(Element element in _ui) element.Update(Core.engine.clock);
    }

    public void Tick() { }

    public void Finish() { }
}
