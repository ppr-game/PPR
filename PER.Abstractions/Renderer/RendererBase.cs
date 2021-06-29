using System;
using System.Collections.Generic;

using PER.Util;

namespace PER.Abstractions.Renderer {
    public abstract class RendererBase : IRenderer {
        public virtual string title { get; private set; }
        public virtual int width { get; private set; }
        public virtual int height { get; private set; }

        public virtual int framerate {
            get => _framerate;
            set {
                _framerate = value;
                UpdateFramerate();
            }
        }

        public virtual bool fullscreen {
            get => _fullscreen;
            set {
                _fullscreen = value;
                Reset();
            }
        }

        public virtual IFont font {
            get => _font;
            set {
                _font = value;
                Reset();
            }
        }

        public virtual string icon { get; set; }

        public abstract bool open { get; }
        public abstract bool focused { get; }

        public virtual Color background { get; set; } = Color.black;

        public virtual Vector2Int mousePosition { get; protected set; } = new(-1, -1);
        public virtual Vector2 accurateMousePosition { get; protected set; } = new(-1f, -1f);
        
        public virtual List<IEffectContainer> ppEffects { get; private set; }

        protected Dictionary<Vector2Int, RenderCharacter> display { get; private set; }
        protected Dictionary<Vector2Int, IEffectContainer> effects { get; private set; }

        private int _framerate;
        private bool _fullscreen;
        private IFont _font;

        public virtual void Setup(RendererSettings settings) {
            title = settings.title;
            width = settings.width;
            height = settings.height;
            _framerate = settings.framerate;
            _fullscreen = settings.fullscreen;
            _font = settings.font;
            icon = settings.icon;
            
            CreateWindow();
        }

        protected abstract void CreateWindow();
        protected abstract void UpdateFramerate();

        public abstract void Loop();
        public abstract void Stop();

        public virtual void Reset(RendererSettings settings) {
            Stop();
            Setup(settings);
        }

        public virtual void Reset() => Reset(new RendererSettings(this));

        protected virtual void UpdateFont() {
            display = new Dictionary<Vector2Int, RenderCharacter>(width * height);
            effects = new Dictionary<Vector2Int, IEffectContainer>(width * height);
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < width; y++) {
                    Vector2Int position = new(x, y);
                    effects.Add(position, CreateEffectContainer());
                }
            }

            ppEffects = new List<IEffectContainer>();
            
            CreateText();
        }

        protected abstract IEffectContainer CreateEffectContainer();
        protected abstract void CreateText();

        public virtual void Clear() => display.Clear();
        public abstract void Draw();

        public virtual void DrawCharacter(Vector2Int position, RenderCharacter character,
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
            
            if(IsRenderCharacterEmpty(character)) display.Remove(position);
            else display[position] = character;
        }

        public virtual void DrawText(Vector2Int position, string text, Color foregroundColor, Color backgroundColor,
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

        public virtual void DrawText(Vector2Int position, string[] lines, Color foregroundColor, Color backgroundColor,
            HorizontalAlignment align = HorizontalAlignment.Left, RenderStyle style = RenderStyle.None,
            RenderFlags flags = RenderFlags.Default) {
            for(int i = 0; i < lines.Length; i++)
                DrawText(position + new Vector2Int(0, i), lines[i], foregroundColor, backgroundColor,
                    align, style, flags);
        }

        public virtual RenderCharacter GetCharacter(Vector2Int position) => display.ContainsKey(position) ? display[position] :
            new RenderCharacter('\0', Color.transparent, Color.transparent);

        private bool IsRenderCharacterEmpty(RenderCharacter renderCharacter) =>
            renderCharacter.background.a == 0 &&
            (!CharacterExists(renderCharacter.character) || renderCharacter.foreground.a == 0);

        private bool CharacterExists(char character) => font.mappings.Contains(character);

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
