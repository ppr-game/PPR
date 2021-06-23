namespace PER.Abstractions {
    public interface IRenderer {
        public void SetFramerate(int framerate);
        public void SetFullscreen(bool fullscreen);
        public void Clear();
        public void Draw();
    }
}
