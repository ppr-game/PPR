﻿using PER.Abstractions.Audio;
using PER.Abstractions.Resources;

using PRR.UI;

namespace PPR.Resources;

public class AudioResources : IResource {
    public bool Load(string id, IResources resources) {
        IAudio audio = Core.engine.audio;

        IAudioMixer master = audio.CreateMixer();
        IAudioMixer sfx = audio.CreateMixer(master);
        IAudioMixer music = audio.CreateMixer(master);

        if(resources.TryGetPath(Path.Combine("audio", "sfx", "buttonClick.wav"), out string? path)) {
            IPlayable buttonClick = audio.CreateSound(path, sfx);
            audio.TryStorePlayable(Button.ClickSoundId, buttonClick);
        }

        if(resources.TryGetPath(Path.Combine("audio", "sfx", "slider.wav"), out path)) {
            IPlayable sliderValueChanged = audio.CreateSound(path, sfx);
            audio.TryStorePlayable(Slider.ValueChangedSoundId, sliderValueChanged);
        }

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
