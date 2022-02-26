using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace PER.Abstractions.Audio;

public interface IAudio {
    public IPlayable CreateSound(string filename);
    public IPlayable CreateMusic(string filename);
    public IPlayable CreateMusic(byte[] bytes);
    public IPlayable CreateMusic(Stream stream);

    public bool TryStorePlayable(string id, IPlayable playable);
    public bool TryGetPlayable(string id, [MaybeNullWhen(false)] out IPlayable playable);

    public void Finish();
}
