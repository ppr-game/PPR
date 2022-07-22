using JetBrains.Annotations;

using PER.Abstractions.Audio;

namespace PER.Audio.Sfml;

[PublicAPI]
public class Music : IPlayable, IDisposable {
    public IAudioMixer mixer {
        get => _mixer;
        set {
            _mixer = value;
            UpdateVolume();
        }
    }

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
        get => _volume;
        set {
            _volume = value;
            UpdateVolume();
        }
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
    private IAudioMixer _mixer;
    private float _volume = 1f;

    private void UpdateVolume() => _music.Volume = volume * mixer.volume * 100f;

    private Music(SFML.Audio.Music music, IAudioMixer mixer) {
        _music = music;
        _mixer = mixer;
        UpdateVolume();
    }

    public Music(string filename, IAudioMixer mixer) : this(new SFML.Audio.Music(filename), mixer) { }
    public Music(byte[] bytes, IAudioMixer mixer) : this(new SFML.Audio.Music(bytes), mixer) { }
    public Music(Stream stream, IAudioMixer mixer) : this(new SFML.Audio.Music(stream), mixer) { }

    public void Dispose() {
        _music.Dispose();
        GC.SuppressFinalize(this);
    }
}
