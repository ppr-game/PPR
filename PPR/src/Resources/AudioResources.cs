﻿using PER.Abstractions.Audio;
using PER.Common.Resources;

using PRR.UI;

namespace PPR.Resources;

public class AudioResources : AudioResourcesBase {
    protected override IAudio audio => Core.engine.audio;
    protected override IReadOnlyDictionary<MixerDefinition, AudioResource[]> sounds { get; } =
        new Dictionary<MixerDefinition, AudioResource[]> {
            { new MixerDefinition("music", AudioType.Music, "ogg"), new [] {
                new AudioResource("mainMenu")
            } },
            { new MixerDefinition("sfx", AudioType.Sfx, "wav"), new [] {
                new AudioResource(ClickableElementBase.ClickSoundId),
                new AudioResource("fail"),
                new AudioResource("hit"),
                new AudioResource("pass"),
                new AudioResource(Slider.ValueChangedSoundId),
                new AudioResource("tick"),
                new AudioResource(InputField.TypeSoundId),
                new AudioResource(InputField.EraseSoundId),
                new AudioResource(InputField.SubmitSoundId)
            } }
        };
}
