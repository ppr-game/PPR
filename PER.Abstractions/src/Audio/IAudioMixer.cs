using JetBrains.Annotations;

namespace PER.Abstractions.Audio;

[PublicAPI]
public interface IAudioMixer {
    public IAudioMixer? parent { get; set; }
    public float volume { get; set; }
}
