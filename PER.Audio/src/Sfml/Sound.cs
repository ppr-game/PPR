using PER.Abstractions.Audio;

using SFML.Audio;

namespace PER.Audio.Sfml;

public class Sound : IPlayable, IDisposable {
    public IAudioMixer mixer { get; set; }

    public PlaybackStatus status {
        get => SfmlConverters.ToPerPlaybackStatus(_sound.Status);
        set {
            switch(value) {
                case PlaybackStatus.Stopped:
                    _sound.Stop();
                    break;
                case PlaybackStatus.Paused:
                    _sound.Pause();
                    break;
                case PlaybackStatus.Playing:
                    _sound.Play();
                    break;
            }
        }
    }

    public TimeSpan time {
        get => SfmlConverters.ToTimeSpan(_sound.PlayingOffset);
        set => _sound.PlayingOffset = SfmlConverters.ToSfmlTime(value);
    }

    public float volume {
        get => _volume;
        set {
            _volume = value;
            _sound.Volume = value * mixer.volume * 100f;
        }
    }

    public bool looped {
        get => _sound.Loop;
        set => _sound.Loop = value;
    }

    public float pitch {
        get => _sound.Pitch;
        set => _sound.Pitch = value;
    }

    public TimeSpan duration => SfmlConverters.ToTimeSpan(_sound.SoundBuffer.Duration);

    private static readonly Dictionary<string, SoundBuffer> cachedBuffers = new();

    private readonly SFML.Audio.Sound _sound;
    private float _volume = 1f;

    public Sound(string filename, IAudioMixer mixer) {
        if(!cachedBuffers.TryGetValue(filename, out SoundBuffer? buffer)) {
            buffer = new SoundBuffer(filename);
            cachedBuffers.Add(filename, buffer);
        }
        _sound = new SFML.Audio.Sound(buffer);

        this.mixer = mixer;
    }

    internal static void Reset() {
        foreach((string? _, SoundBuffer? buffer) in cachedBuffers) buffer.Dispose();
        cachedBuffers.Clear();
    }

    internal static void Finish() => Reset();

    public void Dispose() {
        _sound.Dispose();
        GC.SuppressFinalize(this);
    }
}
