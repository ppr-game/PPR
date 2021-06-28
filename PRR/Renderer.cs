using System;
using System.Collections.Generic;
using System.IO;

using PER.Abstractions.Renderer;
using PER.Util;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

using Color = PER.Util.Color;
using Shader = SFML.Graphics.Shader;

namespace PRR {
    public class Renderer : IRenderer {
        public string title { get; private set; }
        public int width { get; private set; }
        public int height { get; private set; }
        
        public int framerate {
            get => _framerate;
            set {
                _framerate = value;
                if(_window == null) return;
                UpdateFramerate();
            }
        }

        public bool fullscreen {
            get => _fullscreen;
            set {
                _fullscreen = value;
                Reset();
            }
        }

        public string font {
            get => _font;
            set {
                _font = value;
                Reset();
            }
        }

        public Vector2Int fontSize { get; private set; }
        public string icon { get; set; }

        public bool open => _window.IsOpen;
        public bool focused => _window.HasFocus();
        
        public Color background { get; set; } = Color.black;
        
        public Vector2Int mousePosition { get; private set; } = new(-1, -1);
        public Vector2 accurateMousePosition { get; private set; } = new(-1f, -1f);
        
        public List<IEffectContainer> ppEffects { get; private set; }

        private Dictionary<Vector2Int, RenderCharacter> _display;
        private Dictionary<Vector2Int, EffectContainer> _effects;

        private int _framerate;
        private bool _fullscreen;
        private string _font;

        private bool _swapTextures;

        private RenderTexture currentRenderTexture => _swapTextures ? _additionalRenderTexture : _mainRenderTexture;
        private RenderTexture otherRenderTexture => _swapTextures ? _mainRenderTexture : _additionalRenderTexture;
        private Sprite currentSprite => _swapTextures ? _additionalSprite : _mainSprite;
        
        private RenderTexture _mainRenderTexture;
        private RenderTexture _additionalRenderTexture;
        private Sprite _mainSprite;
        private Sprite _additionalSprite;

        private Text _text;
        private Vector2f _textPosition;
        private RenderWindow _window;

        public void Setup(RendererSettings settings) {
            title = settings.title;
            width = settings.width;
            height = settings.height;
            _framerate = settings.framerate;
            _fullscreen = settings.fullscreen;
            _font = settings.font;
            icon = settings.icon;
            
            CreateWindow();
        }
        
        public void Loop() => _window.DispatchEvents();

        public void Stop() => _window?.Close();

        public void Reset(RendererSettings settings) {
            Stop();
            Setup(settings);
        }

        public void Reset() => Reset(new RendererSettings(this));

        private void CreateWindow() {
            if(_window?.IsOpen ?? false) _window.Close();
            UpdateFont();
            
            VideoMode videoMode = fullscreen ? VideoMode.FullscreenModes[0] :
                new VideoMode((uint)(width * fontSize.x), (uint)(height * fontSize.y));

            _window = new RenderWindow(videoMode, title, fullscreen ? Styles.Fullscreen : Styles.Close);
            _window.SetView(new View(new Vector2f(videoMode.Width / 2f, videoMode.Height / 2f),
                new Vector2f(videoMode.Width, videoMode.Height)));
            
            if(File.Exists(this.icon)) {
                Image icon = new(this.icon);
                _window.SetIcon(icon.Size.X, icon.Size.Y, icon.Pixels);
            }
            
            _window.Closed += (_, _) => Stop();
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

        private void UpdateFramerate() {
            _window.SetFramerateLimit(_framerate <= 0 ? 0 : (uint)_framerate);
            _window.SetVerticalSyncEnabled(_framerate == (int)ReservedFramerates.Vsync);
        }

        private void UpdateFont() {
            string[] fontMappingsLines = File.ReadAllLines(Path.Join(this.font, "mappings.txt"));
            string[] fontSizeStr = fontMappingsLines[0].Split(',');
            fontSize = new Vector2Int(int.Parse(fontSizeStr[0]), int.Parse(fontSizeStr[1]));
            char background = fontSizeStr[2][0];
                
            _display = new Dictionary<Vector2Int, RenderCharacter>(width * height);
            _effects = new Dictionary<Vector2Int, EffectContainer>(width * height);
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < width; y++) {
                    Vector2Int position = new(x, y);
                    _effects.Add(position, new EffectContainer());
                }
            }

