namespace PER.Abstractions.Resources;

public interface IResource {
    public bool Load(string id, IResources resources);
    public bool Unload(string id, IResources resources);
}
