using JetBrains.Annotations;

using NLog;

using PER.Abstractions.Audio;
using PER.Abstractions.Resources;

namespace PER.Common.Resources;

[PublicAPI]
public abstract class AudioResourcesLoader : Resource {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    protected enum AudioType { Auto, Sfx, Music }
    protected record struct MixerDefinition(string id, AudioType defaultType, string defaultExtension);
    protected record struct AudioResource(string id, string? extension = null, string directory = "",
        AudioType type = AudioType.Auto);

    protected abstract IAudio audio { get; }
    protected abstract IReadOnlyDictionary<MixerDefinition, AudioResource[]> sounds { get; }

    protected override IEnumerable<KeyValuePair<string, string>> paths => sounds.SelectMany(pair =>
        pair.Value.Select(resource => new KeyValuePair<string, string>(resource.id,
            $"audio/{pair.Key.id}/{resource.directory}/{resource.id}.{resource.extension ?? pair.Key.defaultExtension}")));

    public override void Load(string id) {
        IAudioMixer master = audio.CreateMixer();

        foreach((MixerDefinition mixerDefinition, AudioResource[] audioResources) in sounds) {
            IAudioMixer mixer = audio.CreateMixer(master);

            foreach((string audioId, _, _, AudioType type) in audioResources)
                switch(type == AudioType.Auto ? mixerDefinition.defaultType : type) {
                    case AudioType.Sfx:
                        AddSound(audioId, mixer);
                        break;
                    case AudioType.Music:
                        AddMusic(audioId, mixer);
                        break;
                }

            audio.TryStoreMixer(mixerDefinition.id, mixer);
        }

        audio.TryStoreMixer(nameof(master), master);
    }

    public override void Unload(string id) => audio.Reset();

    protected void AddSound(string id, IAudioMixer mixer) {
        if(TryGetPath(id, out string? path)) {
            logger.Info("Loading sound {Id}", id);
            audio.TryStorePlayable(id, audio.CreateSound(path, mixer));
        }
        else
            logger.Info("Could not find sound {Id}", id);
    }

    protected void AddMusic(string id, IAudioMixer mixer) {
        if(TryGetPath(id, out string? path)) {
            logger.Info("Loading music {Id}", id);
            audio.TryStorePlayable(id, audio.CreateMusic(path, mixer));
        }
        else
            logger.Info("Could not find music {Id}", id);
    }
}
