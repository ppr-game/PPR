﻿using System;
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
    public Dictionary<Vector2Int, RenderCharacter> text { get; }
    public IFont? font { get; }
    public uint imageWidth { get; }
    public uint imageHeight { get; }

    private readonly uint _charWidth;
    private readonly uint _charHeight;
    private readonly Vertex[] _quads;
    private readonly Texture _texture;
    private readonly Vector2f[] _backgroundCharacter;
    private readonly Dictionary<(char, RenderStyle), Vector2f[]> _characters;

    public Text(IFont? font, Vector2Int size, Dictionary<Vector2Int, RenderCharacter> text) {
        this.font = font;
        this.text = text;
        _texture = font is null ? new Texture(0, 0) : new Texture(SfmlConverters.ToSfmlImage(font.image));
        _charWidth = (uint)(font?.size.x ?? 0);
        _charHeight = (uint)(font?.size.y ?? 0);
        uint textWidth = (uint)size.x;
        uint textHeight = (uint)size.y;
        imageWidth = textWidth * _charWidth;
        imageHeight = textHeight * _charHeight;
        _quads = new Vertex[8 * textWidth * textHeight];
        _backgroundCharacter = font?.backgroundCharacter.Select(vector => SfmlConverters.ToSfmlVector2(vector))
            .ToArray() ?? Array.Empty<Vector2f>();
        _characters = font?.characters.ToDictionary(pair => pair.Key, pair =>
            pair.Value.Select(vector => SfmlConverters.ToSfmlVector2(vector)).ToArray()) ??
                      new Dictionary<(char, RenderStyle), Vector2f[]>();
    }

    public void RebuildQuads(Vector2f offset, List<IEffect> fullscreenEffects,
        Dictionary<Vector2Int, IEffect> effects) {
        uint index = 0;
        foreach((Vector2Int pos, RenderCharacter character) in text) {
            (Vector2 position, RenderCharacter character) mod = (new Vector2(pos.x, pos.y), character);
            if(effects.TryGetValue(pos, out IEffect? effect)) IEffect.ApplyModifiers(effect, pos, ref mod);
            foreach(IEffect fullscreenEffect in fullscreenEffects)
                IEffect.ApplyModifiers(fullscreenEffect, pos, ref mod);

            Vector2f position =
                new(mod.position.x * _charWidth + offset.X, mod.position.y * _charHeight + offset.Y);

            _quads[index].Position = position;
            _quads[index + 1].Position = position + new Vector2f(_charWidth, 0f);
            _quads[index + 2].Position = position + new Vector2f(_charWidth, _charHeight);
            _quads[index + 3].Position = position + new Vector2f(0f, _charHeight);

            _quads[index].TexCoords = _backgroundCharacter[0];
            _quads[index + 1].TexCoords = _backgroundCharacter[1];
            _quads[index + 2].TexCoords = _backgroundCharacter[2];
            _quads[index + 3].TexCoords = _backgroundCharacter[3];

            Color background = SfmlConverters.ToSfmlColor(mod.character.background);
            _quads[index].Color = background;
            _quads[index + 1].Color = background;
            _quads[index + 2].Color = background;
            _quads[index + 3].Color = background;

            if(_characters.TryGetValue((mod.character.character, mod.character.style & RenderStyle.AllPerFont),
                   out Vector2f[]? texCoords)) {
                bool italic = (mod.character.style & RenderStyle.Italic) != 0;
                Vector2f italicOffset = italic ? new Vector2f(1f, 0f) : new Vector2f(0f, 0f);

                _quads[index + 4].Position = _quads[index].Position + italicOffset;
                _quads[index + 5].Position = _quads[index + 1].Position + italicOffset;
                _quads[index + 6].Position = _quads[index + 2].Position;
                _quads[index + 7].Position = _quads[index + 3].Position;

                _quads[index + 4].TexCoords = texCoords[0];
                _quads[index + 5].TexCoords = texCoords[1];
                _quads[index + 6].TexCoords = texCoords[2];
                _quads[index + 7].TexCoords = texCoords[3];

                Color foreground = SfmlConverters.ToSfmlColor(mod.character.foreground);
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

    public void DrawQuads(RenderTarget target, BlendMode blendMode, Shader? shader = null) {
        shader?.SetUniform("font", _texture);
        target.Draw(_quads, 0, (uint)(text.Count * 8), PrimitiveType.Quads,
            new RenderStates(blendMode, Transform.Identity, _texture, shader));
    }

    public void DrawFont(RenderTarget target) => target.Draw(new Sprite(_texture));

    public void Dispose() {
        _texture.Dispose();
        GC.SuppressFinalize(this);
    }
}
