namespace PER.Abstractions;

public interface IGame {
    public void Setup();
    public void Update();
    public void Tick();
    public void Finish();
}
