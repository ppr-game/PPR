using PER.Abstractions.Audio;
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
        { "discord", typeof(LayoutResourceButton) },
        { "versions", typeof(LayoutResourceText) },
        { "player.playPause", typeof(LayoutResourceButton) },
        { "player.next", typeof(LayoutResourceButton) },
        { "player.name", typeof(LayoutResourceText) }
    };

    private string _playerPlayPauseTemplate = "  ";
    private string _playerNameTemplate = string.Empty;

    public override void Load(string id, IResources resources) {
        base.Load(id, resources);

        GetElement<Button>("play").onClick += (_, _) => {
            if(!Core.engine.resources.TryGetResource(LevelSelectScreen.GlobalId, out LevelSelectScreen? screen))
                return;
            screen.mode = PlayerMode.Play;
            Core.engine.game.SwitchScreen(screen);
        };

        GetElement<Button>("edit").onClick += (_, _) => {
            if(!Core.engine.resources.TryGetResource(LevelSelectScreen.GlobalId, out LevelSelectScreen? screen)) return;
            screen.mode = PlayerMode.Edit;
            Core.engine.game.SwitchScreen(screen);
        };

        GetElement<Button>("settings").onClick += (_, _) => {
            if(Core.engine.resources.TryGetResource(SettingsScreen.GlobalId, out SettingsScreen? screen))
                Core.engine.game.SwitchScreen(screen);
        };

        GetElement<Button>("exit").onClick += (_, _) => {
            Core.engine.game.SwitchScreen(null);
        };

        GetElement<Button>("sfml").onClick += (_, _) => {
            Helper.OpenUrl("https://sfml-dev.org");
        };

        GetElement<Button>("github").onClick += (_, _) => {
            Helper.OpenUrl("https://github.com/ppr-game/PPR");
        };

        GetElement<Button>("discord").onClick += (_, _) => {
            Helper.OpenUrl("https://discord.gg/AuYUVs5");
        };

        Text versions = GetElement<Text>("versions");
        versions.text =
            string.Format(versions.text ?? string.Empty, Core.version, Core.engineVersion, Core.abstractionsVersion,
                Core.utilVersion, Core.commonVersion, Core.audioVersion, Core.rendererVersion, Core.uiVersion);

        GetElement<Button>("player.playPause").onClick += (_, _) => {
            Conductor.status = Conductor.status == PlaybackStatus.Playing ? PlaybackStatus.Paused :
                PlaybackStatus.Playing;
        };

        GetElement<Button>("player.next").onClick += (_, _) => {
            Conductor.NextMusic();
        };

        _playerPlayPauseTemplate = GetElement<Button>("player.playPause").text ?? _playerPlayPauseTemplate;
        _playerNameTemplate = GetElement<Text>("player.name").text ?? _playerNameTemplate;
    }

    protected override void UpdateMusic(object? sender, EventArgs args) {
        base.UpdateMusic(sender, args);
        GetElement<Button>("player.playPause").text =
            _playerPlayPauseTemplate[(Conductor.status == PlaybackStatus.Playing).ToByte()].ToString();
        GetElement<Text>("player.name").text = string.Format(_playerNameTemplate, Conductor.author, Conductor.name);
    }

    public override void Update() {
        base.Update();
        foreach((string _, Element element) in elements)
            element.Update(Core.engine.clock);
    }

    public override void Tick() { }
}
