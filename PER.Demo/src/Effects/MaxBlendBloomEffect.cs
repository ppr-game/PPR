using System.Collections.Generic;
using System.IO;

using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;
using PER.Util;

namespace PER.Demo.Effects;

public class MaxBlendBloomEffect : IEffect, IResource {
    public IEnumerable<PipelineStep>? pipeline { get; private set; }

    public bool hasModifiers => false;
    public bool drawable => false;

    public bool Load(string id, IResources resources) {
        if(!Core.engine.resources.TryGetPath(Path.Join("graphics", "shaders", "default_vert.glsl"),
               out string? vertexPath) ||
           !Core.engine.resources.TryGetPath(Path.Join("graphics", "shaders", "bloom_frag.glsl"),
               out string? fragmentPath) ||
           !Core.engine.resources.TryGetPath(Path.Join("graphics", "shaders", "blend-max_frag.glsl"),
               out string? blendPath))
            return false;

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

        return true;
    }

    public bool Unload(string id, IResources resources) {
        pipeline = null;
        return true;
    }

    public (Vector2, RenderCharacter) ApplyModifiers(Vector2 position, RenderCharacter character) =>
        (position, character);

    public void Update(bool fullscreen) { }

    public void Draw(Vector2Int position) { }
}
