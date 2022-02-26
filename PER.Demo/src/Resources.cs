using System.IO;

using PER.Abstractions;
using PER.Abstractions.Audio;
using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;

using PRR.UI;

namespace PER.Demo;

public class Resources : ResourcesBase {
    public override void Load() {
        loaded = true;

        IAudio audio = Core.engine.audio;

        audio.Reset();

        if(TryGetResource(Path.Combine("audio", "buttonClick.wav"), out string? path)) {
            IPlayable buttonClick = audio.CreateSound(path);
            buttonClick.volume = 0f;
            audio.TryStorePlayable(Button.ClickSoundId, buttonClick);
        }

        // ReSharper disable once InvertIf
        if(TryGetResource(Path.Combine("audio", "slider.wav"), out path)) {
            IPlayable sliderValueChanged = audio.CreateSound(path);
            sliderValueChanged.volume = 0f;
            audio.TryStorePlayable(Slider.ValueChangedSoundId, sliderValueChanged);
        }
    }
}
