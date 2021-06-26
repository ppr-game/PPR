﻿using PER.Util;

namespace PER.Abstractions.Renderer {
    public interface IRenderer {
        public string title { get; }
        public int width { get; }
        public int height { get; }
        public int framerate { get; set; }
        public bool fullscreen { get; set; }
        public string font { get; set; }
        public Vector2Int fontSize { get; }
        public string icon { get; set; }
        
        public bool open { get; }
        public bool focused { get; }
        
        public Color clear { get; set; }
        
        public Vector2Int mousePosition { get; }
        public Vector2 accurateMousePosition { get; }

        public void Setup(RendererSettings settings);
        public void Loop();
        public void Stop();
        public void Reset();
        public void Reset(RendererSettings settings);
        
        public void Clear();
        public void Draw();
        public void DrawCharacter(Vector2Int position, RenderCharacter character, RenderFlags flags);
        public void DrawText(Vector2Int position, string text, Color foregroundColor, Color backgroundColor,
            HorizontalAlignment align = HorizontalAlignment.Left, RenderFlags flags = RenderFlags.Default);
        public void DrawText(Vector2Int position, string[] lines, Color foregroundColor, Color backgroundColor,
            HorizontalAlignment align = HorizontalAlignment.Left, RenderFlags flags = RenderFlags.Default);

        public RenderCharacter GetCharacter(Vector2Int position);
    }
}
