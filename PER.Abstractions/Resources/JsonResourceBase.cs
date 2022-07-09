using System;

using JetBrains.Annotations;

namespace PER.Abstractions.Resources;

[PublicAPI]
public abstract class JsonResourceBase<T> : IResource {
    public abstract void Load(string id, IResources resources);

    protected void DeserializeAllJson(IResources resources, string relativePath,
        T deserialized, Func<bool> done) {
        foreach(string path in resources.GetAllPathsReverse(relativePath)) {
            DeserializeJson(path, deserialized);
            if(done())
                break;
        }
    }

    protected abstract void DeserializeJson(string path, T deserialized);

    public abstract void Unload(string id, IResources resources);
}
