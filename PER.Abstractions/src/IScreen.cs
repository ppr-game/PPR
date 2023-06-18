using JetBrains.Annotations;

namespace PER.Abstractions;

[PublicAPI]
public interface IScreen : IUpdatable, ITickable {
    public void Open();
    public void Close();
}
