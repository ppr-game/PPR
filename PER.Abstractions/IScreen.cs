namespace PER.Abstractions;

public interface IScreen {
    public void Enter();
    public void Quit();
    public bool QuitUpdate();
    public void Open();
    public void Close();
    public void Update();
    public void Tick();
}
