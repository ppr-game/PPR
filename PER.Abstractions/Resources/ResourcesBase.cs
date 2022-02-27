using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PER.Abstractions.Resources;

public abstract class ResourcesBase : IResources {
    public int currentVersion => 0;
    public bool loaded { get; private set; }
    public IReadOnlyList<ResourcePackData> loadedPacks => _loadedPacks;
    public virtual string defaultPackName => "Default";
    protected virtual string resourcesRoot => "resources";
    protected virtual string resourcePackMeta => "metadata.json";
    protected virtual string resourcesInPack => "resources";

    private bool _loading;

    private readonly List<ResourcePackData> _loadedPacks = new();
    private readonly Dictionary<string, IResource> _resources = new();
    private readonly Dictionary<string, string> _cachedPaths = new();

    public virtual bool Load() {
        if(_loading) return false;
        if(loaded) return true;
        _loading = true;

        foreach((string? id, IResource? resource) in _resources) {
            if(resource.Load(id, this)) continue;
            _loading = false;
            return false;
        }

        _loading = false;
        loaded = true;
        return true;
    }

    public virtual bool Unload() {
        if(!loaded) return true;

        foreach((string? id, IResource? resource) in _resources) {
            if(resource.Unload(id, this)) continue;
            return false;
        }

        _loadedPacks.Clear();
        _cachedPaths.Clear();
        _resources.Clear();

        loaded = false;
        return true;
    }

    public IEnumerable<ResourcePackData> GetAvailablePacks() {
        if(!Directory.Exists(resourcesRoot)) yield break;

        foreach(string pack in Directory.GetDirectories(resourcesRoot)) {
            if(!TryGetPackData(pack, out ResourcePackData data)) continue;
            if(data.meta.version != currentVersion) continue;
            yield return data;
        }
    }

    public IEnumerable<ResourcePackData> GetUnloadedAvailablePacks() {
        IEnumerable<string> loadedPackNames = loadedPacks.Select(packData => packData.name);
        return GetAvailablePacks().Where(data => !loadedPackNames.Contains(data.name));
    }

    private bool TryGetPackData(string pack, out ResourcePackData data) {
        string metaPath = Path.Combine(pack, resourcePackMeta);
        data = default;
        if(!File.Exists(metaPath)) return false;
        string metaText = File.ReadAllText(metaPath);
        ResourcePackMeta meta = JsonSerializer.Deserialize<ResourcePackMeta>(metaText);
        data = new ResourcePackData(Path.GetFileName(pack), Path.Combine(pack, resourcesInPack), meta);
        return true;
    }

    public bool TryAddPack(ResourcePackData data) {
        if(loaded || _loading) return false;
        _loadedPacks.Add(data);
        return true;
    }

    public bool TryAddResource<TResource>(string id, TResource resource) where TResource : class, IResource =>
        !loaded && _resources.TryAdd(id, resource);

    public IEnumerable<string> GetAllPaths(string relativePath) {
        if(!loaded && !_loading) yield break;

        foreach(ResourcePackData data in loadedPacks) {
            string resourcePath = Path.Combine(data.fullPath, relativePath);
            if(!File.Exists(resourcePath)) continue;
            yield return resourcePath;
        }
    }

    public IEnumerable<string> GetAllPathsReverse(string relativePath) {
        if(!loaded && !_loading) yield break;

        for(int i = loadedPacks.Count - 1; i >= 0; i--) {
            ResourcePackData data = loadedPacks[i];
            string resourcePath = Path.Combine(data.fullPath, relativePath);
            if(!File.Exists(resourcePath)) continue;
            yield return resourcePath;
        }
    }

    public bool TryGetPath(string relativePath, [MaybeNullWhen(false)] out string fullPath) {
        fullPath = null;
        if(!loaded && !_loading) return false;

        if(_cachedPaths.TryGetValue(relativePath, out fullPath))
            return true;

        fullPath = GetAllPathsReverse(relativePath).FirstOrDefault((string?)null);
        if(fullPath is null) return false;
        _cachedPaths.Add(relativePath, fullPath);
        return true;
    }

    public bool TryGetResource<TResource>(string id, [MaybeNullWhen(false)] out TResource resource)
        where TResource : class?, IResource? {
        resource = null;
        if(!loaded) return false;

        if(!_resources.TryGetValue(id, out IResource? cachedResource) ||
           cachedResource is not TResource actualResource) return false;

        resource = actualResource;
        return true;
    }
}
