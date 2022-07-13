using JetBrains.Annotations;

using PER.Abstractions.Audio;
using PER.Abstractions.Resources;

namespace PER.Common.Resources;

[PublicAPI]
public abstract class AudioResourcesBase : IResource {
    protected enum AudioType { Auto, Sfx, Music }
    protected record struct MixerDefinition(string id, AudioType defaultType, string defaultExtension);
    protected record struct AudioResource(string id, string? extension = null, string directory = "",
        AudioType type = AudioType.Auto);

    protected abstract IAudio audio { get; }
    protected abstract IReadOnlyDictionary<MixerDefinition, AudioResource[]> sounds { get; }

    public virtual void Load(string id, IResources resources) {
        IAudioMixer master = audio.CreateMixer();

        foreach((MixerDefinition mixerDefinition, AudioResource[] audioResources) in sounds) {
            IAudioMixer mixer = audio.CreateMixer(master);

            foreach((string audioId, string? extension, string directory, AudioType type) in audioResources)
                switch(type == AudioType.Auto ? mixerDefinition.defaultType : type) {
                    case AudioType.Sfx:
                        AddSound(resources, Path.Combine(mixerDefinition.id, directory), audioId,
                            extension ?? mixerDefinition.defaultExtension, mixer);
                        break;
                    case AudioType.Music:
                        AddMusic(resources, Path.Combine(mixerDefinition.id, directory), audioId,
                            extension ?? mixerDefinition.defaultExtension, mixer);
                        break;
                }

            audio.TryStoreMixer(mixerDefinition.id, mixer);
        }

        audio.TryStoreMixer(nameof(master), master);
    }

    public void Unload(string id, IResources resources) => audio.Reset();

    protected void AddSound(IResources resources, string directory, string id, string extension, IAudioMixer mixer) {
        if(resources.TryGetPath(Path.Combine("audio", directory, $"{id}.{extension}"), out string? path))
            audio.TryStorePlayable(id, audio.CreateSound(path, mixer));
    }

    protected void AddMusic(IResources resources, string directory, string id, string extension, IAudioMixer mixer) {
        if(resources.TryGetPath(Path.Combine("audio", directory, $"{id}.{extension}"), out string? path))
            audio.TryStorePlayable(id, audio.CreateMusic(path, mixer));
    }
}
