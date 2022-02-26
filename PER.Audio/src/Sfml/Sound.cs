using PER.Abstractions.Audio;

using SFML.Audio;

namespace PER.Audio.Sfml;

public class Sound : IPlayable, IDisposable {
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
        get => _sound.Volume / 100f;
        set => _sound.Volume = value * 100f;
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

    public Sound(string filename) {
        if(!cachedBuffers.TryGetValue(filename, out SoundBuffer? buffer)) {
            buffer = new SoundBuffer(filename);
            cachedBuffers.Add(filename, buffer);
        }
        _sound = new SFML.Audio.Sound(buffer);
    }

    internal static void Finish() {
        foreach((string? _, SoundBuffer? buffer) in cachedBuffers) buffer.Dispose();
    }

    public void Dispose() {
        _sound.Dispose();
        GC.SuppressFinalize(this);
    }
}
