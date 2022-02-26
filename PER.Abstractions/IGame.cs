namespace PER.Abstractions;

public interface IGame {
    public void Load();
    public void Reload();
    public void Setup();
    public void Update();
    public void Tick();
    public void Finish();
}
