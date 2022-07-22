using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using PER.Abstractions.Audio;

namespace PPR;

[PublicAPI]
public static class Conductor {
    private static readonly Random random = new();
    private static IAudio audio => Core.engine.audio;

    public static event EventHandler? stateChanged;

    public static PlaybackStatus status {
        get => _currentMusic?.status ?? PlaybackStatus.Stopped;
        set {
            if(_currentMusic is not null)
                _currentMusic.status = value;
            stateChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static string author => _currentMusicMetadata.HasValue ? _currentMusicMetadata.Value.author : "<unknown>";
    public static string name => _currentMusicMetadata.HasValue ? _currentMusicMetadata.Value.name : "<unknown>";

    public static float bpm => _currentMusicMetadata.HasValue && _currentMusic is not null &&
        _currentMusic.status == PlaybackStatus.Playing ?
        _currentMusicMetadata.Value.GetBpmAt((float)_currentMusic.time.TotalSeconds) : 60f;

    public static TimeSpan time => _currentMusic is not null &&
        _currentMusic.status != PlaybackStatus.Stopped ? _currentMusic.time : TimeSpan.Zero;

    private static IPlayable? _currentMusic;
    private static LevelSerializer.MusicMetadata? _currentMusicMetadata;

    public static void Start() {
        _currentMusic = null;
        _currentMusicMetadata = null;
        if(!TryGetDefaultMusic(out IPlayable? playable, out LevelSerializer.MusicMetadata metadata) &&
            !TryGetRandomMusic(out playable, out metadata))
            return;
        _currentMusic = playable;
        _currentMusicMetadata = metadata;
        _currentMusic.Play();
    }

    public static void Update() {
        if(_currentMusic is null)
            return;
        if(_currentMusic.status == PlaybackStatus.Stopped)
            NextMusic();
    }

    public static void NextMusic() {
        if(!TryGetRandomMusic(out IPlayable? playable, out LevelSerializer.MusicMetadata metadata) &&
            !TryGetDefaultMusic(out playable, out metadata))
            return;
        SetMusic(playable, metadata);
    }

    public static void SetMusic(string levelDirectory, LevelSerializer.MusicMetadata metadata) {
        if(!TryGetPlayable(levelDirectory, metadata, out IPlayable? playable))
            return;
        SetMusic(playable, metadata);
    }

    private static bool TryGetDefaultMusic([NotNullWhen(true)] out IPlayable? playable,
        out LevelSerializer.MusicMetadata metadata) {
        metadata = new LevelSerializer.MusicMetadata(string.Empty, "Waterflame", "Cove",
            new[] { new LevelSerializer.MusicMetadata.Bpm(0f, 120f) });
        return audio.TryGetPlayable("mainMenu", out playable);
    }

    private static bool TryGetRandomMusic([NotNullWhen(true)] out IPlayable? playable,
        out LevelSerializer.MusicMetadata metadata) {
        List<(string, LevelSerializer.MusicMetadata)> music = new();
        foreach(string levelDirectory in LevelSerializer.EnumerateLevelDirectories()) {
            if(!LevelSerializer.TryReadMusicMetadata(levelDirectory, out LevelSerializer.MusicMetadata musicMetadata))
                continue;
            music.Add((levelDirectory, musicMetadata));
        }

        int index = random.Next(music.Count);
        metadata = music[index].Item2;
        return TryGetPlayable(music[index].Item1, metadata, out playable);
    }

    private static bool TryGetPlayable(string levelDirectory, LevelSerializer.MusicMetadata metadata,
        [NotNullWhen(true)] out IPlayable? playable) {
        string musicPath = Path.Combine(levelDirectory, metadata.fileName);
        if(audio.TryGetPlayable(musicPath, out playable))
            return true;
        if(!audio.TryGetMixer("music", out IAudioMixer? mixer))
            return false;
        playable = audio.CreateMusic(musicPath, mixer);
        audio.TryStorePlayable(musicPath, playable);
        return true;
    }

    private static void SetMusic(IPlayable playable, LevelSerializer.MusicMetadata metadata) {
        _currentMusic?.StopPlayback();
        _currentMusic = playable;
        _currentMusicMetadata = metadata;
        _currentMusic.Play();
        stateChanged?.Invoke(null, EventArgs.Empty);
    }
}
