using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using JetBrains.Annotations;

using PER.Abstractions.Resources;

namespace PER.Common.Resources;

[PublicAPI]
public class ResourcesManager : IResources {
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

    public virtual void Load() {
        if(_loading)
            throw new InvalidOperationException("Already loading.");
        if(loaded)
            return;
        _loading = true;

        foreach((string? id, IResource? resource) in _resources)
            resource.Load(id, this);

        _loading = false;
        loaded = true;
    }

    public virtual void Unload() {
        if(!loaded)
            return;

        foreach((string? id, IResource? resource) in _resources)
            resource.Unload(id, this);

        _loadedPacks.Clear();
        _cachedPaths.Clear();
        _resources.Clear();

        loaded = false;
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
        data = default(ResourcePackData);
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

    public bool TryAddPacksByNames(params string[] names) {
        bool success = true;
        ImmutableDictionary<string, ResourcePackData> availablePacks =
            GetAvailablePacks().ToImmutableDictionary(data => data.name);
        foreach(string name in names) {
            if(!availablePacks.TryGetValue(name, out ResourcePackData data))
                continue;
            success &= TryAddPack(data);
        }
        return success;
    }

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

    public bool TryGetPath(string relativePath, [NotNullWhen(true)] out string? fullPath) {
        fullPath = null;
        if(!loaded && !_loading) return false;

        if(_cachedPaths.TryGetValue(relativePath, out fullPath))
            return true;

        fullPath = GetAllPathsReverse(relativePath).FirstOrDefault((string?)null);
        if(fullPath is null) return false;
        _cachedPaths.Add(relativePath, fullPath);
        return true;
    }

    public bool TryGetResource<TResource>(string id, [NotNullWhen(true)] out TResource? resource)
        where TResource : class?, IResource? {
        resource = null;
        if(!_resources.TryGetValue(id, out IResource? cachedResource) ||
           cachedResource is not TResource actualResource)
            return false;

        resource = actualResource;
        return true;
    }
}
