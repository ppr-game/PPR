using System;
using System.Collections.Generic;
using System.IO;

using PPR.Core;
using PPR.GUI;
using PPR.Rendering.Bitmap;
using PPR.Levels;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace PPR.Rendering {
    public class Renderer {
        public static Renderer instance;

        readonly Dictionary<Vector2, float> randomColorAnimationOffsets;

        readonly Dictionary<Vector2, Color> backgroundColors;
        readonly Dictionary<Vector2, Color> foregroundColors;
        readonly Dictionary<Vector2, char> displayString;
        public char[] charSet = { '#', '+', '-', '|', '\\', '/' };
        public readonly Vector2 fontSize;
        public readonly int width;
        public readonly int windowWidth;
        public readonly int height;
        public readonly int windowHeight;
        public int frameRate;
        readonly BitmapText text;
        public readonly RenderWindow window;
        public static Vector2 cameraPosition = Vector2.zero;
        readonly Shader bloom = Shader.FromString(File.ReadAllText(Path.Combine("resources", "bloom_vert.glsl")), null,
                                                                                                                     File.ReadAllText(Path.Combine("resources", "bloom_frag.glsl")));
        readonly RenderTexture bloomRT;
        readonly RenderTexture finalRT;

        public Vector2 mousePosition = new Vector2(-1, -1);

        public Renderer(int width, int height, int frameRate) {
            instance = this;

            fontSize = new Vector2(10, 10);

            this.width = width;
            windowWidth = width * fontSize.x;
            this.height = height;
            windowHeight = height * fontSize.y;
            this.frameRate = frameRate;

            randomColorAnimationOffsets = new Dictionary<Vector2, float>(this.width * this.height);

            backgroundColors = new Dictionary<Vector2, Color>(this.width * this.height);
            foregroundColors = new Dictionary<Vector2, Color>(this.width * this.height);
            displayString = new Dictionary<Vector2, char>(this.width * this.height);

            window = new RenderWindow(new VideoMode((uint)windowWidth, (uint)windowHeight), "Press Press Revolution");
            bloomRT = new RenderTexture((uint)windowWidth, (uint)windowHeight);
            finalRT = new RenderTexture((uint)windowWidth, (uint)windowHeight);
            window.Closed += (_, __) => window.Close();
            window.MouseMoved += UpdateMousePosition;
            if(frameRate < 0)
                window.SetVerticalSyncEnabled(true);
            else if(frameRate != 0)
                window.SetFramerateLimit((uint)frameRate);
            BitmapFont font = new BitmapFont(new Image(Path.Combine("resources", "font.png")), "\0☺☻♥♦♣♠•◘○◙♂♀♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼ !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~⌂ÇüéâäàåçêëèïîìÄÅÉæÆôöòûùÿÖÜ¢£¥₧ƒáíóúñÑªº¿⌐¬½¼¡«»░▒▓│┤╡╢╖╕╣║╗╝╜╛┐└┴┬├─┼╞╟╚╔╩╦╠═╬╧╨╤╥╙╘╒╓╫╪┘┌█▄▌▐▀αβΓπΣσµτΦΘΩδ∞φε∩≡±≥≤⌠⌡÷≈°∙·√ⁿ²■ ", fontSize);
            text = new BitmapText(font, new Vector2(width, height)) {
                backgroundColors = backgroundColors,
                foregroundColors = foregroundColors,
                text = displayString
            };
        }

        public void ClearText() {
            backgroundColors.Clear();
            foregroundColors.Clear();
            displayString.Clear();
        }
        void UpdateMousePosition(object caller, MouseMoveEventArgs mouse) {
            if(!window.HasFocus()) {
                mousePosition = new Vector2(-1, -1);
                return;
            }
            mousePosition = new Vector2((int)MathF.Floor(mouse.X / fontSize.x), (int)MathF.Floor(mouse.Y / fontSize.y));
        }
        public void Update() {
            ClearText();
            Map.Draw();
            UI.Draw();
            RenderImage();
        }
        public void RenderImage() {
            text.backgroundColors = backgroundColors;
            text.foregroundColors = foregroundColors;
            text.text = displayString;

            bloomRT.Clear();

            Sprite defSprite = new Sprite(text.renderTexture.Texture);

            Shader.Bind(bloom);

            bloom.SetUniform("image", defSprite.Texture);
            bloom.SetUniform("horizontal", false);
            bloomRT.Draw(new Sprite(defSprite), new RenderStates(bloom));
            bloomRT.Display();

            finalRT.Clear();

            bloom.SetUniform("image", bloomRT.Texture);
            bloom.SetUniform("horizontal", true);
            finalRT.Draw(new Sprite(bloomRT.Texture), new RenderStates(bloom));

            Shader.Bind(null);

            finalRT.Draw(defSprite, new RenderStates(BlendMode.Add));

            finalRT.Display();
        }
        public void Draw() {
            window.Clear();
            window.Draw(new Sprite(finalRT.Texture));
        }
        public void DrawText(Vector2 position, string text, Color foregroundColor, Color backgroundColor, bool replacingSpaces = true) {
            if(text.Length == 0) return; // Don't do anything, if the text is empty
            if(text.Length == 1) {
                SetCharacter(position, text[0], foregroundColor, backgroundColor);
            }

            string[] lines = text.Split('\n');
            int height = lines.Length;
            int index = 0;
            for(int l = 0; l < height; l++) {
                string curLine = lines[l];
                for(int lx = 0; lx < curLine.Length; lx++) {
                    char curChar = curLine[lx];
                    if(!replacingSpaces && curChar == ' ') continue;
                    SetCharacter(new Vector2(position.x + lx, position.y + l), curChar, foregroundColor, backgroundColor);
                    index++;
                }
                index++; // Every line has \n at the end, so we need to account that too, when counting the index
            }
        }

        public void SetCharacter(Vector2 position, char character) {
            if(position.InBounds(0, 0, width - 1, height - 1)) {
                // If a character is a space or null, remove it from a dictionary
                if(character == ' ' || character == '\0') _ = displayString.Remove(position);
                else displayString[position] = character;
            }
        }
        public void SetCharacter(Vector2 position, char character, Color foregroundColor, Color backgroundColor) {
            SetCharacter(position, foregroundColor == backgroundColor || foregroundColor.A == 0 ? ' ' : character);
            SetCellColor(position, foregroundColor,  backgroundColor);
        }
        public char GetCharacter(Vector2 position) {
            return !displayString.ContainsKey(position) ? '\0' : displayString[position];
        }
        public void SetCellColor(Vector2 position, Color foregroundColor, Color backgroundColor) {
            if(position.InBounds(0, 0, width - 1, height - 1)) {
                if(backgroundColor == Color.Black) _ = backgroundColors.Remove(position);
                else if(backgroundColor.A != 0) backgroundColors[position] = backgroundColor;

                if(foregroundColor == Color.White) _ = foregroundColors.Remove(position);
                else foregroundColors[position] = foregroundColor;
            }
        }
        public static Color LerpColors(Color a, Color b, float t) {
            return t <= 0f ? a : t >= 1f ? b : 
                new Color((byte)MathF.Floor(a.R + (b.R - a.R) * t), (byte)MathF.Floor(a.G + (b.G - a.G) * t), (byte)MathF.Floor(a.B + (b.B - a.B) * t), (byte)MathF.Floor(a.A + (b.A - a.A) * t));
        }
        public static Color AnimateColor(float time, Color start, Color end, float rate) {
            float t = Math.Clamp(time * rate, 0f, 1f);
            return LerpColors(start, end, t);
        }
        public void UpdateRandomColorAnimationOffset(Vector2 position, float min, float max) {
            if(position.InBounds(0, 0, width - 1, height - 1)) {
                randomColorAnimationOffsets[position] = new Random().NextFloat(min, max);
            }
        }
        public float GetRandomColorAnimationOffset(Vector2 position) {
            return !randomColorAnimationOffsets.ContainsKey(position) ? 0f : randomColorAnimationOffsets[position];
        }
        public Color GetCellBackgroundColor(Vector2 position) {
            return !backgroundColors.ContainsKey(position) ? Color.Black : backgroundColors[position];
        }
        public Color GetCellForegroundColor(Vector2 position) {
            return !foregroundColors.ContainsKey(position) ? Color.White : foregroundColors[position];
        }
    }
}
