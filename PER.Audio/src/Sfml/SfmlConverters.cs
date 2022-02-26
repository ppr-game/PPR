using PER.Abstractions.Audio;

using SFML.Audio;
using SFML.System;

namespace PER.Audio.Sfml;

public static class SfmlConverters {
    private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

    public static PlaybackStatus ToPerPlaybackStatus(SoundStatus status) => (PlaybackStatus)status;
    public static SoundStatus ToSfmlPlaybackStatus(PlaybackStatus status) => (SoundStatus)status;

    public static TimeSpan ToTimeSpan(Time time) => TimeSpan.FromTicks(time.AsMicroseconds() * TicksPerMicrosecond);
    public static Time ToSfmlTime(TimeSpan timeSpan) => Time.FromMicroseconds(timeSpan.Ticks / TicksPerMicrosecond);
}
