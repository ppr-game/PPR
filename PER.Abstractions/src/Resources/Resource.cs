using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

namespace PER.Abstractions.Resources;

[PublicAPI]
public abstract class Resource {
    protected virtual IEnumerable<KeyValuePair<string, Type>> dependencyTypes =>
        ImmutableDictionary<string, Type>.Empty;

    protected virtual IEnumerable<KeyValuePair<string, string>> paths => ImmutableDictionary<string, string>.Empty;

    private Dictionary<string, Resource> _dependencies = new();
    private Dictionary<string, IEnumerable<string>> _fullPaths = new();

    public void ResolveDependencies(IResources resources) {
        _dependencies.Clear();
        foreach((string id, Type type) in dependencyTypes) {
            if(_dependencies.ContainsKey(id))
                throw new InvalidOperationException($"Dependency {id} already registered.");
            if(!resources.TryGetResource(id, out Resource? dependency))
                throw new InvalidOperationException($"Resource {id} does not exist.");
            if(dependency.GetType() != type)
                throw new InvalidOperationException($"Resource {id} is not {type}.");
            _dependencies.Add(id, dependency);
        }
    }

    public void ResolvePaths(IResources resources) {
        _fullPaths.Clear();
        foreach((string id, string path) in paths) {
            if(_fullPaths.ContainsKey(id))
                throw new InvalidOperationException($"File with ID {id} already registered.");
            _fullPaths.Add(id, resources.GetAllPaths(Path.Combine(path.Split('/'))));
        }
    }

    public int GetPathsHash() {
        StringBuilder builder = new();
        foreach((_, IEnumerable<string> paths) in _fullPaths)
            foreach(string path in paths)
                builder.Append(path);
        return builder.ToString().GetHashCode();
    }

    public abstract void Load(string id);
    public abstract void Unload(string id);

    public bool HasDependency(string id) => _dependencies.ContainsKey(id);

    protected Resource GetDependency(string id) {
        if(!_dependencies.TryGetValue(id, out Resource? dependency))
            throw new InvalidOperationException($"Resource {id} is not registered as a dependency.");
        return dependency;
    }

    protected T GetDependency<T>(string id) where T : Resource {
        Resource dependency = GetDependency(id);
        if(dependency is not T typedDependency)
            throw new InvalidOperationException($"Resource {id} is not {nameof(T)}.");

        return typedDependency;
    }

    protected bool TryGetPaths(string id, [NotNullWhen(true)] out IEnumerable<string>? fullPaths) =>
        _fullPaths.TryGetValue(id, out fullPaths);

    protected IEnumerable<string> GetPaths(string id) {
        if(!TryGetPaths(id, out IEnumerable<string>? fullPaths))
            throw new InvalidOperationException($"File with ID {id} is not registered.");
        return fullPaths;
    }

    protected bool TryGetPath(string id, [NotNullWhen(true)] out string? fullPath) {
        if(!TryGetPaths(id, out IEnumerable<string>? fullPaths)) {
            fullPath = null;
            return false;
        }
        fullPath = fullPaths.FirstOrDefault((string?)null);
        return fullPath is not null;
    }

    protected string GetPath(string id) {
        if(!TryGetPath(id, out string? fullPath))
            throw new FileNotFoundException(null, id);
        return fullPath;
    }
}
