using JetBrains.Annotations;

namespace PER.Abstractions.Audio;

[PublicAPI]
public class AudioMixer : IAudioMixer {
    public IAudioMixer? parent { get; set; }

    public float volume {
        get => _volume * (parent?.volume ?? 1f);
        set => _volume = value;
    }

    private float _volume = 1f;

    public AudioMixer(IAudioMixer? parent = null) => this.parent = parent;
}
