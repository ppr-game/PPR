using System;

using PPR.Main;

using PRR;

using SFML.Graphics;
using SFML.System;

namespace PPR.Scripts.Rendering {
    public class Renderer {
        public static int width => PPR.Core.renderer.width;
        public static int height => PPR.Core.renderer.height;
        public static Vector2i mousePosition => PPR.Core.renderer.mousePosition;
        public static Vector2f mousePositionF => PPR.Core.renderer.mousePositionF;
        public static Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)>
            charactersModifier {
            get => PPR.Core.renderer.charactersModifier;
            set => PPR.Core.renderer.charactersModifier = value;
        }

        public static void DrawText(Vector2i position, string text, Color foregroundColor, Color backgroundColor,
            Color defaultBackground, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, text, foregroundColor, backgroundColor, defaultBackground,
            align, replacingSpaces, invertOnDarkBG, charactersModifier);
        
        public static void DrawText(Vector2i position, string[] lines, Color foregroundColor, Color backgroundColor,
            Color defaultBackground, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, lines, foregroundColor, backgroundColor,
            defaultBackground, align, replacingSpaces, invertOnDarkBG, charactersModifier);
        
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static void DrawLines(Vector2i position, string[] lines, Color foregroundColor, Color backgroundColor,
            Color defaultBackground, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawLines(position, lines, foregroundColor, backgroundColor,
            defaultBackground, align, replacingSpaces, invertOnDarkBG, charactersModifier);
        
        public static void SetCharacter(Vector2i position, RenderCharacter character, Color defaultBackground) =>
            PPR.Core.renderer.SetCharacter(position, character, defaultBackground);
        public static RenderCharacter GetCharacter(Vector2i position) => PPR.Core.renderer.GetCharacter(position);
        public static void SetCellColor(Vector2i position, Color foregroundColor, Color backgroundColor,
            Color defaultBackground) =>
            PPR.Core.renderer.SetCellColor(position, foregroundColor, backgroundColor, defaultBackground);
        public static Color LerpColors(Color a, Color b, float t) => PRR.Renderer.LerpColors(a, b, t);
        public static Color AnimateColor(float time, Color start, Color end, float rate) =>
            PRR.Renderer.AnimateColor(time, start, end, rate);
        public static Color BlendColors(Color bottom, Color top) => PRR.Renderer.BlendColors(bottom, top);
        
        public static void DrawText(Vector2i position, string text,
            PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, text, align,
            replacingSpaces, invertOnDarkBG, charactersModifier);
        
        public static void DrawText(Vector2i position, string text, Color color,
            PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, text, color, align, replacingSpaces,
            invertOnDarkBG, charactersModifier);
        
        public static void DrawText(Vector2i position, string text, Color foregroundColor, Color backgroundColor,
            PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, text, foregroundColor, backgroundColor,
            align, replacingSpaces, invertOnDarkBG, charactersModifier);
        
        public static void DrawText(Vector2i position, string[] lines, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, lines, align, replacingSpaces, invertOnDarkBG,
            charactersModifier);
        
        public static void DrawText(Vector2i position, string[] lines, Color color, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, lines, color, align, replacingSpaces, invertOnDarkBG,
            charactersModifier);
        
        public static void DrawText(Vector2i position, string[] lines, Color foregroundColor, Color backgroundColor,
            PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawText(position, lines, foregroundColor, backgroundColor, align,
            replacingSpaces,
            invertOnDarkBG, charactersModifier);
        
        public static void DrawLines(Vector2i position, string[] lines, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawLines(position, lines, align, replacingSpaces,
            invertOnDarkBG, charactersModifier);
        
        public static void DrawLines(Vector2i position, string[] lines, Color color, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawLines(position, lines, color, align, replacingSpaces,
            invertOnDarkBG, charactersModifier);
        
        public static void DrawLines(Vector2i position, string[] lines, Color foregroundColor,
            Color backgroundColor, PRR.Renderer.Alignment align = PRR.Renderer.Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false,
            Func<Vector2i, RenderCharacter, (Vector2i position, RenderCharacter character)> charactersModifier =
                null) => PPR.Core.renderer.DrawLines(position, lines, foregroundColor, backgroundColor, align,
            replacingSpaces, invertOnDarkBG, charactersModifier);
    }
}
