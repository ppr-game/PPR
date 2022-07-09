using PER.Abstractions.Audio;
using PER.Abstractions.Resources;

using PRR.UI;

namespace PPR.Resources;

public class AudioResources : IResource {
    public void Load(string id, IResources resources) {
        IAudio audio = Core.engine.audio;

        IAudioMixer master = audio.CreateMixer();
        IAudioMixer sfx = audio.CreateMixer(master);
        IAudioMixer music = audio.CreateMixer(master);

        if(resources.TryGetPath(Path.Combine("audio", "music", "mainMenu.ogg"), out string? path))
            audio.TryStorePlayable("mainMenu", audio.CreateSound(path, music));

        if(resources.TryGetPath(Path.Combine("audio", "sfx", $"{ClickableElementBase.ClickSoundId}.wav"), out path))
            audio.TryStorePlayable(ClickableElementBase.ClickSoundId, audio.CreateSound(path, sfx));

        if(resources.TryGetPath(Path.Combine("audio", "sfx", "fail.wav"), out path))
            audio.TryStorePlayable("fail", audio.CreateSound(path, sfx));

        if(resources.TryGetPath(Path.Combine("audio", "sfx", "hit.wav"), out path))
            audio.TryStorePlayable("hit", audio.CreateSound(path, sfx));

        if(resources.TryGetPath(Path.Combine("audio", "sfx", "pass.wav"), out path))
            audio.TryStorePlayable("pass", audio.CreateSound(path, sfx));

        if(resources.TryGetPath(Path.Combine("audio", "sfx", $"{Slider.ValueChangedSoundId}.wav"), out path))
            audio.TryStorePlayable(Slider.ValueChangedSoundId, audio.CreateSound(path, sfx));

        if(resources.TryGetPath(Path.Combine("audio", "sfx", "tick.wav"), out path))
            audio.TryStorePlayable("tick", audio.CreateSound(path, sfx));

        if(resources.TryGetPath(Path.Combine("audio", "sfx", $"{InputField.TypeSoundId}.wav"), out path))
            audio.TryStorePlayable(InputField.TypeSoundId, audio.CreateSound(path, sfx));

        if(resources.TryGetPath(Path.Combine("audio", "sfx", $"{InputField.EraseSoundId}.wav"), out path))
            audio.TryStorePlayable(InputField.EraseSoundId, audio.CreateSound(path, sfx));

        if(resources.TryGetPath(Path.Combine("audio", "sfx", $"{InputField.SubmitSoundId}.wav"), out path))
            audio.TryStorePlayable(InputField.SubmitSoundId, audio.CreateSound(path, sfx));

        audio.TryStoreMixer(nameof(master), master);
        audio.TryStoreMixer(nameof(sfx), sfx);
        audio.TryStoreMixer(nameof(music), music);
    }

    public void Unload(string id, IResources resources) => Core.engine.audio.Reset();
}
