using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PER.Abstractions.Resources;

public interface IResources {
    public int currentVersion { get; }
    public IReadOnlyList<ResourcePackData> loadedPacks { get; }

    public void Load();
    public void Unload();

    public IEnumerable<ResourcePackData> GetAvailablePacks();
    public IEnumerable<ResourcePackData> GetUnloadedAvailablePacks();

    public bool TryAddPack(ResourcePackData data);
    public bool TryGetResource(string relativePath, [MaybeNullWhen(false)] out string fullPath);
}
