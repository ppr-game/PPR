using System;
using System.Collections.Generic;
using System.Linq;

using PER.Abstractions.Rendering;
using PER.Util;

using SFML.Graphics;
using SFML.System;

using BlendMode = SFML.Graphics.BlendMode;
using Color = SFML.Graphics.Color;
using Shader = SFML.Graphics.Shader;

namespace PRR.Sfml;

public class Text : IDisposable {
    public uint imageWidth { get; }
    public uint imageHeight { get; }

    private readonly RenderCharacter[,] _display;
    private readonly HashSet<Vector2Int> _displayUsed;
    private readonly uint _charWidth;
    private readonly uint _charHeight;
    private readonly Vector2f _charBottomRight;
    private readonly Vector2f _charTopRight;
    private readonly Vector2f _charTopLeft;
    private readonly Vertex[] _quads;
    private readonly Texture _texture;
    private readonly Vector2f[] _backgroundCharacter;
    private readonly Dictionary<(char, RenderStyle), Vector2f[]> _characters;

    private uint _quadCount;

    public Text(IFont? font, Vector2Int size, RenderCharacter[,] display, HashSet<Vector2Int> displayUsed) {
        _display = display;
        _displayUsed = displayUsed;
        _texture = font is null ? new Texture(0, 0) : new Texture(SfmlConverters.ToSfmlImage(font.image));
        _charWidth = (uint)(font?.size.x ?? 0);
        _charHeight = (uint)(font?.size.y ?? 0);
        _charBottomRight = new Vector2f(_charWidth, 0f);
        _charTopRight = new Vector2f(_charWidth, _charHeight);
        _charTopLeft = new Vector2f(0f, _charHeight);
        uint textWidth = (uint)size.x;
        uint textHeight = (uint)size.y;
        imageWidth = textWidth * _charWidth;
        imageHeight = textHeight * _charHeight;
        _quads = new Vertex[8 * textWidth * textHeight];
        _backgroundCharacter = font?.backgroundCharacter.Select(SfmlConverters.ToSfmlVector2).ToArray() ??
            Array.Empty<Vector2f>();
        _characters = font?.characters.ToDictionary(pair => pair.Key, pair =>
            pair.Value.Select(SfmlConverters.ToSfmlVector2).ToArray()) ??
                      new Dictionary<(char, RenderStyle), Vector2f[]>();
    }

    public void RebuildQuads(Vector2f offset, List<IEffect> globalModEffects, Dictionary<Vector2Int, IEffect> effects) {
        uint index = 0;
        foreach(Vector2Int pos in _displayUsed) {
            RenderCharacter character = _display[pos.y, pos.x];
            Vector2 modPosition = new(pos.x, pos.y);

            if(effects.TryGetValue(pos, out IEffect? effect) && effect.hasModifiers)
                effect.ApplyModifiers(pos, ref modPosition, ref character);

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach(IEffect globalEffect in globalModEffects)
                globalEffect.ApplyModifiers(pos, ref modPosition, ref character);

            Vector2f position = new(modPosition.x * _charWidth + offset.X, modPosition.y * _charHeight + offset.Y);
            Color background = SfmlConverters.ToSfmlColor(character.background);

            _quads[index].Position = position;
            _quads[index].Color = background;
            _quads[index].TexCoords = _backgroundCharacter[0];

            _quads[index + 1].Position = position + _charBottomRight;
            _quads[index + 1].Color = background;
            _quads[index + 1].TexCoords = _backgroundCharacter[1];

            _quads[index + 2].Position = position + _charTopRight;
            _quads[index + 2].Color = background;
            _quads[index + 2].TexCoords = _backgroundCharacter[2];

            _quads[index + 3].Position = position + _charTopLeft;
            _quads[index + 3].Color = background;
            _quads[index + 3].TexCoords = _backgroundCharacter[3];

            index += 4;

            if(!_characters.TryGetValue((character.character, character.style & RenderStyle.AllPerFont),
                out Vector2f[]? texCoords))
                continue;

            bool italic = (character.style & RenderStyle.Italic) != 0;
            Vector2f italicOffset = new(italic.ToByte(), 0f);
            Color foreground = SfmlConverters.ToSfmlColor(character.foreground);

            _quads[index].Position = position + italicOffset;
            _quads[index].Color = foreground;
            _quads[index].TexCoords = texCoords[0];

            _quads[index + 1].Position = position + _charBottomRight + italicOffset;
            _quads[index + 1].Color = foreground;
            _quads[index + 1].TexCoords = texCoords[1];

            _quads[index + 2].Position = position + _charTopRight;
            _quads[index + 2].Color = foreground;
            _quads[index + 2].TexCoords = texCoords[2];

            _quads[index + 3].Position = position + _charTopLeft;
            _quads[index + 3].Color = foreground;
            _quads[index + 3].TexCoords = texCoords[3];

            index += 4;
        }
        _quadCount = index;
    }

    public void DrawQuads(RenderTarget target, BlendMode blendMode, Shader? shader = null) {
        shader?.SetUniform("font", _texture);
        target.Draw(_quads, 0, _quadCount, PrimitiveType.Quads,
            new RenderStates(blendMode, Transform.Identity, _texture, shader));
    }

    public void DrawFont(RenderTarget target) => target.Draw(new Sprite(_texture));

    public void Dispose() {
        _texture.Dispose();
        GC.SuppressFinalize(this);
    }
}
