namespace PER.Abstractions.Resources;

public interface IResource {
    public void Load(string id, IResources resources);
    public void Unload(string id, IResources resources);
}
