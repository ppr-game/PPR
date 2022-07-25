using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;

using PPR.Screens.Templates;

using PRR.UI;

namespace PPR.Screens;

public class LevelSelectScreen : MenuWithCoolBackgroundAnimationScreenResource {
    public const string GlobalId = "layouts/levelSelect";

    public PlayerMode mode { get; set; }

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInput input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "levelSelect";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "frameLeft", typeof(LayoutResourceText) },
        { "frameRight", typeof(LayoutResourceText) },
        { "scores", typeof(LayoutResourceListBox<LevelSerializer.LevelScore>) },
        { "levels", typeof(LayoutResourceListBox<LevelSerializer.LevelItem>) },
        { "metadata.labels", typeof(LayoutResourceText) },
        { "metadata.difficulty", typeof(LayoutResourceText) },
        { "metadata.author", typeof(LayoutResourceText) },
        { "metadata.description", typeof(LayoutResourceText) },
        { "play_auto", typeof(LayoutResourceButton) },
        { "edit_new", typeof(LayoutResourceButton) },
        { "back", typeof(LayoutResourceButton) }
    };

    protected override IEnumerable<KeyValuePair<string, Type>> dependencyTypes {
        get {
            foreach(KeyValuePair<string, Type> pair in base.dependencyTypes)
                yield return pair;
            yield return new KeyValuePair<string, Type>(LevelSelectorTemplate.GlobalId, typeof(LevelSelectorTemplate));
            yield return new KeyValuePair<string, Type>(ScoreListTemplate.GlobalId, typeof(ScoreListTemplate));
        }
    }

    protected override IEnumerable<KeyValuePair<string, string>> paths {
        get {
            foreach(KeyValuePair<string, string> pair in base.paths)
                yield return pair;
            yield return new KeyValuePair<string, string>("frameLeft.text", $"{layoutsPath}/{layoutName}Left.txt");
            yield return new KeyValuePair<string, string>("frameRight.text", $"{layoutsPath}/{layoutName}Right.txt");
        }
    }

    public static readonly IReadOnlyDictionary<string, string> authorToSpecial = new Dictionary<string, string> {
        { "ConfiG", "ConfiG" },
        { "sbeve", "contributor" }
    };

    private Dictionary<Guid, LevelSerializer.LevelScore[]> _scores = new();

    private NewLevelDialogBoxScreen? _newLevelDialogBox;

    public LevelSelectScreen(IResources resources) {
        resources.TryAddResource(LevelSelectorTemplate.GlobalId, new LevelSelectorTemplate(this));
        resources.TryAddResource(ScoreListTemplate.GlobalId, new ScoreListTemplate());
    }

    public override void Load(string id) {
        base.Load(id);

        GetElement<Button>("edit_new").onClick += (_, _) => {
            if(!Core.engine.resources.TryGetResource(NewLevelDialogBoxScreen.GlobalId, out _newLevelDialogBox))
                return;
            _newLevelDialogBox.onCancel += () => {
                Core.engine.game.FadeScreen(() => {
                    _newLevelDialogBox.Close();
                    _newLevelDialogBox = null;
                });
            };
            Core.engine.game.FadeScreen(_newLevelDialogBox.Open);
        };

        GetElement<Button>("back").onClick += (_, _) => {
            if(Core.engine.resources.TryGetResource(MainMenuScreen.GlobalId, out MainMenuScreen? screen))
                Core.engine.game.SwitchScreen(screen);
        };
    }

    public override void Open() {
        base.Open();
        if(LevelSerializer.TryReadScoreList(out Dictionary<Guid, LevelSerializer.LevelScore[]>? scores))
            _scores = scores;
        GenerateLevelSelector();
    }

    private void GenerateLevelSelector() {
        ListBox<LevelSerializer.LevelItem> levels = GetElement<ListBox<LevelSerializer.LevelItem>>("levels");
        levels.Clear();
        foreach(LevelSerializer.LevelItem item in LevelSerializer.ReadLevelList())
            levels.Add(item);
    }

    private void ResetMetadataPanel() => UpdateMetadataPanel(null);
    public void UpdateMetadataPanel(LevelSerializer.LevelMetadata? metadata) {
        Text labels = GetElement<Text>("metadata.labels");
        Text difficulty = GetElement<Text>("metadata.difficulty");
        Text author = GetElement<Text>("metadata.author");
        Text description = GetElement<Text>("metadata.description");

        labels.enabled = metadata is not null;
        difficulty.text = metadata?.difficulty.ToString();
        author.text = metadata?.author;
        description.text = metadata?.description;

        // TODO: move this clamp to a separate class
        difficulty.UpdateColors(colors.colors, layoutName, "metadata.difficulty",
            Math.Clamp(metadata?.difficulty ?? 0, 1, 10).ToString());
        author.UpdateColors(colors.colors, layoutName, "metadata.author",
            authorToSpecial.TryGetValue(author.text ?? string.Empty, out string? special) ? special : null);
    }

    private void ResetScoreList() => GetElement<ListBox<LevelSerializer.LevelScore>>("scores").Clear();
    public void UpdateScoreList(Guid levelGuid) {
        ListBox<LevelSerializer.LevelScore> scores = GetElement<ListBox<LevelSerializer.LevelScore>>("scores");

        if(!_scores.TryGetValue(levelGuid, out LevelSerializer.LevelScore[]? currentScores))
            currentScores = Array.Empty<LevelSerializer.LevelScore>();

        scores.Clear();
        foreach(LevelSerializer.LevelScore currentScore in currentScores)
            scores.Add(currentScore);
    }

    public override void Close() {
        base.Close();
        ResetMetadataPanel();
        ResetScoreList();
    }

    public override void Update(TimeSpan time) {
        bool prevInputBlock = input.block;
        input.block = _newLevelDialogBox is not null;

        base.Update(time);
        foreach((string id, Element element) in elements) {
            bool isPlay = id.StartsWith("play_", StringComparison.Ordinal);
            bool isEdit = !isPlay && id.StartsWith("edit_", StringComparison.Ordinal);
            if(isPlay && mode == PlayerMode.Play || isEdit && mode == PlayerMode.Edit || !isPlay && !isEdit)
                element.Update(time);
        }

        input.block = prevInputBlock;

        _newLevelDialogBox?.Update(time);
    }

    public override void Tick(TimeSpan time) { }
}
