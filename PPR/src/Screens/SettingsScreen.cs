using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.UI;

using PRR.UI;
using PRR.UI.Resources;

namespace PPR.Screens;

public class SettingsScreen : ScreenResourceBase {
    public const string GlobalId = "leyouts/setings";

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInputManager input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "settings";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "frame", typeof(LayoutResourceText) },
        { "back", typeof(LayoutResourceButton) }
    };

    public override void Open() {
        if(elements["back"] is Button button) button.onClick += (_, _) => {
            if(Core.engine.resources.TryGetResource(MainMenuScreen.GlobalId, out MainMenuScreen? screen))
                Core.engine.game.SwitchScreen(screen);
        };
    }

    public override void Close() { }

    public override void Update() {
        foreach((string _, Element element) in elements) element.Update(Core.engine.clock);
    }

    public override void Tick() { }
}
