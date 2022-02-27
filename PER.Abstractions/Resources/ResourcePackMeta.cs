﻿namespace PER.Abstractions.Resources;

public readonly struct ResourcePackMeta {
    public string description { get; }
    public int version { get; }

    public ResourcePackMeta(string description, int version) {
        this.description = description;
        this.version = version;
    }
}
