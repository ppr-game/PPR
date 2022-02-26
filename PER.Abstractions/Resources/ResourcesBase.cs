using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PER.Abstractions.Resources;

public abstract class ResourcesBase : IResources {
    public int currentVersion => 0;
    public IReadOnlyList<ResourcePackData> loadedPacks => mutableLoadedPacks;
    protected List<ResourcePackData> mutableLoadedPacks { get; } = new();
    protected virtual string resourcesRoot => "resources";
    protected virtual string resourcePackMeta => "metadata.json";
    protected virtual string resourcesInPack => "resources";

    protected bool loaded { get; set; }

    private readonly Dictionary<string, string> _resourcesCache = new();

    public abstract void Load();

    public void Unload() {
        loaded = false;
        mutableLoadedPacks.Clear();
        _resourcesCache.Clear();
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
        if(loaded) return false;
        mutableLoadedPacks.Add(data);
        return true;
    }

    public bool TryGetResource(string relativePath, [MaybeNullWhen(false)] out string fullPath) {
        fullPath = null;
        if(!loaded) return false;

        if(_resourcesCache.TryGetValue(relativePath, out fullPath))
            return true;

        for(int i = loadedPacks.Count - 1; i >= 0; i--) {
            ResourcePackData data = loadedPacks[i];
            string resourcePath = Path.Combine(data.fullPath, relativePath);
            if(!File.Exists(resourcePath) && !Directory.Exists(resourcePath)) continue;
            fullPath = resourcePath;
            _resourcesCache.Add(relativePath, fullPath);
            return true;
        }

        return false;
    }
}
