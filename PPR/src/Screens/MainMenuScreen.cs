using PER.Abstractions.Renderer;
using PER.Abstractions.UI;

using PPR.Resources;

using PRR.UI;

namespace PPR.Screens;

public class MainMenuScreen : ScreenResourceBase {
    protected override string layoutName => "mainMenu";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "frame", typeof(LayoutResourceText) },
        { "title", typeof(LayoutResourceText) },
        { "play", typeof(LayoutResourceButton) },
        { "edit", typeof(LayoutResourceButton) },
        { "settings", typeof(LayoutResourceButton) },
        { "exit", typeof(LayoutResourceButton) }
    };

    public override void Enter() => Open();
    public override void Quit() => Close();
    public override bool QuitUpdate() => true;

    public override void Open() {
        IRenderer renderer = Core.engine.renderer;

        if(elements["exit"] is Button button) button.onClick += (_, _) => {
            renderer.Close();
        };
    }

    public override void Close() { }

    public override void Update() {
        foreach((string _, Element element) in elements) element.Update(Core.engine.clock);
    }

    public override void Tick() { }
}
