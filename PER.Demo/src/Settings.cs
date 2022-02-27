using System.IO;
using System.Text.Json;

using PER.Abstractions.Audio;

namespace PER.Demo;

public class Settings {
    public bool loadOnlyDefault { get; set; } = true;
    public float volume { get; set; } = 0.2f;

    public static Settings Load(string path) => !File.Exists(path) ? new Settings() :
        JsonSerializer.Deserialize<Settings>(File.ReadAllText(path)) ?? new Settings();

    public void Save(string path) => File.WriteAllText(path, JsonSerializer.Serialize(this));

    public void Apply() {
        if(!Core.engine.audio.TryGetMixer("master", out IAudioMixer? mixer)) return;
        mixer.volume = volume;
        Core.engine.audio.UpdateVolumes();
    }
}
