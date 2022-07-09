using System.IO;

using PER.Abstractions.Audio;
using PER.Abstractions.Resources;

using PRR.UI;

namespace PER.Demo.Resources;

public class AudioResources : IResource {
    public void Load(string id, IResources resources) {
        IAudio audio = Core.engine.audio;

        IAudioMixer master = audio.CreateMixer();
        IAudioMixer sfx = audio.CreateMixer(master);
        IAudioMixer music = audio.CreateMixer(master);

        if(resources.TryGetPath(Path.Combine("audio", $"{ClickableElementBase.ClickSoundId}.wav"), out string? path))
            audio.TryStorePlayable(ClickableElementBase.ClickSoundId, audio.CreateSound(path, sfx));

        if(resources.TryGetPath(Path.Combine("audio", $"{Slider.ValueChangedSoundId}.wav"), out path))
            audio.TryStorePlayable(Slider.ValueChangedSoundId, audio.CreateSound(path, sfx));

        if(resources.TryGetPath(Path.Combine("audio", $"{InputField.TypeSoundId}.wav"), out path))
            audio.TryStorePlayable(InputField.TypeSoundId, audio.CreateSound(path, sfx));

        if(resources.TryGetPath(Path.Combine("audio", $"{InputField.EraseSoundId}.wav"), out path))
            audio.TryStorePlayable(InputField.EraseSoundId, audio.CreateSound(path, sfx));

        if(resources.TryGetPath(Path.Combine("audio", $"{InputField.SubmitSoundId}.wav"), out path))
            audio.TryStorePlayable(InputField.SubmitSoundId, audio.CreateSound(path, sfx));

        audio.TryStoreMixer(nameof(master), master);
        audio.TryStoreMixer(nameof(sfx), sfx);
        audio.TryStoreMixer(nameof(music), music);
    }

    public void Unload(string id, IResources resources) => Core.engine.audio.Reset();
}
