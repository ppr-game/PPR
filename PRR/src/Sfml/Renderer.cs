using System;
using System.IO;

using PER.Abstractions.Renderer;
using PER.Util;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

using Shader = SFML.Graphics.Shader;

namespace PRR.Sfml {
    public class Renderer : RendererBase, IDisposable {
        public override bool open => _window.IsOpen;
        public override bool focused => _window.HasFocus();

        private bool _swapTextures;

        private RenderTexture currentRenderTexture => _swapTextures ? _additionalRenderTexture : _mainRenderTexture;
        private RenderTexture otherRenderTexture => _swapTextures ? _mainRenderTexture : _additionalRenderTexture;
        private Sprite currentSprite => _swapTextures ? _additionalSprite : _mainSprite;
        private Sprite otherSprite => _swapTextures ? _mainSprite : _additionalSprite;
        
        private RenderTexture _mainRenderTexture;
        private RenderTexture _additionalRenderTexture;
        private Sprite _mainSprite;
        private Sprite _additionalSprite;

        private Text _text;
        private Vector2f _textPosition;
        private RenderWindow _window;

        public override void Update() => _window.DispatchEvents();

        public override void Finish() => _window?.Close();

        protected override void CreateWindow() {
            if(_window?.IsOpen ?? false) _window.Close();
            UpdateFont();
            
            VideoMode videoMode = fullscreen ? VideoMode.FullscreenModes[0] :
                new VideoMode((uint)(width * font.size.x), (uint)(height * font.size.y));

            _window = new RenderWindow(videoMode, title, fullscreen ? Styles.Fullscreen : Styles.Close);
            _window.SetView(new View(new Vector2f(videoMode.Width / 2f, videoMode.Height / 2f),
                new Vector2f(videoMode.Width, videoMode.Height)));
            
            if(File.Exists(this.icon)) {
                SFML.Graphics.Image icon = new(this.icon);
                _window.SetIcon(icon.Size.X, icon.Size.Y, icon.Pixels);
            }
            
            _window.Closed += (_, _) => Finish();
            _window.MouseMoved += UpdateMousePosition;
            _window.SetKeyRepeatEnabled(false);
                
            _mainRenderTexture = new RenderTexture(videoMode.Width, videoMode.Height);
            _additionalRenderTexture = new RenderTexture(videoMode.Width, videoMode.Height);
            _mainSprite = new Sprite(_mainRenderTexture.Texture);
            _additionalSprite = new Sprite(_additionalRenderTexture.Texture);
                
            _textPosition = new Vector2f((videoMode.Width - _text.imageWidth) / 2f,
                (videoMode.Height - _text.imageHeight) / 2f);

            UpdateFramerate();
        }

        protected override IEffectContainer CreateEffectContainer() => new EffectContainer();
        protected override void CreateText() =>
            _text = new Text(font, new Vector2Int(width, height)) { text = display };

        protected override void UpdateFramerate() {
            if(_window is null) return;
            _window.SetFramerateLimit(framerate <= 0 ? 0 : (uint)framerate);
            _window.SetVerticalSyncEnabled(framerate == (int)ReservedFramerates.Vsync);
        }

        private void UpdateMousePosition(object caller, MouseMoveEventArgs mouse) {
            if(!_window.HasFocus()) {
                mousePosition = new Vector2Int(-1, -1);
                accurateMousePosition = new Vector2(-1f, -1f);
                return;
            }

            accurateMousePosition = new Vector2((mouse.X - _window.Size.X / 2f + _text.imageWidth / 2f) / font.size.x,
                (mouse.Y - _window.Size.Y / 2f + _text.imageHeight / 2f) / font.size.y);
            mousePosition = new Vector2Int((int)accurateMousePosition.x, (int)accurateMousePosition.y);
        }

        public override void Draw() => Draw(false);

        private void Draw(bool drawFont) {
            SFML.Graphics.Color background = SfmlConverters.ToSfmlColor(this.background);
            
            if(drawFont) {
                _window.Clear(background);
                _text.DrawFont(_window);
                _window.Display();
                return;
            }
            
            _text.RebuildQuads(_textPosition, fullscreenEffects, effects);
            
            _window.Clear(background);
            _mainRenderTexture.Clear(background);
            _additionalRenderTexture.Clear(background);
            _mainRenderTexture.Display();
            _additionalRenderTexture.Display();

            RunPipeline();
            
            _window.Display();
        }

        private void RunPipeline() {
            for(int i = 0; i < fullscreenEffects.Count; i++) {
                IEffectContainer effectContainer = fullscreenEffects[i];
                while(effectContainer.effect.ended) {
                    fullscreenEffects.RemoveAt(i);
                    effectContainer = fullscreenEffects[i];
                }

                if(effectContainer.effect.pipeline is null) continue;

                EffectContainer effect = (EffectContainer)effectContainer;
                for(int j = 0; j < effect.pipeline.Length; j++) {
                    CachedPipelineStep step = effect.pipeline[j];
                    RunPipelineStep(step, j);
                }
            }
        }

        private void RunPipelineStep(CachedPipelineStep step, int index) {
            step.shader?.SetUniform("step", index);
            switch(step.type) {
                case PipelineStep.Type.Text:
                    step.shader?.SetUniform("current", currentRenderTexture.Texture);
                    step.shader?.SetUniform("target", otherRenderTexture.Texture);
                    _text.DrawQuads(_window, step.blendMode, step.shader);
                    break;
                case PipelineStep.Type.Screen:
                    step.shader?.SetUniform("current", currentRenderTexture.Texture);
                    step.shader?.SetUniform("target", otherRenderTexture.Texture);
                    currentSprite.Draw(_window, step.renderState);
                    break;
                case PipelineStep.Type.TemporaryText:
                    step.shader?.SetUniform("current", Shader.CurrentTexture);
                    _text.DrawQuads(currentRenderTexture, step.blendMode, step.shader);
                    break;
                case PipelineStep.Type.TemporaryScreen:
                    step.shader?.SetUniform("current", Shader.CurrentTexture);
                    step.shader?.SetUniform("target", currentRenderTexture.Texture);
                    otherSprite.Draw(currentRenderTexture, step.renderState);
                    break;
                case PipelineStep.Type.SwapBuffer:
                    _swapTextures = !_swapTextures;
                    break;
                case PipelineStep.Type.ClearBuffer:
                    currentRenderTexture.Clear();
                    break;
            }
        }

        public void Dispose() {
            _mainRenderTexture?.Dispose();
            _additionalRenderTexture?.Dispose();
            _mainSprite?.Dispose();
            _additionalSprite?.Dispose();
            _text?.Dispose();
            _window?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
