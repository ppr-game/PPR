using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

using PPR.GUI;
using PPR.Main;
using PPR.Main.Levels;
using PPR.Properties;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace PPR.Rendering {
    public readonly struct RenderCharacter {
        public RenderCharacter(char character, Color background, Color foreground) {
            this.character = character;
            this.background = background;
            this.foreground = foreground;
        }
        public RenderCharacter(char character, RenderCharacter oldChar) {
            this.character = character;
            background = oldChar.background;
            foreground = oldChar.foreground;
        }
        public RenderCharacter(Color background, Color foreground, RenderCharacter oldChar) {
            character = oldChar.character;
            this.background = background;
            this.foreground = foreground;
        }
        public RenderCharacter(Color background, RenderCharacter oldChar) {
            character = oldChar.character;
            this.background = background;
            foreground = oldChar.foreground;
        }
        public RenderCharacter(RenderCharacter oldChar, Color foreground) {
            character = oldChar.character;
            background = oldChar.background;
            this.foreground = foreground;
        }
        public bool Equals(RenderCharacter other) {
            return character == other.character && background.Equals(other.background) &&
                   foreground.Equals(other.foreground);
        }
        public override bool Equals(object obj) {
            return obj is RenderCharacter other && Equals(other);
        }
        public override int GetHashCode() {
            return HashCode.Combine(character, background, foreground);
        }
        public static bool operator ==(RenderCharacter left, RenderCharacter right) { return left.Equals(right); }
        public static bool operator !=(RenderCharacter left, RenderCharacter right) { return !left.Equals(right); }
        
        public readonly char character;
        public readonly Color background;
        public readonly Color foreground;
    }
    public class Renderer {
        public static Renderer instance;

        public readonly Dictionary<Vector2, RenderCharacter> display;
        
        public Vector2 fontSize;
        public readonly int width;
        public int windowWidth;
        public readonly int height;
        public int windowHeight;
        int _framerate;
        public int framerate {
            get => _framerate;
            set {
                _framerate = value;
                window.SetFramerateLimit(value < 0 ? 0 : (uint)value);
                window.SetVerticalSyncEnabled(value < 0);
            }
        }
        public BitmapText text;
        Sprite _textSprite;
        readonly Image _icon;
        public RenderWindow window;
        readonly Shader _bloomFirstPass = Shader.FromString(
            File.ReadAllText(Path.Join("resources", "bloom_vert.glsl")), null,
            File.ReadAllText(Path.Join("resources", "bloom_frag.glsl")));
        readonly Shader _bloomSecondPass = Shader.FromString(
            File.ReadAllText(Path.Join("resources", "bloom_vert.glsl")), null,
            File.ReadAllText(Path.Join("resources", "bloom_frag.glsl")));
        public Shader bloomBlend;

        RenderTexture _bloomRT1;
        RenderTexture _bloomRT2;

        public Vector2f mousePositionF = new Vector2f(-1f, -1f);
        public Vector2 mousePosition = new Vector2(-1, -1);
        public bool leftButtonPressed;

        public Renderer(int width, int height, int framerate) {
            instance = this;

            string[] fontMappingsLines =
                File.ReadAllLines(Path.Join("resources", "fonts", Settings.GetPath("font"), "mappings.txt"));
            string[] fontSizeStr = fontMappingsLines[0].Split(',');
            fontSize = new Vector2(int.Parse(fontSizeStr[0]), int.Parse(fontSizeStr[1]));

            this.width = width;
            this.height = height;

            display = new Dictionary<Vector2, RenderCharacter>(this.width * this.height);

            _icon = new Image(Path.Join("resources", "icon.png"));

            BitmapFont font =
                new BitmapFont(new Image(Path.Join("resources", "fonts", Settings.GetPath("font"), "font.png")),
                    fontMappingsLines[1], fontSize);
            text = new BitmapText(font, new Vector2(width, height)) {
                text = display
            };

            SetFullscreen(Settings.GetBool("fullscreen"), false);

            this.framerate = framerate;

            _bloomFirstPass.SetUniform("horizontal", true);
            _bloomSecondPass.SetUniform("horizontal", false);
        }

        public void UpdateFramerateSetting() {
            SetFramerateSetting(Settings.GetInt("fpsLimit"));
        }
        public void SetFramerateSetting(int framerate) {
            this.framerate = window.HasFocus() ? framerate < 60 ? -1 : framerate > 960 ? 0 : framerate : 30;
        }
        void SubscribeWindowEvents() {
            leftButtonPressed = false;

            window.KeyPressed += Game.KeyPressed;
            window.MouseWheelScrolled += Game.MouseWheelScrolled;
            window.LostFocus += Game.LostFocus;
            window.GainedFocus += Game.GainedFocus;
            window.Closed += (_, __) => Game.End();
            window.MouseMoved += UpdateMousePosition;
            window.MouseButtonPressed += (_, e) => {
                if(e.Button == Mouse.Button.Left) leftButtonPressed = true;
            };
            window.MouseButtonReleased += (_, e) => {
                if(e.Button == Mouse.Button.Left) leftButtonPressed = false;
            };
            window.SetKeyRepeatEnabled(false);
        }
        public void SetFullscreen(bool fullscreen = false, bool reloadColorScheme = true) {
            if(window != null && window.IsOpen) window.Close();
            if(!fullscreen) {
                windowWidth = width * fontSize.x;
                windowHeight = height * fontSize.y;
            }
            window = fullscreen
                ? new RenderWindow(VideoMode.FullscreenModes[0], "Press Press Revolution", Styles.Fullscreen)
                : new RenderWindow(new VideoMode((uint)windowWidth, (uint)windowHeight), "Press Press Revolution", Styles.Close);
            window.SetIcon(_icon.Size.X, _icon.Size.Y, _icon.Pixels);
            if(reloadColorScheme) ColorScheme.Reload();
            SubscribeWindowEvents();
            UpdateWindow();
        }
        public void UpdateWindow() {
            if(Settings.GetBool("fullscreen")) {
                VideoMode videoMode = VideoMode.FullscreenModes[0];

                windowWidth = (int)videoMode.Width;
                windowHeight = (int)videoMode.Height;
            }
            else {
                window.Size = new Vector2u((uint)windowWidth, (uint)windowHeight);
                window.SetView(new View(new Vector2f(windowWidth / 2f, windowHeight / 2f), new Vector2f(windowWidth, windowHeight)));
            }
            _bloomRT1 = new RenderTexture((uint)windowWidth, (uint)windowHeight);
            _bloomRT2 = new RenderTexture((uint)windowWidth, (uint)windowHeight);
            _textSprite = new Sprite(text.renderTexture.Texture) {
                Origin = new Vector2f(text.imageWidth / 2f, text.imageHeight / 2f),
                Position = new Vector2f(windowWidth / 2f, windowHeight / 2f)
            };

            UpdateFramerateSetting();
        }

        void UpdateMousePosition(object caller, MouseMoveEventArgs mouse) {
            if(!window.HasFocus()) {
                mousePosition = new Vector2(-1, -1);
                return;
            }
            mousePositionF = new Vector2f((mouse.X - windowWidth / 2f + text.imageWidth / 2f) / fontSize.x,
                (mouse.Y - windowHeight / 2f + text.imageHeight / 2f) / fontSize.y);
            mousePosition = new Vector2((int)mousePositionF.X, (int)mousePositionF.Y);
        }
        public void Update() {
            display.Clear();
            Map.Draw();
            UI.Draw();
        }
        public void Draw() {
            text.RebuildRenderTexture();
            
            if(Settings.GetBool("bloom")) {
                _bloomRT1.Clear(ColorScheme.GetColor("background"));
                _bloomRT1.Draw(_textSprite);
                //Texture fullscreenText = new Texture(_bloomRT.Texture);
                
                _bloomFirstPass.SetUniform("image", _bloomRT1.Texture);
                _bloomRT2.Clear(ColorScheme.GetColor("background"));
                _bloomRT2.Draw(new Sprite(_bloomRT1.Texture), new RenderStates(_bloomFirstPass));

                _bloomSecondPass.SetUniform("image", _bloomRT2.Texture);
                _bloomRT1.Clear(ColorScheme.GetColor("background"));
                _bloomRT1.Draw(new Sprite(_bloomRT2.Texture), new RenderStates(_bloomSecondPass));
                
                _bloomRT2.Clear(ColorScheme.GetColor("background"));
                _bloomRT2.Draw(_textSprite);
                
                _bloomRT1.Display();
                _bloomRT2.Display();

                bloomBlend.SetUniform("imageA", _bloomRT2.Texture);
                bloomBlend.SetUniform("imageB", _bloomRT1.Texture);
                window.Draw(new Sprite(_bloomRT1.Texture), new RenderStates(bloomBlend));
            }
            else {
                window.Clear(ColorScheme.GetColor("background"));
                window.Draw(_textSprite);
            }
        }
        public enum Alignment { Left, Center, Right }
        public void DrawText(Vector2 position, string text, Alignment align = Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            DrawText(position, text, ColorScheme.GetColor("foreground"), align, replacingSpaces, invertOnDarkBG);
        }
        public void DrawText(Vector2 position, string text, Color color, Alignment align = Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            DrawText(position, text, color, ColorScheme.GetColor("transparent"), align, replacingSpaces,
                invertOnDarkBG);
        }
        public void DrawText(Vector2 position, string text, Color foregroundColor, Color backgroundColor, Alignment align = Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            switch(text.Length) {
                case 0: return; // Don't do anything, if the text is empty
                case 1: {
                    if(!replacingSpaces && text[0] == ' ') return;
                    Color useFG = foregroundColor;
                    if(invertOnDarkBG) {
                        Color useBGColor = backgroundColor.A == 0 ? GetBackgroundColor(position) : backgroundColor;
                        float luma = 0.299f * useBGColor.R +
                                     0.587f * useBGColor.G +
                                     0.114f * useBGColor.B;
                        if(luma < 127.5f) useFG = new Color(255, 255, 255, foregroundColor.A) -
                                                  new Color(foregroundColor.R, foregroundColor.G,
                                                      foregroundColor.B, 0);
                    }
                    SetCharacter(position, new RenderCharacter(text[0], backgroundColor, useFG));
                    return;
                }
            }

            int posX = position.x - align switch
            {
                Alignment.Right => text.Length - 1,
                Alignment.Center => (int)MathF.Ceiling(text.Length / 2f),
                _ => 0
            };

            int x = 0;
            foreach(char curChar in text) {
                if(!replacingSpaces && curChar == ' ') {
                    x++;
                    continue;
                }
                Vector2 charPos = new Vector2(posX + x, position.y);
                Color useFG = foregroundColor;
                if(invertOnDarkBG) {
                    Color useBGColor = backgroundColor.A == 0 ? GetBackgroundColor(charPos) : backgroundColor;
                    float luma = 0.299f * useBGColor.R +
                                 0.587f * useBGColor.G +
                                 0.114f * useBGColor.B;
                    if(luma < 127.5f) useFG = new Color(255, 255, 255, foregroundColor.A) -
                                              new Color(foregroundColor.R, foregroundColor.G,
                                                  foregroundColor.B, 0);
                }
                SetCharacter(charPos, new RenderCharacter(curChar, backgroundColor, useFG));
                x++;
            }
        }
        public void DrawText(Vector2 position, string[] lines, Alignment align = Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            DrawLines(position, lines, align, replacingSpaces, invertOnDarkBG);
        }
        public void DrawText(Vector2 position, string[] lines, Color color, Alignment align = Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            DrawLines(position, lines, color, align, replacingSpaces, invertOnDarkBG);
        }
        public void DrawText(Vector2 position, string[] lines, Color foregroundColor, Color backgroundColor,
            Alignment align = Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            DrawLines(position, lines, foregroundColor, backgroundColor, align, replacingSpaces, invertOnDarkBG);
        }
        public void DrawLines(Vector2 position, string[] lines, Alignment align = Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            DrawLines(position, lines, ColorScheme.GetColor("foreground"), align, replacingSpaces, invertOnDarkBG);
        }
        public void DrawLines(Vector2 position, string[] lines, Color color, Alignment align = Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            DrawLines(position, lines, color, ColorScheme.GetColor("transparent"), align, replacingSpaces,
                invertOnDarkBG);
        }
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public void DrawLines(Vector2 position, string[] lines, Color foregroundColor, Color backgroundColor,
            Alignment align = Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            for(int i = 0; i < lines.Length; i++)
                DrawText(position + new Vector2(0, i), lines[i], foregroundColor, backgroundColor, align,
                    replacingSpaces, invertOnDarkBG);
        }

        public void SetCharacter(Vector2 position, RenderCharacter character) {
            if(!position.InBounds(0, 0, width - 1, height - 1)) return;
            
            if(IsRenderCharacterEmpty(character)) display.Remove(position);
            else display[position] = character;
        }
        public RenderCharacter GetCharacter(Vector2 position) {
            return display.ContainsKey(position) ? display[position] : new RenderCharacter('\0', Color.Transparent, Color.Transparent);
        }
        public char GetDisplayedCharacter(Vector2 position) {
            return !display.ContainsKey(position) ? '\0' : display[position].character;
        }
        public void SetCellColor(Vector2 position, Color foregroundColor, Color backgroundColor) {
            if(!position.InBounds(0, 0, width - 1, height - 1)) return;
            
            RenderCharacter newCharacter = new RenderCharacter(backgroundColor, foregroundColor,
                GetCharacter(position));

            if(IsRenderCharacterEmpty(newCharacter)) display.Remove(position);
            else display[position] = newCharacter;
        }
        public Color GetBackgroundColor(Vector2 position) {
            if(!position.InBounds(0, 0, width - 1, height - 1) || !display.ContainsKey(position))
                return ColorScheme.GetColor("background");
            return display[position].background;
        }

        static bool IsRenderCharacterEmpty(RenderCharacter renderCharacter) {
            return renderCharacter.background.A == 0 &&
                   (renderCharacter.character == '\0' || renderCharacter.character == ' ' ||
                   renderCharacter.foreground.A == 0);
        }
        
        public static Color LerpColors(Color a, Color b, float t) {
            return t <= 0f ? a : t >= 1f ? b :
                new Color((byte)MathF.Floor(a.R + (b.R - a.R) * t),
                          (byte)MathF.Floor(a.G + (b.G - a.G) * t),
                          (byte)MathF.Floor(a.B + (b.B - a.B) * t),
                          (byte)MathF.Floor(a.A + (b.A - a.A) * t));
        }
        public static Color AnimateColor(float time, Color start, Color end, float rate) {
            return LerpColors(start, end, time * rate);
        }
    }
}
