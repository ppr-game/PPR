﻿using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Util;

using PRR.UI;

namespace PPR.Screens;

public class MainMenuScreen : MenuWithCoolBackgroundAnimationScreenResourceBase {
    public const string GlobalId = "layouts/mainMenu";

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInput input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "mainMenu";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "frame", typeof(LayoutResourceText) },
        { "title", typeof(LayoutResourceText) },
        { "play", typeof(LayoutResourceButton) },
        { "edit", typeof(LayoutResourceButton) },
        { "settings", typeof(LayoutResourceButton) },
        { "exit", typeof(LayoutResourceButton) },
        { "sfml", typeof(LayoutResourceButton) },
        { "github", typeof(LayoutResourceButton) },
        { "discord", typeof(LayoutResourceButton) }
    };

    // shut up
    // ReSharper disable once CognitiveComplexity
    public override void Load(string id, IResources resources) {
        base.Load(id, resources);

        if(elements["play"] is Button play) play.onClick += (_, _) => {
            if(!Core.engine.resources.TryGetResource(LevelSelectScreen.GlobalId, out LevelSelectScreen? screen)) return;
            screen.mode = PlayerMode.Play;
            Core.engine.game.SwitchScreen(screen);
        };

        if(elements["edit"] is Button edit) edit.onClick += (_, _) => {
            if(!Core.engine.resources.TryGetResource(LevelSelectScreen.GlobalId, out LevelSelectScreen? screen)) return;
            screen.mode = PlayerMode.Edit;
            Core.engine.game.SwitchScreen(screen);
        };

        if(elements["settings"] is Button settings) settings.onClick += (_, _) => {
            if(Core.engine.resources.TryGetResource(SettingsScreen.GlobalId, out SettingsScreen? screen))
                Core.engine.game.SwitchScreen(screen);
        };

        if(elements["exit"] is Button exit) exit.onClick += (_, _) => {
            Core.engine.game.SwitchScreen(null);
        };

        if(elements["sfml"] is Button sfml) sfml.onClick += (_, _) => {
            Helper.OpenUrl("https://sfml-dev.org");
        };

        if(elements["github"] is Button github) github.onClick += (_, _) => {
            Helper.OpenUrl("https://github.com/ppr-game/PPR");
        };

        if(elements["discord"] is Button discord) discord.onClick += (_, _) => {
            Helper.OpenUrl("https://discord.gg/AuYUVs5");
        };
    }

    public override void Open() { }

    public override void Close() { }

    public override void Update() {
        base.Update();
        foreach((string _, Element element) in elements)
            element.Update(Core.engine.clock);
    }

    public override void Tick() { }
}
