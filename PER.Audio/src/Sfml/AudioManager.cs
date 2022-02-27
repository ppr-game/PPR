using System.Diagnostics.CodeAnalysis;

using PER.Abstractions.Audio;

namespace PER.Audio.Sfml;

public class AudioManager : IAudio {
    private readonly Dictionary<string, IAudioMixer> _storedMixers = new();

    private readonly List<IPlayable> _allPlayables = new();
    private readonly Dictionary<string, IPlayable> _storedPlayables = new();

    public IAudioMixer CreateMixer(IAudioMixer? parent = null) => new AudioMixer(parent);
    public bool TryStoreMixer(string id, IAudioMixer mixer) => _storedMixers.TryAdd(id, mixer);
    public bool TryGetMixer(string id, [MaybeNullWhen(false)] out IAudioMixer mixer) =>
        _storedMixers.TryGetValue(id, out mixer);

    public IPlayable CreateSound(string filename, IAudioMixer mixer) => AddPlayable(new Sound(filename, mixer));
    public IPlayable CreateMusic(string filename, IAudioMixer mixer) => AddPlayable(new Music(filename, mixer));
    public IPlayable CreateMusic(byte[] bytes, IAudioMixer mixer) => AddPlayable(new Music(bytes, mixer));
    public IPlayable CreateMusic(Stream stream, IAudioMixer mixer) => AddPlayable(new Music(stream, mixer));

    private IPlayable AddPlayable(IPlayable playable) {
        _allPlayables.Add(playable);
        return playable;
    }

    public bool TryStorePlayable(string id, IPlayable playable) => _storedPlayables.TryAdd(id, playable);
    public bool TryGetPlayable(string id, [MaybeNullWhen(false)] out IPlayable playable) =>
        _storedPlayables.TryGetValue(id, out playable);

    public void UpdateVolumes() {
        foreach(IPlayable playable in _allPlayables) playable.volume = playable.volume;
    }

    public void Reset() {
        Sound.Reset();
        foreach(IPlayable? playable in _allPlayables)
            if(playable is IDisposable disposable)
                disposable.Dispose();
        _allPlayables.Clear();
        _storedPlayables.Clear();
        _storedMixers.Clear();
    }

    public void Finish() {
        Sound.Finish();
        Reset();
    }
}
