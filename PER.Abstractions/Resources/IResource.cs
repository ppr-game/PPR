using JetBrains.Annotations;

namespace PER.Abstractions.Resources;

[PublicAPI]
public interface IResource {
    public void Load(string id, IResources resources);
    public void Unload(string id, IResources resources);
}
