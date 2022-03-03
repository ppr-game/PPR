﻿using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.UI;

using PRR.UI;
using PRR.UI.Resources;

namespace PPR.Screens;

public class MainMenuScreen : ScreenResourceBase {
    public const string GlobalId = "layouts/mainMenu";

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInputManager input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "mainMenu";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "frame", typeof(LayoutResourceText) },
        { "title", typeof(LayoutResourceText) },
        { "play", typeof(LayoutResourceButton) },
        { "edit", typeof(LayoutResourceButton) },
        { "settings", typeof(LayoutResourceButton) },
        { "exit", typeof(LayoutResourceButton) }
    };

    public override void Open() {
        if(elements["settings"] is Button settings) settings.onClick += (_, _) => {
            if(Core.engine.resources.TryGetResource(SettingsScreen.GlobalId, out SettingsScreen? screen))
                Core.engine.game.SwitchScreen(screen, 0.2f, 0.2f);
        };

        if(elements["exit"] is Button exit) exit.onClick += (_, _) => {
            Core.engine.game.SwitchScreen(null, 2f, 0f);
        };
    }

    public override void Close() { }

    public override void Update() {
        foreach((string _, Element element) in elements) element.Update(Core.engine.clock);
    }

    public override void Tick() { }
}
