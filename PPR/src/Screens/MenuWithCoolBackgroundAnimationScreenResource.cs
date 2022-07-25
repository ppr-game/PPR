using PER.Abstractions;
using PER.Abstractions.Rendering;
using PER.Util;

using PRR.UI.Resources;

namespace PPR.Screens;

public abstract class MenuWithCoolBackgroundAnimationScreenResource : LayoutResource, IScreen {
    private static readonly Stopwatch clock = new();
    private Color _animMax;

    public override void Load(string id) {
        base.Load(id);
        clock.Reset();
        if(!colors.colors.TryGetValue("menus_anim_max", out Color menusAnimMax))
            menusAnimMax = Color.white;
        _animMax = menusAnimMax;
    }

    public virtual void Open() {
        Conductor.stateChanged += UpdateMusic;
        UpdateMusic(null, EventArgs.Empty);
    }

    public virtual void Close() => Conductor.stateChanged -= UpdateMusic;

    protected virtual void UpdateMusic(object? sender, EventArgs args) => clock.speed = Conductor.bpm / 240d;

    public virtual void Update(TimeSpan time) {
        // forcefully unblock input for the effect
        bool prevInputBlock = input.block;
        input.block = false;

        float animTime = (float)clock.time.TotalSeconds;
        float mouseX = input.accurateMousePosition.x / renderer.width - 0.5f;
        float mouseY = input.accurateMousePosition.y / renderer.width - 0.5f;
        for(int x = -3; x < renderer.width + 3; x += 3)
            for(int y = -3; y < renderer.height + 3; y += 3)
                DrawAnimationAt(x, y, mouseX, mouseY, animTime);

        input.block = prevInputBlock;
    }

    private void DrawAnimationAt(int x, int y, float mouseX, float mouseY, float time) {
        float scaledX = x / 10f;
        float scaledY = y / 10f;

        float noiseX = Perlin.Get(scaledX, scaledY, time) - 0.5f;
        float noiseY = Perlin.Get(scaledX, scaledY, time + 100f) - 0.5f;
        float noise = MathF.Abs(noiseX * noiseY);

        float xOffset = mouseX * noise * -100f;
        float yOffset = mouseY * noise * -100f;
        Color useColor = new(_animMax.r, _animMax.g, _animMax.b, MoreMath.Lerp(0f, _animMax.a, noise * 30f));

        float xPos = x + noiseX * 10f + xOffset;
        float yPos = y + noiseY * 10f + yOffset;

        int flooredX = (int)xPos;
        int flooredY = (int)yPos;

        Span<Vector2Int> positions = stackalloc Vector2Int[] {
            new(flooredX, flooredY),
            new(flooredX + 1, flooredY),
            new(flooredX, flooredY + 1),
            new(flooredX + 1, flooredY + 1)
        };

        float percentRight = xPos - flooredX;
        float percentLeft = 1f - percentRight;
        float percentBottom = yPos - flooredY;
        float percentTop = 1f - percentBottom;

        Span<Color> colors = stackalloc Color[] {
            new(useColor.r, useColor.g, useColor.b, MoreMath.Lerp(0f, useColor.a, percentLeft * percentTop)),
            new(useColor.r, useColor.g, useColor.b, MoreMath.Lerp(0f, useColor.a, percentRight * percentTop)),
            new(useColor.r, useColor.g, useColor.b, MoreMath.Lerp(0f, useColor.a, percentLeft * percentBottom)),
            new(useColor.r, useColor.g, useColor.b, MoreMath.Lerp(0f, useColor.a, percentRight * percentBottom))
        };

        for(int i = 0; i < 4; i++) {
            Vector2Int pos = positions[i];
            RenderCharacter character = renderer.GetCharacter(pos);
            renderer.DrawCharacter(pos, new RenderCharacter(character.character, colors[i], character.foreground));
        }
    }

    public abstract void Tick(TimeSpan time);
}
