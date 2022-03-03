using System;

namespace PER.Abstractions.Resources;

public abstract class JsonResourceBase<T> : IResource {
    public abstract bool Load(string id, IResources resources);

    protected bool DeserializeAllJson(IResources resources, string relativePath,
        T deserialized, Func<bool> done) {
        foreach(string path in resources.GetAllPathsReverse(relativePath)) {
            if(!DeserializeJson(path, deserialized)) return false;
            if(done()) break;
        }

        return true;
    }

    protected abstract bool DeserializeJson(string path, T deserialized);

    public abstract bool Unload(string id, IResources resources);
}
