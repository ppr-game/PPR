namespace PER.Abstractions.Audio;

public interface IAudioMixer {
    public IAudioMixer? parent { get; set; }
    public float volume { get; set; }
}
