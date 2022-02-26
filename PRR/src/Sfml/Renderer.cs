using System;
using System.Collections.Generic;
using System.IO;

using PER.Abstractions.Renderer;
using PER.Util;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

using Shader = SFML.Graphics.Shader;

namespace PRR.Sfml;

public class Renderer : RendererBase, IDisposable {
    public override bool open => window?.IsOpen ?? false;
    public override bool focused => window?.HasFocus() ?? false;

    public Text? text { get; private set; }
    public RenderWindow? window { get; private set; }

    private readonly Dictionary<IEffect, CachedEffect> _cachedFullscreenEffects = new();

    private bool _swapTextures;

    private RenderTexture? currentRenderTexture => _swapTextures ? _additionalRenderTexture : _mainRenderTexture;
    private RenderTexture? otherRenderTexture => _swapTextures ? _mainRenderTexture : _additionalRenderTexture;
    private Sprite? currentSprite => _swapTextures ? _additionalSprite : _mainSprite;
    private Sprite? otherSprite => _swapTextures ? _mainSprite : _additionalSprite;

    private RenderTexture? _mainRenderTexture;
    private RenderTexture? _additionalRenderTexture;
    private Sprite? _mainSprite;
    private Sprite? _additionalSprite;

    private Vector2f _textPosition;

    public override void Update() {
        window?.DispatchEvents();
        input?.Update();
    }

    public override void Finish() {
        input?.Finish();
        window?.Close();
    }

    protected override void CreateWindow() {
        if(window?.IsOpen ?? false) window.Close();
        UpdateFont();

        VideoMode videoMode = fullscreen ? VideoMode.FullscreenModes[0] :
            new VideoMode((uint)(width * font?.size.x ?? 0), (uint)(height * font?.size.y ?? 0));

        window = new RenderWindow(videoMode, title, fullscreen ? Styles.Fullscreen : Styles.Close);
        window.SetView(new View(new Vector2f(videoMode.Width / 2f, videoMode.Height / 2f),
            new Vector2f(videoMode.Width, videoMode.Height)));

        if(File.Exists(this.icon)) {
            SFML.Graphics.Image icon = new(this.icon);
            window.SetIcon(icon.Size.X, icon.Size.Y, icon.Pixels);
        }

        window.Closed += (_, _) => Finish();
        window.SetKeyRepeatEnabled(false);

        _mainRenderTexture = new RenderTexture(videoMode.Width, videoMode.Height);
        _additionalRenderTexture = new RenderTexture(videoMode.Width, videoMode.Height);
        _mainSprite = new Sprite(_mainRenderTexture.Texture);
        _additionalSprite = new Sprite(_additionalRenderTexture.Texture);

        _textPosition = new Vector2f((videoMode.Width - text?.imageWidth ?? 0) / 2f,
            (videoMode.Height - text?.imageHeight ?? 0) / 2f);

        UpdateFramerate();

        input = new InputManager(this);
        input.Setup();
    }

    protected override void UpdateFramerate() {
        if(window is null) return;
        window.SetFramerateLimit(framerate <= 0 ? 0 : (uint)framerate);
        window.SetVerticalSyncEnabled(framerate == (int)ReservedFramerates.Vsync);
    }

    protected override void UpdateFont() {
        _cachedFullscreenEffects.Clear();
        base.UpdateFont();
    }

    protected override void CreateText() =>
        text = new Text(font, new Vector2Int(width, height), display);

    public override void AddEffect(IEffect effect) {
        base.AddEffect(effect);
        if(_cachedFullscreenEffects.ContainsKey(effect)) return;
        CachedEffect cachedEffect = new() { effect = effect };
        _cachedFullscreenEffects.Add(effect, cachedEffect);
    }

    public override void Draw() => Draw(false);

    private void Draw(bool drawFont) {
        if(window is null) return;

        SFML.Graphics.Color background = SfmlConverters.ToSfmlColor(this.background);

        if(drawFont) {
            window.Clear(background);
            text?.DrawFont(window);
            window.Display();
            return;
        }

        DrawAllEffects();

        text?.RebuildQuads(_textPosition, fullscreenEffects, effects);

        window.Clear(background);
        _mainRenderTexture?.Clear(background);
        _additionalRenderTexture?.Clear(background);
        _mainRenderTexture?.Display();
        _additionalRenderTexture?.Display();

        RunPipelines();

        window.Display();

        effects.Clear();
        fullscreenEffects.Clear();
    }

    private void RunPipelines() {
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach(IEffect effect in fullscreenEffects) {
            if(effect.pipeline is null) continue;

            CachedEffect cachedEffect = _cachedFullscreenEffects[effect];
            // ignore because can't be null when effect.pipeline is not null
            for(int i = 0; i < cachedEffect.pipeline!.Length; i++) {
                CachedPipelineStep step = cachedEffect.pipeline[i];
                RunPipelineStep(step, i);
            }
        }
    }

    private void RunPipelineStep(CachedPipelineStep step, int index) {
        if(window is null || currentRenderTexture is null) return;

        step.shader?.SetUniform("step", index);
        switch(step.type) {
            case PipelineStep.Type.Text:
                step.shader?.SetUniform("current", currentRenderTexture?.Texture);
                step.shader?.SetUniform("target", otherRenderTexture?.Texture);
                text?.DrawQuads(window, step.blendMode, step.shader);
                break;
            case PipelineStep.Type.Screen:
                step.shader?.SetUniform("current", currentRenderTexture?.Texture);
                step.shader?.SetUniform("target", otherRenderTexture?.Texture);
                currentSprite?.Draw(window, step.renderState);
                break;
            case PipelineStep.Type.TemporaryText:
                step.shader?.SetUniform("current", Shader.CurrentTexture);
                text?.DrawQuads(currentRenderTexture, step.blendMode, step.shader);
                break;
            case PipelineStep.Type.TemporaryScreen:
                step.shader?.SetUniform("current", Shader.CurrentTexture);
                step.shader?.SetUniform("target", currentRenderTexture?.Texture);
                otherSprite?.Draw(currentRenderTexture, step.renderState);
                break;
            case PipelineStep.Type.SwapBuffer:
                _swapTextures = !_swapTextures;
                break;
            case PipelineStep.Type.ClearBuffer:
                currentRenderTexture?.Clear();
                break;
        }
    }

    public void Dispose() {
        _mainRenderTexture?.Dispose();
        _additionalRenderTexture?.Dispose();
        _mainSprite?.Dispose();
        _additionalSprite?.Dispose();
        text?.Dispose();
        window?.Dispose();
        GC.SuppressFinalize(this);
    }
}
