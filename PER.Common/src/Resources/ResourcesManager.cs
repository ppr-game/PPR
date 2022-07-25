using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using JetBrains.Annotations;

using NLog;

using PER.Abstractions.Resources;

namespace PER.Common.Resources;

[PublicAPI]
public class ResourcesManager : IResources {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public int currentVersion => 0;
    public bool loaded { get; private set; }
    public IReadOnlyList<ResourcePackData> loadedPacks => _loadedPacks;
    public virtual string defaultPackName => "Default";
    protected virtual string resourcesRoot => "resources";
    protected virtual string resourcePackMeta => "metadata.json";
    protected virtual string resourcesInPack => "resources";

    private bool _loading;

    private readonly List<ResourcePackData> _loadedPacks = new();
    private readonly Dictionary<string, Resource> _resources = new();
    private readonly Dictionary<string, string> _cachedPaths = new();

    public virtual void Load() {
        if(_loading)
            throw new InvalidOperationException("Already loading.");
        if(loaded)
            return;
        _loading = true;

        logger.Info("Loading resources");

        foreach((string id, Resource resource) in _resources) {
            logger.Info("Loading resource {Id}", id);
            resource.ResolveDependencies(this);
            resource.ResolvePaths(this);
            resource.Load(id);
        }

        _loading = false;
        loaded = true;

        logger.Info("Resources loaded");
    }

    public virtual void Unload() {
        if(!loaded)
            return;

        logger.Info("Unloading resources");

        foreach((string id, Resource resource) in _resources) {
            logger.Info("Unloading resource {Id}", id);
            resource.Unload(id);
        }

        _loadedPacks.Clear();
        _cachedPaths.Clear();
        _resources.Clear();

        loaded = false;

        logger.Info("Resources unloaded");
    }

    public IEnumerable<ResourcePackData> GetAvailablePacks() {
        if(!Directory.Exists(resourcesRoot)) {
            logger.Warn("Resources directory ({Directory}) missing", Path.GetFullPath(resourcesRoot));
            yield break;
        }

        foreach(string pack in Directory.GetDirectories(resourcesRoot)) {
            if(!TryGetPackData(pack, out ResourcePackData data) ||
                data.meta.version != currentVersion)
                continue;
            logger.Info("Found pack {Name}", data.name);
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
        if(!File.Exists(metaPath))
            return false;

        FileStream file = File.OpenRead(metaPath);
        ResourcePackMeta meta = JsonSerializer.Deserialize<ResourcePackMeta>(file);
        file.Close();

        data = new ResourcePackData(Path.GetFileName(pack), Path.Combine(pack, resourcesInPack), meta);
        return true;
    }

    public bool TryAddPack(ResourcePackData data) {
        if(loaded || _loading)
            return false;
        _loadedPacks.Add(data);
        logger.Info("Enabled pack {Name}", data.name);
        return true;
    }

    public bool TryAddResource<TResource>(string id, TResource resource) where TResource : Resource =>
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
        if(!loaded && !_loading)
            yield break;

        for(int i = loadedPacks.Count - 1; i >= 0; i--) {
            string resourcePath = Path.Combine(loadedPacks[i].fullPath, relativePath);
            if(File.Exists(resourcePath))
                yield return resourcePath;
        }
    }

    public bool TryGetResource<TResource>(string id, [NotNullWhen(true)] out TResource? resource)
        where TResource : Resource? {
        resource = null;
        if(!_resources.TryGetValue(id, out Resource? cachedResource) ||
           cachedResource is not TResource actualResource)
            return false;

        resource = actualResource;
        return true;
    }
}
