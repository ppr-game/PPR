using System;
using System.Collections.Generic;
using System.IO;

using PPR.GUI;
using PPR.Main;
using PPR.Main.Levels;
using PPR.Properties;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace PPR.Rendering {
    public class Renderer {
        public static Renderer instance;

        public readonly Dictionary<Vector2, Color> backgroundColors;
        public readonly Dictionary<Vector2, Color> foregroundColors;
        public readonly Dictionary<Vector2, char> displayString;
        public Vector2 fontSize;
        public readonly int width;
        public int windowWidth;
        public readonly int height;
        public int windowHeight;
        public int frameRate;
        public BitmapText text;
        Sprite _textSprite;
        readonly Image _icon;
        public RenderWindow window;
        readonly Shader _bloomFirstPass = Shader.FromString(
            File.ReadAllText(Path.Combine("resources", "bloom_vert.glsl")), null,
            File.ReadAllText(Path.Combine("resources", "bloom_frag.glsl")));
        readonly Shader _bloomSecondPass = Shader.FromString(
            File.ReadAllText(Path.Combine("resources", "bloom_vert.glsl")), null,
            File.ReadAllText(Path.Combine("resources", "bloom_frag.glsl")));

        RenderTexture _bloomRT;
        readonly RenderStates _blendModeAddState = new RenderStates(BlendMode.Add);

        public Vector2f mousePositionF = new Vector2f(-1f, -1f);
        public Vector2 mousePosition = new Vector2(-1, -1);
        public bool leftButtonPressed;

        public Renderer(int width, int height, int frameRate) {
            instance = this;

            string[] fontMappingsLines = File.ReadAllLines(Path.Combine("resources", "fonts", Settings.Default.font, "mappings.txt"));
            string[] fontSizeStr = fontMappingsLines[0].Split(',');
            fontSize = new Vector2(int.Parse(fontSizeStr[0]), int.Parse(fontSizeStr[1]));

            this.width = width;
            windowWidth = width * fontSize.x;
            this.height = height;
            windowHeight = height * fontSize.y;
            this.frameRate = frameRate;

            backgroundColors = new Dictionary<Vector2, Color>(this.width * this.height);
            foregroundColors = new Dictionary<Vector2, Color>(this.width * this.height);
            displayString = new Dictionary<Vector2, char>(this.width * this.height);

            _icon = new Image(Path.Combine("resources", "icon.png"));

            window = new RenderWindow(new VideoMode((uint)windowWidth, (uint)windowHeight), "Press Press Revolution", Styles.Close);
            window.SetIcon(_icon.Size.X, _icon.Size.Y, _icon.Pixels);

            _bloomRT = new RenderTexture((uint)windowWidth, (uint)windowHeight);

            SubscribeWindowEvents();

            if(frameRate < 0) window.SetVerticalSyncEnabled(true);
            else if(frameRate != 0) window.SetFramerateLimit((uint)frameRate);

            BitmapFont font = new BitmapFont(new Image(Path.Combine("resources", "fonts", Settings.Default.font, "font.png")), fontMappingsLines[1], fontSize);
            text = new BitmapText(font, new Vector2(width, height)) {
                backgroundColors = backgroundColors,
                foregroundColors = foregroundColors,
                text = displayString
            };

            _textSprite = new Sprite(text.renderTexture.Texture) {
                Origin = new Vector2f(text.imageWidth / 2f, text.imageHeight / 2f),
                Position = new Vector2f(windowWidth / 2f, windowHeight / 2f)
            };

            _bloomFirstPass.SetUniform("horizontal", true);
            _bloomSecondPass.SetUniform("horizontal", false);
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
        public void SetFullscreen(bool fullscreen = false) {
            window.Close();
            if(!fullscreen) {
                Core.renderer.windowWidth = Core.renderer.width * Core.renderer.fontSize.x;
                Core.renderer.windowHeight = Core.renderer.height * Core.renderer.fontSize.y;
            }
            window = fullscreen
                ? new RenderWindow(VideoMode.FullscreenModes[0], "Press Press Revolution", Styles.Fullscreen)
                : new RenderWindow(new VideoMode((uint)windowWidth, (uint)windowHeight), "Press Press Revolution", Styles.Close);
            window.SetIcon(_icon.Size.X, _icon.Size.Y, _icon.Pixels);
            SubscribeWindowEvents();
            UpdateWindow();
        }
        public void UpdateWindow() {
            if(Settings.Default.fullscreen) {
                VideoMode videoMode = VideoMode.FullscreenModes[0];

                windowWidth = (int)videoMode.Width;
                windowHeight = (int)videoMode.Height;
            }
            else {
                window.Size = new Vector2u((uint)windowWidth, (uint)windowHeight);
                window.SetView(new View(new Vector2f(windowWidth / 2f, windowHeight / 2f), new Vector2f(windowWidth, windowHeight)));
            }
            _bloomRT = new RenderTexture((uint)windowWidth, (uint)windowHeight);
            _textSprite = new Sprite(text.renderTexture.Texture) {
                Origin = new Vector2f(text.imageWidth / 2f, text.imageHeight / 2f),
                Position = new Vector2f(windowWidth / 2f, windowHeight / 2f)
            };

            if(frameRate < 0) window.SetVerticalSyncEnabled(true);
            else if(frameRate != 0) window.SetFramerateLimit((uint)frameRate);
        }

        void ClearText() {
            backgroundColors.Clear();
            foregroundColors.Clear();
            displayString.Clear();
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
            ClearText();
            Map.Draw();
            UI.Draw();
        }
        public void Draw() {
            text.backgroundColors = backgroundColors;
            text.foregroundColors = foregroundColors;
            text.text = displayString;

            text.RebuildRenderTexture();

            Color backgroundColor = ColorScheme.GetColor("background");

            window.Clear(backgroundColor);

            if(Settings.Default.bloom) {
                _bloomRT.Clear(backgroundColor);
                _bloomRT.Draw(_textSprite);
                Sprite sprite = new Sprite(_bloomRT.Texture);

                _bloomFirstPass.SetUniform("image", _bloomRT.Texture);
                _bloomRT.Draw(sprite, new RenderStates(_bloomFirstPass));
                _bloomRT.Display();

                _bloomSecondPass.SetUniform("image", _bloomRT.Texture);
                window.Draw(sprite, new RenderStates(_bloomSecondPass));
            }

            window.Draw(_textSprite, _blendModeAddState);
        }
        public enum Alignment { Left, Center, Right }

        
        public void DrawText(Vector2 position, string text, Alignment align = Alignment.Left,
            bool replacingSpaces = false) {
            DrawText(position, text, ColorScheme.GetColor("foreground"), align, replacingSpaces);
        }
        public void DrawText(Vector2 position, string text, Color color, Alignment align = Alignment.Left,
            bool replacingSpaces = false) {
            DrawText(position, text, color, ColorScheme.GetColor("transparent"), align, replacingSpaces);
        }
        public void DrawText(Vector2 position, string text, Color foregroundColor, Color backgroundColor, Alignment align = Alignment.Left,
            bool replacingSpaces = false) {
            switch(text.Length) {
                case 0: return; // Don't do anything, if the text is empty
                case 1: {
                    if(!replacingSpaces && text[0] == ' ') return;
                    SetCharacter(position, text[0], foregroundColor, backgroundColor);
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
            int y = 0;
            foreach(char curChar in text) {
                if(curChar == '\n') {
                    x = 0;
                    y++;
                    continue;
                }
                if(!replacingSpaces && curChar == ' ') {
                    x++;
                    continue;
                }
                SetCharacter(new Vector2(posX + x, position.y + y), curChar, foregroundColor, backgroundColor);
                x++;
            }
        }

        public void SetCharacter(Vector2 position, char character) {
            if(!position.InBounds(0, 0, width - 1, height - 1)) return;
            
            // If a character is a space or null, remove it from a dictionary
            if(character == ' ' || character == '\0') _ = displayString.Remove(position);
            else displayString[position] = character;
        }
        public void SetCharacter(Vector2 position, char character, Color foregroundColor, Color backgroundColor) {
            SetCharacter(position, foregroundColor == backgroundColor || foregroundColor.A == 0 ? ' ' : character);
            SetCellColor(position, foregroundColor, backgroundColor);
        }
        public char GetCharacter(Vector2 position) {
            return !displayString.ContainsKey(position) ? '\0' : displayString[position];
        }
        public void SetCellColor(Vector2 position, Color foregroundColor, Color backgroundColor) {
            if(!position.InBounds(0, 0, width - 1, height - 1)) return;
            
            if(backgroundColor == ColorScheme.GetColor("background")) _ = backgroundColors.Remove(position);
            else if(backgroundColor.A != 0) backgroundColors[position] = backgroundColor;

            if(foregroundColor == ColorScheme.GetColor("foreground")) _ = foregroundColors.Remove(position);
            else foregroundColors[position] = foregroundColor;
        }
        public static Color LerpColors(Color a, Color b, float t) {
            return t <= 0f ? a : t >= 1f ? b :
                new Color((byte)MathF.Floor(a.R + (b.R - a.R) * t),
                          (byte)MathF.Floor(a.G + (b.G - a.G) * t),
                          (byte)MathF.Floor(a.B + (b.B - a.B) * t),
                          (byte)MathF.Floor(a.A + (b.A - a.A) * t));
        }
        public static Color AnimateColor(float time, Color start, Color end, float rate) {
            float t = Math.Clamp(time * rate, 0f, 1f);
            return LerpColors(start, end, t);
        }
    }
}