            ppEffects = new List<IEffectContainer>();

            Font font = new(new Image(Path.Join(this.font, "font.png")), fontMappingsLines[1], fontSize, background);
            _text = new Text(font, new Vector2Int(width, height)) { text = _display };
        }

        private void UpdateMousePosition(object caller, MouseMoveEventArgs mouse) {
            if(!_window.HasFocus()) {
                mousePosition = new Vector2Int(-1, -1);
                accurateMousePosition = new Vector2(-1f, -1f);
                return;
            }

            accurateMousePosition = new Vector2((mouse.X - _window.Size.X / 2f + _text.imageWidth / 2f) / fontSize.x,
                (mouse.Y - _window.Size.Y / 2f + _text.imageHeight / 2f) / fontSize.y);
            mousePosition = new Vector2Int((int)accurateMousePosition.x, (int)accurateMousePosition.y);
        }

        public void Clear() => _display.Clear();

        public void Draw() => Draw(true, false);

        public void Draw(bool bloom, bool drawFont) {
            SFML.Graphics.Color background = SfmlConverters.ToSfmlColor(this.background);
            
            if(drawFont) {
                _window.Clear(background);
                _window.Draw(new Sprite(_text.font.texture));
                _window.Display();
                return;
            }
            
            _text.RebuildQuads(_textPosition);
            
            _window.Clear(background);
            _text.DrawQuads(_window, BlendMode.Alpha);

            if(bloom) {
                _mainRenderTexture.Clear(background);
                _additionalRenderTexture.Clear(background);
                _mainRenderTexture.Display();
                _additionalRenderTexture.Display();
                
                _text.DrawQuads(currentRenderTexture, BlendMode.Alpha);

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach(IEffectContainer effectContainer in ppEffects) {
                    if(effectContainer.effect.ppShaders is null) continue;
                    EffectContainer effect = (EffectContainer)effectContainer;
                    for(int i = 0; i < effect.ppShaders.Length; i++) {
                        Shader shader = effect.ppShaders[i];
                        shader.SetUniform("image", Shader.CurrentTexture);
                        shader.SetUniform("pass", i);
                        otherRenderTexture.Draw(currentSprite, new RenderStates(shader));
                        _swapTextures = !_swapTextures;
                    }
                }

                _window.Draw(currentSprite, new RenderStates(BlendMode.Add));
            }
            
            _window.Display();
        }

        public void DrawCharacter(Vector2Int position, RenderCharacter character,
            RenderFlags flags = RenderFlags.Default) {
            if(position.x < 0 || position.y < 0 || position.x >= width || position.y >= height) return;
            
            if((flags & RenderFlags.BackgroundAlphaBlending) != 0) {
                RenderCharacter currentCharacter = GetCharacter(position);
                Color background = Color.Blend(currentCharacter.background, character.background);
                //Color foreground = Color.Blend(background, character.foreground);
                character = new RenderCharacter(character.character, background, character.foreground, character.style);
            }

            if((flags & RenderFlags.InvertedBackgroundAsForegroundColor) != 0) {
                RenderCharacter currentCharacter = GetCharacter(position);
                character = new RenderCharacter(character.character, character.background,
                    Color.white - currentCharacter.background, character.style);
            }
            
            if(IsRenderCharacterEmpty(character)) _display.Remove(position);
            else _display[position] = character;
        }

        public void DrawText(Vector2Int position, string text, Color foregroundColor, Color backgroundColor,
            HorizontalAlignment align = HorizontalAlignment.Left, RenderStyle style = RenderStyle.None,
            RenderFlags flags = RenderFlags.Default) {
            if(text.Length == 0) return;

            int actualTextLength = GetTextLengthWithoutFormatting(text);

            int x = align switch {
                HorizontalAlignment.Left => 0,
                HorizontalAlignment.Middle => -actualTextLength + actualTextLength / 2 + 1,
                HorizontalAlignment.Right => -actualTextLength + 1,
                _ => throw new ArgumentOutOfRangeException(nameof(align), align, "wtf")
            };

            bool formatting = false;
            bool color = false;
            bool foreground = false;
            bool background = false;
            IList<char> colorsRecord = new List<char>(8);
            foreach(char curChar in text) {
                if(curChar == '\f') {
                    formatting = !formatting;
                    color = false;
                    foreground = false;
                    background = false;
                    colorsRecord.Clear();
                    continue;
                }

                if(formatting)
                    ProcessFormatting(curChar, ref color, ref foreground, ref background, colorsRecord,
                        ref foregroundColor, ref backgroundColor, ref style, ref flags);
                else {
                    Vector2Int charPos = new(position.x + x, position.y);
                    DrawCharacter(charPos, new RenderCharacter(curChar, backgroundColor, foregroundColor, style), flags);
                    x++;
                }
            }
        }

