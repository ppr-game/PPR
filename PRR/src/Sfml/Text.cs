using System;
using System.Collections.Generic;
using System.Linq;

using PER.Abstractions.Renderer;
using PER.Util;

using SFML.Graphics;
using SFML.System;

using BlendMode = SFML.Graphics.BlendMode;
using Color = SFML.Graphics.Color;
using Shader = SFML.Graphics.Shader;

namespace PRR.Sfml {
    public class Text : IDisposable {
        public Dictionary<Vector2Int, RenderCharacter> text { get; set; }
        public IFont font { get; }
        public uint imageWidth { get; }
        public uint imageHeight { get; }
        
        private readonly uint _charWidth;
        private readonly uint _charHeight;
        private readonly Vertex[] _quads;
        private readonly Texture _texture;
        private readonly Vector2f[] _backgroundCharacter;
        private readonly Dictionary<(char, RenderStyle), Vector2f[]> _characters;

        public Text(IFont font, Vector2Int size) {
            this.font = font;
            _texture = new Texture(SfmlConverters.ToSfmlImage(font.image));
            _charWidth = (uint)font.size.x;
            _charHeight = (uint)font.size.y;
            uint textWidth = (uint)size.x;
            uint textHeight = (uint)size.y;
            imageWidth = textWidth * _charWidth;
            imageHeight = textHeight * _charHeight;
            _quads = new Vertex[8 * textWidth * textHeight];
            _backgroundCharacter = font.backgroundCharacter.Select(vector => SfmlConverters.ToSfmlVector2(vector))
                .ToArray();
            _characters = font.characters.ToDictionary(pair => pair.Key, pair =>
                pair.Value.Select(vector => SfmlConverters.ToSfmlVector2(vector)).ToArray());
        }

        public void RebuildQuads(Vector2f offset) {
            uint index = 0;
            foreach((Vector2Int pos, RenderCharacter character) in text) {
                Vector2f position = new(pos.x * _charWidth + offset.X, pos.y * _charHeight + offset.Y);

                _quads[index].Position = position;
                _quads[index + 1].Position = position + new Vector2f(_charWidth, 0f);
                _quads[index + 2].Position = position + new Vector2f(_charWidth, _charHeight);
                _quads[index + 3].Position = position + new Vector2f(0f, _charHeight);

                _quads[index].TexCoords = _backgroundCharacter[0];
                _quads[index + 1].TexCoords = _backgroundCharacter[1];
                _quads[index + 2].TexCoords = _backgroundCharacter[2];
                _quads[index + 3].TexCoords = _backgroundCharacter[3];

                Color background = SfmlConverters.ToSfmlColor(character.background);
                _quads[index].Color = background;
                _quads[index + 1].Color = background;
                _quads[index + 2].Color = background;
                _quads[index + 3].Color = background;

                if(_characters.TryGetValue((character.character, character.style & RenderStyle.AllPerFont),
                    out Vector2f[] texCoords)) {
                    bool italic = character.style.HasFlag(RenderStyle.Italic);
                    Vector2f italicOffset = italic ? new Vector2f(1f, 0f) : new Vector2f(0f, 0f);
                    
                    _quads[index + 4].Position = _quads[index].Position + italicOffset;
                    _quads[index + 5].Position = _quads[index + 1].Position + italicOffset;
                    _quads[index + 6].Position = _quads[index + 2].Position;
                    _quads[index + 7].Position = _quads[index + 3].Position;

                    _quads[index + 4].TexCoords = texCoords[0];
                    _quads[index + 5].TexCoords = texCoords[1];
                    _quads[index + 6].TexCoords = texCoords[2];
                    _quads[index + 7].TexCoords = texCoords[3];

                    Color foreground = SfmlConverters.ToSfmlColor(character.foreground);
                    _quads[index + 4].Color = foreground;
                    _quads[index + 5].Color = foreground;
                    _quads[index + 6].Color = foreground;
                    _quads[index + 7].Color = foreground;
                }
                else {
                    _quads[index + 4].TexCoords = new Vector2f();
                    _quads[index + 5].TexCoords = new Vector2f();
                    _quads[index + 6].TexCoords = new Vector2f();
                    _quads[index + 7].TexCoords = new Vector2f();
                }

                index += 8;
            }
        }

        public void DrawQuads(RenderTarget target, BlendMode blendMode, Shader shader = null) => target.Draw(_quads, 0,
            (uint)(text.Count * 8), PrimitiveType.Quads,
            new RenderStates(blendMode, Transform.Identity, _texture, shader));

        public void DrawFont(RenderTarget target) => target.Draw(new Sprite(_texture));

        public void Dispose() {
            _texture?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
