namespace PER.Abstractions {
    public interface IGame {
        public void Setup();
        public void Loop();
        public void Tick();
        public void Stop();
    }
}
