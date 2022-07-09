using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace PER.Abstractions.Resources;

[PublicAPI]
public readonly struct ResourcePackMeta {
    public string description { get; }
    public int version { get; }

    [JsonConstructor]
    public ResourcePackMeta(string description, int version) {
        this.description = description;
        this.version = version;
    }
}
