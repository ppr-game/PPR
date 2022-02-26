using PER.Abstractions.Audio;

namespace PER.Audio.Sfml;

public class Music : IPlayable, IDisposable {
    public PlaybackStatus status {
        get => SfmlConverters.ToPerPlaybackStatus(_music.Status);
        set {
            switch(value) {
                case PlaybackStatus.Stopped:
                    _music.Stop();
                    break;
                case PlaybackStatus.Paused:
                    _music.Pause();
                    break;
                case PlaybackStatus.Playing:
                    _music.Play();
                    break;
            }
        }
    }

    public TimeSpan time {
        get => SfmlConverters.ToTimeSpan(_music.PlayingOffset);
        set => _music.PlayingOffset = SfmlConverters.ToSfmlTime(value);
    }

    public float volume {
        get => _music.Volume / 100f;
        set => _music.Volume = value * 100f;
    }

    public bool looped {
        get => _music.Loop;
        set => _music.Loop = value;
    }

    public float pitch {
        get => _music.Pitch;
        set => _music.Pitch = value;
    }

    public TimeSpan duration => SfmlConverters.ToTimeSpan(_music.Duration);

    private readonly SFML.Audio.Music _music;

    public Music(string filename) => _music = new SFML.Audio.Music(filename);
    public Music(byte[] bytes) => _music = new SFML.Audio.Music(bytes);
    public Music(Stream stream) => _music = new SFML.Audio.Music(stream);

    public void Dispose() {
        _music.Dispose();
        GC.SuppressFinalize(this);
    }
}
