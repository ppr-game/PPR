using PER.Abstractions.Audio;
using PER.Abstractions.Renderer;
using PER.Abstractions.UI;
using PER.Util;

using PPR.Resources;

using PRR.UI;

namespace PPR.Screens;

public class MainMenuScreen : IScreen {
    private Dictionary<string, Color> _colors = new();
    private readonly List<Element> _ui = new();

    public void Enter() => Open();
    public void Quit() => Close();
    public bool QuitUpdate() => true;

    public void Open() {
        if(Core.engine.resources.TryGetResource("graphics/colors", out ColorsResource? colors))
            _colors = colors!.colors;

        IRenderer renderer = Core.engine.renderer;
        IAudio audio = Core.engine.audio;

        Button playButton = new(renderer) {
            audio = audio,
            position = new Vector2Int(40 - 2, 25),
            size = new Vector2Int(4, 1),
            text = "PLAY",
            idleColor = _colors["button_mainMenu.play_idle"],
            hoverColor = _colors["button_mainMenu.play_hover"],
            clickColor = _colors["button_mainMenu.play_click"]
        };
        playButton.onClick += (_, _) => {
        };
        _ui.Add(playButton);

        Button editButton = new(renderer) {
            audio = audio,
            position = new Vector2Int(40 - 2, 27),
            size = new Vector2Int(4, 1),
            text = "EDIT",
            idleColor = _colors["button_mainMenu.edit_idle"],
            hoverColor = _colors["button_mainMenu.edit_hover"],
            clickColor = _colors["button_mainMenu.edit_click"]
        };
        editButton.onClick += (_, _) => {
        };
        _ui.Add(editButton);

        Button settingsButton = new(renderer) {
            audio = audio,
            position = new Vector2Int(40 - 4, 29),
            size = new Vector2Int(8, 1),
            text = "SETTINGS",
            idleColor = _colors["button_mainMenu.settings_idle"],
            hoverColor = _colors["button_mainMenu.settings_hover"],
            clickColor = _colors["button_mainMenu.settings_click"]
        };
        settingsButton.onClick += (_, _) => {
        };
        _ui.Add(settingsButton);

        Button exitButton = new(renderer) {
            audio = audio,
            position = new Vector2Int(40 - 2, 31),
            size = new Vector2Int(4, 1),
            text = "EXIT",
            idleColor = _colors["button_mainMenu.exit_idle"],
            hoverColor = _colors["button_mainMenu.exit_hover"],
            clickColor = _colors["button_mainMenu.exit_click"]
        };
        exitButton.onClick += (_, _) => {
            Core.engine.renderer.Close();
        };
        _ui.Add(exitButton);
    }

    public void Close() {
        _ui.Clear();
    }

    public void Update() {
        foreach(Element element in _ui) element.Update(Core.engine.clock);
    }

    public void Tick() { }
}
