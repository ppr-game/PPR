using JetBrains.Annotations;

namespace PER.Abstractions.Rendering;

[PublicAPI]
public struct PipelineStep {
    public enum Type { Text, Screen, TemporaryText, TemporaryScreen, SwapBuffer, ClearBuffer }

    public Type stepType { get; init; }
    public string? vertexShader { get; init; }
    public string? fragmentShader { get; init; }
    public BlendMode blendMode { get; init; }
}
