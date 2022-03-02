using System.Text.Json;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Util;

using PPR.Resources;

using PRR.UI;

namespace PPR.Screens;

public class MainMenuScreen : ScreenResourceBase {
    //private Dictionary<string, Color> _colors = new();

    protected override string layoutName => "mainMenu";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "frame", typeof(LayoutText) },
        { "title", typeof(LayoutText) },
        { "play", typeof(LayoutButton) },
        { "edit", typeof(LayoutButton) },
        { "settings", typeof(LayoutButton) },
        { "exit", typeof(LayoutButton) }
    };

    public override void Enter() => Open();
    public override void Quit() => Close();
    public override bool QuitUpdate() => true;

    public override void Open() {
        //if(Core.engine.resources.TryGetResource("graphics/colors", out ColorsResource? colors))
        //    _colors = colors!.colors;

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
