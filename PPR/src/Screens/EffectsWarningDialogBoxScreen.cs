using JetBrains.Annotations;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Util;

using PRR.UI;

namespace PPR.Screens;

[PublicAPI]
public class EffectsWarningDialogBoxScreen : DialogBoxScreenResource {
    public const string GlobalId = "layouts/effectsWarningDialog";

    public Action? onCancel { get; set; }
    public Action? onPlay { get; set; }
    public Action? onDisableAndPlay { get; set; }
    public Action? onSaveChoice { get; set; }

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInput input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "effectsWarningDialog";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "title", typeof(LayoutResourceText) },
        { "text", typeof(LayoutResourceText) },
        { "cancel", typeof(LayoutResourceButton) },
        { "play", typeof(LayoutResourceButton) },
        { "disableAndPlay", typeof(LayoutResourceButton) },
        { "saveChoice", typeof(LayoutResourceButton) }
    };

    public EffectsWarningDialogBoxScreen() : base(new Vector2Int(46, 24)) { }

    public override void Load(string id) {
        base.Load(id);

        GetElement<Button>("cancel").onClick += (_, _) => { onCancel?.Invoke(); };
        GetElement<Button>("play").onClick += (_, _) => { onPlay?.Invoke(); };
        GetElement<Button>("disableAndPlay").onClick += (_, _) => { onDisableAndPlay?.Invoke(); };
        GetElement<Button>("saveChoice").onClick += (_, _) => { onSaveChoice?.Invoke(); };
    }

    public override void Close() {
        base.Close();
        onCancel = null;
        onPlay = null;
        onDisableAndPlay = null;
        onSaveChoice = null;
    }

    public override void Tick(TimeSpan time) { }
}
