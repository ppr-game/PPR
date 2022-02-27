namespace PER.Abstractions;

public interface IGame {
    public void Unload();
    public void Load();
    public void Loaded();
    public void Setup();
    public void Update();
    public void Tick();
    public void Finish();
}
