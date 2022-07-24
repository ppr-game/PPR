using System;

using JetBrains.Annotations;

namespace PER.Abstractions.Resources;

[PublicAPI]
public abstract class JsonResourceBase<T> : ResourceBase {
    protected void DeserializeAllJson(string id, T deserialized, Func<bool> done) {
        foreach(string path in GetPaths(id)) {
            DeserializeJson(path, deserialized);
            if(done())
                break;
        }
    }

    protected abstract void DeserializeJson(string path, T deserialized);
}
