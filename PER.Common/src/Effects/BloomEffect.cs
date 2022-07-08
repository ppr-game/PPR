using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Util;

namespace PER.Common.Effects;

public class BloomEffect : IEffect, IResource {
    public const string GlobalId = "graphics/effects/bloom";

    public IEnumerable<PipelineStep>? pipeline { get; private set; }
    public bool hasModifiers => false;
    public bool drawable => false;

    public void Load(string id, IResources resources) {
        if(!resources.TryGetPath(Path.Join("graphics", "shaders", "default_vert.glsl"),
                out string? vertexPath) ||
            !resources.TryGetPath(Path.Join("graphics", "shaders", "bloom_frag.glsl"),
                out string? fragmentPath) ||
            !resources.TryGetPath(Path.Join("graphics", "shaders", "bloom-blend_frag.glsl"),
                out string? blendPath))
            throw new InvalidOperationException("Missing dependencies.");

        pipeline = new[] {
            new PipelineStep {
                stepType = PipelineStep.Type.TemporaryText,
                blendMode = BlendMode.alpha
            },
            new PipelineStep {
                stepType = PipelineStep.Type.SwapBuffer
            },
            new PipelineStep {
                stepType = PipelineStep.Type.TemporaryScreen,
                vertexShader = File.ReadAllText(vertexPath),
                fragmentShader = File.ReadAllText(fragmentPath),
                blendMode = BlendMode.alpha
            },
            new PipelineStep {
                stepType = PipelineStep.Type.SwapBuffer
            },
            new PipelineStep {
                stepType = PipelineStep.Type.TemporaryScreen,
                vertexShader = File.ReadAllText(vertexPath),
                fragmentShader = File.ReadAllText(fragmentPath),
                blendMode = BlendMode.alpha
            },
            new PipelineStep {
                stepType = PipelineStep.Type.SwapBuffer
            },
            new PipelineStep {
                stepType = PipelineStep.Type.ClearBuffer
            },
            new PipelineStep {
                stepType = PipelineStep.Type.TemporaryText,
                blendMode = BlendMode.alpha
            },
            new PipelineStep {
                stepType = PipelineStep.Type.Screen,
                vertexShader = File.ReadAllText(vertexPath),
                fragmentShader = File.ReadAllText(blendPath),
                blendMode = BlendMode.alpha
            }
        };
    }

    public void Unload(string id, IResources resources) => pipeline = null;

    public (Vector2, RenderCharacter) ApplyModifiers(Vector2Int at, Vector2 position, RenderCharacter character) =>
        (position, character);

    public void Update(bool fullscreen) { }

    public void Draw(Vector2Int position) { }
}
