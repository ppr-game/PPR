using JetBrains.Annotations;

namespace PER.Abstractions;

[PublicAPI]
public interface IScreen {
    public void Open();
    public void Close();
    public void Update();
    public void Tick();
}
