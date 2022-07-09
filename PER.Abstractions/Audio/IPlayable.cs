using System;

using JetBrains.Annotations;

namespace PER.Abstractions.Audio;

[PublicAPI]
public interface IPlayable {
    public IAudioMixer mixer { get; set; }
    public PlaybackStatus status { get; set; }
    public TimeSpan time { get; set; }
    public float volume { get; set; }
    public bool looped { get; set; }
    public float pitch { get; set; }
    public TimeSpan duration { get; }

    public void Play() => status = PlaybackStatus.Playing;
    public void Pause() => status = PlaybackStatus.Paused;
    // hhh shut up compiler now the method name is ugly
    public void StopPlayback() => status = PlaybackStatus.Stopped;
}
