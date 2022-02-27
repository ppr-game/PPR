using System.IO;

using PER.Abstractions.Audio;
using PER.Abstractions.Resources;

using PRR.UI;

namespace PER.Demo.Resources;

public class AudioResources : IResource {
    public bool Load(string id, IResources resources) {
        IAudio audio = Core.engine.audio;

        if(resources.TryGetPath(Path.Combine("audio", "buttonClick.wav"), out string? path)) {
            IPlayable buttonClick = audio.CreateSound(path);
            audio.TryStorePlayable(Button.ClickSoundId, buttonClick);
        }

        // ReSharper disable once InvertIf
        if(resources.TryGetPath(Path.Combine("audio", "slider.wav"), out path)) {
            IPlayable sliderValueChanged = audio.CreateSound(path);
            audio.TryStorePlayable(Slider.ValueChangedSoundId, sliderValueChanged);
        }

        return true;
    }

    public bool Unload(string id, IResources resources) {
        Core.engine.audio.Reset();
        return true;
    }
}