        public void DrawText(Vector2Int position, string[] lines, Color foregroundColor, Color backgroundColor,
            HorizontalAlignment align = HorizontalAlignment.Left, RenderStyle style = RenderStyle.None,
            RenderFlags flags = RenderFlags.Default) {
            for(int i = 0; i < lines.Length; i++)
                DrawText(position + new Vector2Int(0, i), lines[i], foregroundColor, backgroundColor,
                    align, style, flags);
        }

        public RenderCharacter GetCharacter(Vector2Int position) => _display.ContainsKey(position) ? _display[position] :
            new RenderCharacter('\0', Color.transparent, Color.transparent);

        private bool IsRenderCharacterEmpty(RenderCharacter renderCharacter) =>
            renderCharacter.background.a == 0 &&
            (!CharacterExists(renderCharacter.character) || renderCharacter.foreground.a == 0);

        private bool CharacterExists(char character) => _text.font.mappings.Contains(character);

        private static void ProcessFormatting(char character, ref bool color, ref bool foreground, ref bool background,
            IList<char> colorsRecord, ref Color foregroundColor, ref Color backgroundColor, ref RenderStyle style,
            ref RenderFlags flags) {
            if(color)
                ProcessColorFormatting(character, ref color, ref foreground, ref background, colorsRecord,
                    ref foregroundColor, ref backgroundColor);
            else ProcessNormalFormatting(character, ref color, ref style, ref flags);
        }

        private static void ProcessNormalFormatting(char character, ref bool color, ref RenderStyle style,
            ref RenderFlags flags) {
            switch(character) {
                case 'c': color = true;
                    break;
                case 'b': style ^= RenderStyle.Bold;
                    break;
                case 'u': style ^= RenderStyle.Underline;
                    break;
                case 's': style ^= RenderStyle.Strikethrough;
                    break;
                case 'i': style ^= RenderStyle.Italic;
                    break;
                case 'a': flags ^= RenderFlags.BackgroundAlphaBlending;
                    break;
                case 'x': flags ^= RenderFlags.InvertedBackgroundAsForegroundColor;
                    break;
            }
        }

        private static void ProcessColorFormatting(char character, ref bool color, ref bool foreground,
            ref bool background, IList<char> colorsRecord, ref Color foregroundColor, ref Color backgroundColor) {
            if(foreground || background) {
                ProcessColor(character, ref foreground, ref background, colorsRecord, ref foregroundColor, ref backgroundColor);
            }
            else {
                switch(character) {
                    case 'c': color = false;
                        break;
                    case 'f': foreground = true;
                        break;
                    case 'b': background = true;
                        break;
                }
            }
        }

        private static void ProcessColor(char character, ref bool foreground, ref bool background,
            IList<char> colorsRecord, ref Color foregroundColor, ref Color backgroundColor) {
            colorsRecord.Add(character);
            if(colorsRecord.Count < 8) return;
            byte[] colorArray = new byte[4];
            for(int i = 0; i < 4; i++)
                colorArray[i] =
                    (byte)(GetHexVal(colorsRecord[i * 2]) * 0xF + GetHexVal(colorsRecord[i * 2 + 1]));

            if(foreground) {
                foregroundColor = new Color(colorArray[0], colorArray[1], colorArray[2], colorArray[3]);
                foreground = false;
            }
            else {
                backgroundColor = new Color(colorArray[0], colorArray[1], colorArray[2], colorArray[3]);
                background = false;
            }
            
            colorsRecord.Clear();
        }

        private static int GetHexVal(char hex) {
            int val = hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        private static int GetTextLengthWithoutFormatting(string text) {
            int actualTextLength = text.Length;
            bool formatting = false;
            foreach(char curChar in text) {
                if(curChar == '\f') {
                    formatting = !formatting;
                    actualTextLength--;
                }
                else if(formatting) actualTextLength--;
            }

            return actualTextLength;
        }
    }
}
