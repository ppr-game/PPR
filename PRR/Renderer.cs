using System;
using System.Collections.Generic;
using System.IO;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace PRR {
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

        public bool Equals(RenderCharacter other) => character == other.character &&
                                                     background.Equals(other.background) &&
                                                     foreground.Equals(other.foreground);

        public override bool Equals(object obj) => obj is RenderCharacter other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(character, background, foreground);
        public static bool operator ==(RenderCharacter left, RenderCharacter right) => left.Equals(right);
        public static bool operator !=(RenderCharacter left, RenderCharacter right) => !left.Equals(right);

        public readonly char character;
        public readonly Color background;
        public readonly Color foreground;
    }

    public class Renderer {
        public enum Alignment { Left, Center, Right }

        public Color background = Color.Black;

        private readonly Shader _bloomFirstPass = Shader.FromString(
            File.ReadAllText(Path.Join("resources", "bloom_vert.glsl")), null,
            File.ReadAllText(Path.Join("resources", "bloom_frag.glsl")));

        private readonly Shader _bloomSecondPass = Shader.FromString(
            File.ReadAllText(Path.Join("resources", "bloom_vert.glsl")), null,
            File.ReadAllText(Path.Join("resources", "bloom_frag.glsl")));

        private readonly Image _icon;

        public readonly Dictionary<Vector2i, RenderCharacter> display;
        public readonly int height;

        public readonly string title;
        public readonly int width;

        private RenderTexture _bloomRT1;
        private RenderTexture _bloomRT2;
        private int _framerate;
        public Shader bloomBlend;

        public Func<Vector2i, RenderCharacter, (Vector2f position, RenderCharacter character)> charactersModifier;
        public Vector2i fontSize;
        public bool leftButtonPressed;
        public Vector2i mousePosition = new Vector2i(-1, -1);

        public Vector2f mousePositionF = new Vector2f(-1f, -1f);
        public EventHandler onWindowRecreated;
        public BitmapText text;
        public Vector2f textPosition;
        public RenderWindow window;
        public int windowHeight;
        public int windowWidth;

        public Renderer(string title, int width, int height, int framerate, bool fullscreen, string fontPath) {
            this.title = title;

            string[] fontMappingsLines = File.ReadAllLines(Path.Join(fontPath, "mappings.txt"));
            string[] fontSizeStr = fontMappingsLines[0].Split(',');
            fontSize = new Vector2i(int.Parse(fontSizeStr[0]), int.Parse(fontSizeStr[1]));

            this.width = width;
            this.height = height;

            display = new Dictionary<Vector2i, RenderCharacter>(this.width * this.height);

            _icon = new Image(Path.Join("resources", "icon.png"));

            BitmapFont font = new BitmapFont(new Image(Path.Join(fontPath, "font.png")), fontMappingsLines[1],
                fontSize);
            text = new BitmapText(font, new Vector2i(width, height)) { text = display };

            SetFullscreen(fullscreen);

            this.framerate = framerate;

            _bloomFirstPass.SetUniform("horizontal", true);
            _bloomSecondPass.SetUniform("horizontal", false);
        }

        public int framerate {
            get => _framerate;
            set {
                _framerate = value;
                window.SetFramerateLimit(value < 0 ? 0 : (uint)value);
                window.SetVerticalSyncEnabled(value < 0);
            }
        } // ReSharper disable once FieldCanBeMadeReadOnly.Global

        public void SetFramerate(int framerate) => this.framerate = window.HasFocus() ? framerate < 60 ? -1 :
            framerate > 960 ? 0 : framerate : 30;

        private void SubscribeWindowEvents() {
            leftButtonPressed = false;

            onWindowRecreated?.Invoke(this, EventArgs.Empty);

            window.MouseMoved += UpdateMousePosition;
            window.MouseButtonPressed += (_, e) => {
                if(e.Button == Mouse.Button.Left) leftButtonPressed = true;
            };
            window.MouseButtonReleased += (_, e) => {
                if(e.Button == Mouse.Button.Left) leftButtonPressed = false;
            };
            window.SetKeyRepeatEnabled(false);
        }

        public void SetFullscreen(bool fullscreen) {
            if(window.IsOpen) window.Close();
            if(!fullscreen) {
                windowWidth = width * fontSize.X;
                windowHeight = height * fontSize.Y;
            }

            window = fullscreen
                ? new RenderWindow(VideoMode.FullscreenModes[0], title, Styles.Fullscreen)
                : new RenderWindow(new VideoMode((uint)windowWidth, (uint)windowHeight), title, Styles.Close);
            window.SetIcon(_icon.Size.X, _icon.Size.Y, _icon.Pixels);
            SubscribeWindowEvents();
            UpdateWindow(fullscreen, framerate);
        }

        public void UpdateWindow(bool fullscreen, int framerate) {
            if(fullscreen) {
                VideoMode videoMode = VideoMode.FullscreenModes[0];

                windowWidth = (int)videoMode.Width;
                windowHeight = (int)videoMode.Height;
            }
            else {
                window.Size = new Vector2u((uint)windowWidth, (uint)windowHeight);
                window.SetView(new View(new Vector2f(windowWidth / 2f, windowHeight / 2f),
                    new Vector2f(windowWidth, windowHeight)));
            }

            _bloomRT1 = new RenderTexture((uint)windowWidth, (uint)windowHeight);
            _bloomRT2 = new RenderTexture((uint)windowWidth, (uint)windowHeight);
            textPosition = new Vector2f((windowWidth - text.imageWidth) / 2f, (windowHeight - text.imageHeight) / 2f);

            this.framerate = framerate;
        }

        private void UpdateMousePosition(object caller, MouseMoveEventArgs mouse) {
            if(!window.HasFocus()) {
                mousePosition = new Vector2i(-1, -1);
                return;
            }

            mousePositionF = new Vector2f((mouse.X - windowWidth / 2f + text.imageWidth / 2f) / fontSize.X,
                (mouse.Y - windowHeight / 2f + text.imageHeight / 2f) / fontSize.Y);
            mousePosition = new Vector2i((int)mousePositionF.X, (int)mousePositionF.Y);
        }

        public void Clear() => display.Clear();

        public void Draw(bool bloom) {
            text.RebuildQuads(textPosition, charactersModifier);

            if(bloom) {
                _bloomRT1.Clear(background);
                text.DrawQuads(_bloomRT1);

                _bloomFirstPass.SetUniform("image", _bloomRT1.Texture);
                _bloomRT2.Clear(background);
                _bloomRT2.Draw(new Sprite(_bloomRT1.Texture), new RenderStates(_bloomFirstPass));

                _bloomSecondPass.SetUniform("image", _bloomRT2.Texture);
                _bloomRT1.Clear(background);
                _bloomRT1.Draw(new Sprite(_bloomRT2.Texture), new RenderStates(_bloomSecondPass));

                _bloomRT2.Clear(background);
                text.DrawQuads(_bloomRT2);

                _bloomRT1.Display();
                _bloomRT2.Display();

                bloomBlend.SetUniform("imageA", _bloomRT2.Texture);
                bloomBlend.SetUniform("imageB", _bloomRT1.Texture);
                window.Draw(new Sprite(_bloomRT1.Texture), new RenderStates(bloomBlend));
            }
            else {
                window.Clear(background);
                text.DrawQuads(window);
            }
        }

        public void DrawText(Vector2i position, string text, Color foregroundColor, Color backgroundColor,
            Alignment align = Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier = null) {
            switch(text.Length) {
                case 0: return; // Don't do anything if the text is empty
                case 1: {
                    if(!replacingSpaces && text[0] == ' ') return;
                    DrawTextChar(position, text[0], foregroundColor, backgroundColor, invertOnDarkBG, charactersModifier);
                    return;
                }
            }

            int posX = position.X - align switch {
                Alignment.Right => text.Length - 1,
                Alignment.Center => (int)MathF.Floor(text.Length / 2f),
                _ => 0
            };

            int x = 0;
            foreach(char curChar in text) {
                if(!replacingSpaces && curChar == ' ') {
                    x++;
                    continue;
                }

                Vector2i charPos = new Vector2i(posX + x, position.Y);
                DrawTextChar(charPos, curChar, foregroundColor, backgroundColor, invertOnDarkBG, charactersModifier);
                x++;
            }
        }

        private void DrawTextChar(Vector2i position, char character, Color foregroundColor, Color backgroundColor,
            bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier = null) {
            Color useFG = foregroundColor;
            if(invertOnDarkBG) {
                Color useBGColor = backgroundColor.A == 0 ? GetBackgroundColor(position) : backgroundColor;
                float luma = 0.299f * useBGColor.R + 0.587f * useBGColor.G + 0.114f * useBGColor.B;
                if(luma < 127.5f)
                    useFG = new Color(255, 255, 255, foregroundColor.A) -
                            new Color(foregroundColor.R, foregroundColor.G,
                                foregroundColor.B, 0);
            }

            Vector2i usePos = position;
            RenderCharacter useChar = new RenderCharacter(character, backgroundColor, useFG);
            if(charactersModifier != null) (usePos, useChar) = charactersModifier(position, useChar);
            SetCharacter(usePos, useChar);
        }

        public void DrawText(Vector2i position, string[] lines, Color foregroundColor, Color backgroundColor,
            Alignment align = Alignment.Left, bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => DrawLines(position, lines, foregroundColor, backgroundColor, align, replacingSpaces,
            invertOnDarkBG, charactersModifier);

        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public void DrawLines(Vector2i position, string[] lines, Color foregroundColor, Color backgroundColor,
            Alignment align = Alignment.Left,
            bool replacingSpaces = false,
            bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier = null) {
            for(int i = 0; i < lines.Length; i++)
                DrawText(position + new Vector2i(0, i), lines[i], foregroundColor, backgroundColor,
                    align, replacingSpaces, invertOnDarkBG, charactersModifier);
        }

        public void SetCharacter(Vector2i position, RenderCharacter character) {
            if(position.X < 0 || position.Y < 0 || position.X >= width || position.Y >= height) return;

            Color currentBackground = GetBackgroundColor(position);
            Color background = OverlayColors(currentBackground, character.background);
            Color foreground = OverlayColors(background, character.foreground);
            display[position] = new RenderCharacter(background, foreground, character);
            if(IsRenderCharacterEmpty(display[position])) display.Remove(position);
        }

        public RenderCharacter GetCharacter(Vector2i position) => display.ContainsKey(position) ? display[position] :
            new RenderCharacter('\0', Color.Transparent, Color.Transparent);

        public char GetDisplayedCharacter(Vector2i position) =>
            !display.ContainsKey(position) ? '\0' : display[position].character;

        public void SetCellColor(Vector2i position, Color foregroundColor, Color backgroundColor) =>
            SetCharacter(position, new RenderCharacter(backgroundColor, foregroundColor, GetCharacter(position)));

        public Color GetBackgroundColor(Vector2i position) {
            if(position.X < 0 || position.Y < 0 || position.X >= width || position.Y >= height ||
               !display.ContainsKey(position)) return background;
            return display[position].background;
        }

        private static bool IsRenderCharacterEmpty(RenderCharacter renderCharacter) =>
            renderCharacter.background.A == 0 &&
            (renderCharacter.character == '\0' || renderCharacter.character == ' ' ||
             renderCharacter.foreground.A == 0);

        public static Color LerpColors(Color a, Color b, float t) => t <= 0f ? a :
            t >= 1f ? b :
            new Color((byte)MathF.Floor(a.R + (b.R - a.R) * t),
                (byte)MathF.Floor(a.G + (b.G - a.G) * t),
                (byte)MathF.Floor(a.B + (b.B - a.B) * t),
                (byte)MathF.Floor(a.A + (b.A - a.A) * t));

        public static Color AnimateColor(float time, Color start, Color end, float rate) =>
            LerpColors(start, end, time * rate);

        public static Color OverlayColors(Color bottom, Color top) {
            float t = (255f - top.A) * bottom.A / 255f;
            float a = t + top.A;

            float r = (t * bottom.R + top.A * (float)top.R) / a;
            float g = (t * bottom.G + top.A * (float)top.G) / a;
            float b = (t * bottom.B + top.A * (float)top.B) / a;

            return new Color((byte)r, (byte)g, (byte)b, (byte)a);
        }

        [Obsolete("BlendsColors was renamed to OverlayColors and is now deprecated, use OverlayColors instead.")]
        public static Color BlendColors(Color bottom, Color top) => OverlayColors(bottom, top);
    }
}
