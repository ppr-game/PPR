using System.Collections.Immutable;

using JetBrains.Annotations;

using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Util;

namespace PER.Common.Effects;

[PublicAPI]
public class BloomEffect : Resource, IEffect {
    public const string GlobalId = "graphics/effects/bloom";

    protected override IEnumerable<KeyValuePair<string, string>> paths { get; } = new Dictionary<string, string> {
        { "vertex", "graphics/shaders/default_vert.glsl" },
        { "fragment", "graphics/shaders/bloom_frag.glsl" },
        { "blend", "graphics/shaders/bloom-blend_frag.glsl" }
    };

    public IEnumerable<PipelineStep>? pipeline { get; private set; }
    public bool hasModifiers => false;
    public bool drawable => false;

    public override void Load(string id) {
        string vertexPath = GetPath("vertex");
        string fragmentPath = GetPath("fragment");
        string blendPath = GetPath("blend");

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

    public override void Unload(string id) => pipeline = null;

    public void ApplyModifiers(Vector2Int at, ref Vector2 position, ref RenderCharacter character) { }

    public void Update(bool fullscreen) { }

    public void Draw(Vector2Int position) { }
}
