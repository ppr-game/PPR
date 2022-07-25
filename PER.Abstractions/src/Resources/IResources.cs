using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace PER.Abstractions.Resources;

[PublicAPI]
public interface IResources {
    public int currentVersion { get; }
    public bool loaded { get; }
    public IReadOnlyList<ResourcePackData> loadedPacks { get; }
    public string defaultPackName { get; }

    public void Load();
    public void Unload();

    public IEnumerable<ResourcePackData> GetAvailablePacks();
    public IEnumerable<ResourcePackData> GetUnloadedAvailablePacks();

    public bool TryAddPack(ResourcePackData data);
    public bool TryAddResource<TResource>(string id, TResource resource) where TResource : Resource;

    public bool TryAddPacksByNames(params string[] names);

    public IEnumerable<string> GetAllPaths(string relativePath);
    public bool TryGetResource<TResource>(string id, [NotNullWhen(true)] out TResource? resource)
        where TResource : Resource?;
}
