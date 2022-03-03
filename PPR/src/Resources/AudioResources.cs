using PER.Abstractions.Audio;
using PER.Abstractions.Resources;

using PRR.UI;

namespace PPR.Resources;

public class AudioResources : IResource {
    public bool Load(string id, IResources resources) {
        IAudio audio = Core.engine.audio;

        IAudioMixer master = audio.CreateMixer();
        IAudioMixer sfx = audio.CreateMixer(master);
        IAudioMixer music = audio.CreateMixer(master);

        if(resources.TryGetPath(Path.Combine("audio", "music", "mainMenu.ogg"), out string? path))
            audio.TryStorePlayable("mainMenu", audio.CreateSound(path, music));

        if(resources.TryGetPath(Path.Combine("audio", "sfx", $"{Button.ClickSoundId}.wav"), out path))
            audio.TryStorePlayable(Button.ClickSoundId, audio.CreateSound(path, sfx));

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

        audio.TryStoreMixer(nameof(master), master);
        audio.TryStoreMixer(nameof(sfx), sfx);
        audio.TryStoreMixer(nameof(music), music);

        return true;
    }

    public bool Unload(string id, IResources resources) {
        Core.engine.audio.Reset();
        return true;
    }
}
