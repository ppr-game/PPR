namespace PER.Abstractions.Resources;

public struct ResourcePackData {
    public string name { get; }
    public string fullPath { get; }
    public ResourcePackMeta meta { get; }

    public ResourcePackData(string name, string fullPath, ResourcePackMeta meta) {
        this.name = name;
        this.fullPath = fullPath;
        this.meta = meta;
    }
}
