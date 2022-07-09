using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Util;

using PRR.UI.Resources;

namespace PPR.Screens;

public abstract class MenuWithCoolBackgroundAnimationScreenResourceBase : ScreenResourceBase {
    // ReSharper disable once MemberCanBePrivate.Global AutoPropertyCanBeMadeGetOnly.Global
    public static int animBpm { get; set; } = 120;

    private static readonly Perlin perlin = new();
    private Color _animMax;

    public override void Load(string id, IResources resources) {
        base.Load(id, resources);
        if(colors is null || !colors.colors.TryGetValue("menus_anim_max", out Color menusAnimMax))
            menusAnimMax = Color.white;
        _animMax = menusAnimMax;
    }

    public override void Update() {
        // forcefully unblock input for the effect
        bool prevInputBlock = input.block;
        input.block = false;

        float time = (float)Core.engine.clock.time.TotalSeconds * animBpm / 120f;
        for(int x = -3; x < renderer.width + 3; x++) {
            for(int y = -3; y < renderer.height + 3; y++) {
                if(x % 3 != 0 || y % 3 != 0)
                    continue;
                DrawAnimationAt(x, y, time);
            }
        }

        input.block = prevInputBlock;
    }

    private void DrawAnimationAt(int x, int y, float time) {
        float noiseX = (float)perlin.Get(x / 10f, y / 10f, time / 2f) - 0.5f;
        float noiseY = (float)perlin.Get(x / 10f, y / 10f, time / 2f + 100f) - 0.5f;
        float noise = MathF.Abs(noiseX * noiseY);
        float xOffset = (input.accurateMousePosition.x / renderer.width - 0.5f) * noise * -100f;
        float yOffset = (input.accurateMousePosition.y / renderer.width - 0.5f) * noise * -100f;
        Color useColor = new(_animMax.r, _animMax.g, _animMax.b, MoreMath.Lerp(0f, _animMax.a, noise * 30f));
        float xPos = x + noiseX * 10f + xOffset;
        float yPos = y + noiseY * 10f + yOffset;
        int flooredX = (int)xPos;
        int flooredY = (int)yPos;
        for(int useX = flooredX; useX <= flooredX + 1; useX++) {
            for(int useY = flooredY; useY <= flooredY + 1; useY++) {
                float percentX = 1f - MathF.Abs(xPos - useX);
                float percentY = 1f - MathF.Abs(yPos - useY);
                float percent = percentX * percentY;
                Color posColor = new(useColor.r, useColor.g, useColor.b, MoreMath.Lerp(0f, useColor.a, percent));
                Vector2Int pos = new(useX, useY);
                RenderCharacter character = renderer.GetCharacter(pos);
                renderer.DrawCharacter(pos,
                    new RenderCharacter(character.character, posColor, character.foreground));
            }
        }
    }
}
