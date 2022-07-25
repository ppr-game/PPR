using System;

using JetBrains.Annotations;

namespace PER.Abstractions.Resources;

[PublicAPI]
public readonly struct ResourcePackData : IEquatable<ResourcePackData> {
    public string name { get; }
    public string fullPath { get; }
    public ResourcePackMeta meta { get; }

    public ResourcePackData(string name, string fullPath, ResourcePackMeta meta) {
        this.name = name;
        this.fullPath = fullPath;
        this.meta = meta;
    }

    public bool Equals(ResourcePackData other) => fullPath == other.fullPath;
    public override bool Equals(object? obj) => obj is ResourcePackData other && Equals(other);
    public override int GetHashCode() => fullPath.GetHashCode();
    public static bool operator ==(ResourcePackData left, ResourcePackData right) => left.Equals(right);
    public static bool operator !=(ResourcePackData left, ResourcePackData right) => !left.Equals(right);
}
