using System.Diagnostics.CodeAnalysis;

using PER.Abstractions.Audio;

namespace PER.Audio.Sfml;

public class AudioManager : IAudio {
    private readonly List<IPlayable> _allPlayables = new();
    private readonly Dictionary<string, IPlayable> _storedPlayables = new();

    public IPlayable CreateSound(string filename) => AddPlayable(new Sound(filename));
    public IPlayable CreateMusic(string filename) => AddPlayable(new Music(filename));
    public IPlayable CreateMusic(byte[] bytes) => AddPlayable(new Music(bytes));
    public IPlayable CreateMusic(Stream stream) => AddPlayable(new Music(stream));

    private IPlayable AddPlayable(IPlayable playable) {
        _allPlayables.Add(playable);
        return playable;
    }

    public bool TryStorePlayable(string id, IPlayable playable) => _storedPlayables.TryAdd(id, playable);
    public bool TryGetPlayable(string id, [MaybeNullWhen(false)] out IPlayable playable) =>
        _storedPlayables.TryGetValue(id, out playable);

    public void Reset() {
        Sound.Reset();
        foreach(IPlayable? playable in _allPlayables)
            if(playable is IDisposable disposable)
                disposable.Dispose();
        _allPlayables.Clear();
        _storedPlayables.Clear();
    }

    public void Finish() {
        Sound.Finish();
        Reset();
    }
}
