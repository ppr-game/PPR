using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Util;

using PRR.UI;

namespace PPR.Screens;

public class NewLevelDialogBoxScreen : DialogBoxScreenResourceBase {
    public const string GlobalId = "layouts/newLevelDialog";

    public Action? onCancel { get; set; }

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInput input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "newLevelDialog";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "title", typeof(LayoutResourceText) },
        { "labels", typeof(LayoutResourceText) },
        { "metadata.name", typeof(LayoutResourceInputField) },
        { "metadata.description", typeof(LayoutResourceInputField) },
        { "metadata.author", typeof(LayoutResourceInputField) },
        { "metadata.difficulty", typeof(LayoutResourceSlider) },
        { "metadata.difficulty.value", typeof(LayoutResourceText) },
        { "cancel", typeof(LayoutResourceButton) },
        { "create", typeof(LayoutResourceButton) }
    };

    public NewLevelDialogBoxScreen() : base(new Vector2Int(30, 32)) { }

    public override void Load(string id) {
        base.Load(id);

        InputField author = GetElement<InputField>("metadata.author");
        author.onTextChanged += (_, _) => {
            author.UpdateColors(colors.colors, layoutName, "metadata.author",
                LevelSelectScreen.authorToSpecial.TryGetValue(
                    author.value ?? string.Empty, out string? special) ? special : null);
        };

        Slider difficulty = GetElement<Slider>("metadata.difficulty");
        void DifficultyChanged() {
            Text text = GetElement<Text>("metadata.difficulty.value");
            int difficultyValue = (int)difficulty.value;
            // TODO: move this clamp to a separate class
            string special = Math.Clamp(difficultyValue, 1, 10).ToString();
            text.text = difficultyValue.ToString();
            difficulty.UpdateColors(colors.colors, layoutName, "metadata.difficulty", special);
            text.UpdateColors(colors.colors, layoutName, "metadata.difficulty", special);
        }
        DifficultyChanged();
        difficulty.onValueChanged += (_, _) => { DifficultyChanged(); };

        GetElement<Button>("cancel").onClick += (_, _) => { onCancel?.Invoke(); };

        GetElement<Button>("create").onClick += (_, _) => { };
    }

    public override void Close() {
        base.Close();
        onCancel = null;
    }

    public override void Tick(TimeSpan time) { }
}
