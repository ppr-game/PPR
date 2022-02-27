namespace PER.Abstractions.Audio;

public class AudioMixer : IAudioMixer {
    public IAudioMixer? parent { get; set; }

    public float volume {
        get => _volume * (parent?.volume ?? 1f);
        set => _volume = value;
    }

    private float _volume = 1f;

    public AudioMixer(IAudioMixer? parent = null) => this.parent = parent;
}
