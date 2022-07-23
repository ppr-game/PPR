using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Common.Resources;
using PER.Util;

using PRR.UI;
using PRR.UI.Resources;

namespace PPR.Screens;

public class LevelSelectScreen : MenuWithCoolBackgroundAnimationScreenResourceBase {
    public const string GlobalId = "layouts/levelSelect";

    public PlayerMode mode { get; set; }

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInput input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "levelSelect";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "frame", typeof(LayoutResourceText) },
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

    public static readonly IReadOnlyDictionary<string, string> authorToSpecial = new Dictionary<string, string> {
        { "ConfiG", "ConfiG" },
        { "sbeve", "contributor" }
    };

    private Dictionary<Guid, LevelSerializer.LevelScore[]> _scores = new();
    private Button? _selectedLevelButton;

    private NewLevelDialogBoxScreen? _newLevelDialogBox;

    public LevelSelectScreen(IResources resources) {
        resources.TryAddResource(LevelSelectorTemplate.GlobalId, new LevelSelectorTemplate());
        resources.TryAddResource(ScoreListTemplate.GlobalId, new ScoreListTemplate());
    }

    public override void Load(string id, IResources resources) {
        base.Load(id, resources);

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

    private class LevelSelectorTemplate : ListBoxTemplateResourceBase<LevelSerializer.LevelItem> {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public const string GlobalId = "layouts/templates/levelItem";

        protected override IRenderer renderer => Core.engine.renderer;
        protected override IInput input => Core.engine.input;
        protected override IAudio audio => Core.engine.audio;

        protected override string layoutName => "levelItem";

        protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
            { "level", typeof(LayoutResourceButton) },
            { "error_level", typeof(LayoutResourceButton) }
        };

        private LevelSelectScreen? _screen;

        public override void Load(string id, IResources resources) {
            base.Load(id, resources);
            if(!resources.TryGetResource(LevelSelectScreen.GlobalId, out LevelSelectScreen? screen))
                throw new InvalidOperationException("Missing dependency.");
            _screen = screen;
        }

        private class Template : TemplateBase {
            private readonly LevelSelectorTemplate _resource;

            public Template(LevelSelectorTemplate resource) : base(resource) => _resource = resource;

            public override void UpdateWithItem(int index, LevelSerializer.LevelItem item, int width) {
                if(_resource._screen is null)
                    return;
                LevelSelectScreen screen = _resource._screen;

                GetElement<Button>(item.hasErrors ? "level" : "error_level").enabled = false;
                Button levelButton = GetElement<Button>(item.hasErrors ? "error_level" : "level");
                levelButton.enabled = true;
                levelButton.text =
                    item.metadata.name.Length > levelButton.size.x ? item.metadata.name[..levelButton.size.x] :
                        item.metadata.name;
                levelButton.toggled = false;
                levelButton.onHover += (_, _) => {
                    if(screen._selectedLevelButton is not null)
                        screen._selectedLevelButton.toggled = false;
                    levelButton.toggled = true;
                    screen.UpdateMetadataPanel(item.metadata);
                    screen.UpdateScoreList(item.metadata.guid);
                    screen._selectedLevelButton = levelButton;
                    if(!item.hasErrors)
                        Conductor.SetMusic(item.path, item.music);
                };
            }
        }

        public override IListBoxTemplateFactory<LevelSerializer.LevelItem>.Template CreateTemplate() => new Template(this);
    }

    private void GenerateLevelSelector() {
        ListBox<LevelSerializer.LevelItem> levels = GetElement<ListBox<LevelSerializer.LevelItem>>("levels");
        levels.Clear();
        foreach(LevelSerializer.LevelItem item in LevelSerializer.ReadLevelList())
            levels.Add(item);
    }

    private void ResetMetadataPanel() => UpdateMetadataPanel(null);
    private void UpdateMetadataPanel(LevelSerializer.LevelMetadata? metadata) {
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

    private void ResetScoreList() {
        foreach(Element element in GetElement<ScrollablePanel>("scores").elements)
            element.enabled = false;
    }

    private void UpdateScoreList(Guid levelGuid) {
        ListBox<LevelSerializer.LevelScore> scores = GetElement<ListBox<LevelSerializer.LevelScore>>("scores");

        if(!_scores.TryGetValue(levelGuid, out LevelSerializer.LevelScore[]? currentScores))
            currentScores = Array.Empty<LevelSerializer.LevelScore>();

        scores.Clear();
        foreach(LevelSerializer.LevelScore currentScore in currentScores)
            scores.Add(currentScore);
    }

    private class ScoreListTemplate : ListBoxTemplateResourceBase<LevelSerializer.LevelScore> {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public const string GlobalId = "layouts/templates/scoreItem";

        protected override IRenderer renderer => Core.engine.renderer;
        protected override IInput input => Core.engine.input;
        protected override IAudio audio => Core.engine.audio;

        protected override string layoutName => "scoreItem";
        protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
            { "score", typeof(LayoutResourceText) },
            { "accuracy", typeof(LayoutResourceText) },
            { "middleDivider", typeof(LayoutResourceText) },
            { "maxCombo", typeof(LayoutResourceText) },
            { "mini.misses", typeof(LayoutResourceText) },
            { "mini.hits", typeof(LayoutResourceText) },
            { "mini.perfectHits", typeof(LayoutResourceText) },
            { "divider", typeof(LayoutResourceText) }
        };

        private ColorsResource _colors = new();

        private string _scoreTemplate = "{0}";
        private string _accuracyTemplate = "{0}";
        private string _maxComboTemplate = "{0}";
        private string _missesTemplate = "{0}";
        private string _hitsTemplate = "{0}";
        private string _perfectHitsTemplate = "{0}";

        public override void Load(string id, IResources resources) {
            base.Load(id, resources);
            if(!resources.TryGetResource(ColorsResource.GlobalId, out ColorsResource? colors))
                throw new InvalidOperationException("Missing dependency.");
            _colors = colors;

            _scoreTemplate = GetElement<Text>("score").text ?? _scoreTemplate;
            _accuracyTemplate = GetElement<Text>("accuracy").text ?? _accuracyTemplate;
            _maxComboTemplate = GetElement<Text>("maxCombo").text ?? _maxComboTemplate;
            _missesTemplate = GetElement<Text>("mini.misses").text ?? _maxComboTemplate;
            _hitsTemplate = GetElement<Text>("mini.hits").text ?? _hitsTemplate;
            _perfectHitsTemplate = GetElement<Text>("mini.perfectHits").text ?? _perfectHitsTemplate;
        }

        private class Template : TemplateBase {
            private readonly ScoreListTemplate _resource;

            private readonly int _middleDividerOffset;
            private readonly int _maxComboOffset;
            private readonly int _hitsOffset;
            private readonly int _perfectHitsOffset;

            public Template(ScoreListTemplate resource) : base(resource) {
                height++;
                _resource = resource;

                Text accuracy = resource.GetElement<Text>("accuracy");
                Text middleDivider = resource.GetElement<Text>("middleDivider");
                Text maxCombo = resource.GetElement<Text>("maxCombo");
                _middleDividerOffset = middleDivider.position.x - ((accuracy.text?.Length ?? 0) + accuracy.position.x);
                _maxComboOffset = maxCombo.position.x - ((middleDivider.text?.Length ?? 0) + middleDivider.position.x);

                Text misses = resource.GetElement<Text>("mini.misses");
                Text hits = resource.GetElement<Text>("mini.hits");
                Text perfectHits = resource.GetElement<Text>("mini.perfectHits");
                _hitsOffset = hits.position.x - ((misses.text?.Length ?? 0) + misses.position.x);
                _perfectHitsOffset = perfectHits.position.x - ((hits.text?.Length ?? 0) + hits.position.x);
            }

            public override void UpdateWithItem(int index, LevelSerializer.LevelScore item, int width) {
                GetElement<Text>("score").text = string.Format(_resource._scoreTemplate, item.score.ToString());

                Text accuracy = GetElement<Text>("accuracy");
                Text middleDivider = GetElement<Text>("middleDivider");
                Text maxCombo = GetElement<Text>("maxCombo");

                accuracy.text = string.Format(_resource._accuracyTemplate, item.accuracy.ToString());
                maxCombo.text = string.Format(_resource._maxComboTemplate, item.maxCombo.ToString());

                offsets["middleDivider"] =
                    new Vector2Int((accuracy.text?.Length ?? 0) + offsets["accuracy"].x + _middleDividerOffset,
                        offsets["middleDivider"].y);
                offsets["maxCombo"] =
                    new Vector2Int((middleDivider.text?.Length ?? 0) + offsets["middleDivider"].x + _maxComboOffset,
                        offsets["maxCombo"].y);

                if(accuracy.formatting.TryGetValue('\0', out Formatting oldFormatting) &&
                    // TODO: move this color selection thing to a different class
                    _resource._colors.colors.TryGetValue(item.accuracy >= 100 ? "accuracy_good" :
                        item.accuracy >= 70 ? "accuracy_ok" : "accuracy_bad", out Color accuracyColor))
                    accuracy.formatting['\0'] = new Formatting(accuracyColor, oldFormatting.backgroundColor,
                        oldFormatting.style, oldFormatting.options, oldFormatting.effect);

                Text misses = GetElement<Text>("mini.misses");
                Text hits = GetElement<Text>("mini.hits");
                Text perfectHits = GetElement<Text>("mini.perfectHits");

                misses.text = string.Format(_resource._missesTemplate, item.scores[0].ToString());
                hits.text = string.Format(_resource._hitsTemplate, item.scores[1].ToString());
                perfectHits.text = string.Format(_resource._perfectHitsTemplate, item.scores[2].ToString());

                offsets["mini.hits"] =
                    new Vector2Int((misses.text?.Length ?? 0) + offsets["mini.misses"].x + _hitsOffset,
                        offsets["mini.hits"].y);
                offsets["mini.perfectHits"] =
                    new Vector2Int((hits.text?.Length ?? 0) + offsets["mini.hits"].x + _perfectHitsOffset,
                        offsets["mini.perfectHits"].y);
            }
        }

        public override IListBoxTemplateFactory<LevelSerializer.LevelScore>.Template CreateTemplate() => new Template(this);
    }

    public override void Close() {
        base.Close();
        _selectedLevelButton = null;
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
