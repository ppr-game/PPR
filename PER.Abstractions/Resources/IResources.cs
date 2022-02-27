using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PER.Abstractions.Resources;

public interface IResources {
    public int currentVersion { get; }
    public bool loaded { get; }
    public IReadOnlyList<ResourcePackData> loadedPacks { get; }
    public string defaultPackName { get; }

    public bool Load();
    public bool Unload();

    public IEnumerable<ResourcePackData> GetAvailablePacks();
    public IEnumerable<ResourcePackData> GetUnloadedAvailablePacks();

    public bool TryAddPack(ResourcePackData data);
    public bool TryAddResource<TResource>(string id, TResource resource) where TResource : class, IResource;

    public IEnumerable<string> GetAllPaths(string relativePath);
    public IEnumerable<string> GetAllPathsReverse(string relativePath);
    public bool TryGetPath(string relativePath, [MaybeNullWhen(false)] out string fullPath);
    public bool TryGetResource<TResource>(string id, [MaybeNullWhen(false)] out TResource resource)
        where TResource : class?, IResource?;
}
