using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Util;

using PRR.UI;
using PRR.UI.Resources;

namespace PPR.Screens;

public class LevelSelectScreen : ScreenResourceBase {
    public const string GlobalId = "layouts/levelSelect";

    public PlayerMode mode { get; set; }

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInput input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "levelSelect";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "frame", typeof(LayoutResourceText) },
        { "scores", typeof(LayoutResourceScrollablePanel) },
        { "template_score.score", typeof(LayoutResourceText) },
        { "template_score.accuracy", typeof(LayoutResourceText) },
        { "template_score.middleDivider", typeof(LayoutResourceText) },
        { "template_score.maxCombo", typeof(LayoutResourceText) },
        { "template_score.mini.misses", typeof(LayoutResourceText) },
        { "template_score.mini.hits", typeof(LayoutResourceText) },
        { "template_score.mini.perfectHits", typeof(LayoutResourceText) },
        { "template_score.divider", typeof(LayoutResourceText) },
        { "levels", typeof(LayoutResourceScrollablePanel) },
        { "template_level", typeof(LayoutResourceButton) },
        { "template_error_level", typeof(LayoutResourceButton) },
        { "metadata.difficulty", typeof(LayoutResourceText) },
        { "metadata.author", typeof(LayoutResourceText) },
        { "metadata.description", typeof(LayoutResourceText) },
        { "back", typeof(LayoutResourceButton) }
    };

    private const string LevelsPath = "levels";
    private const string TemplateLevelName = "_template";
    private const string MetadataFileName = "metadata.json";
    private const string ScoresPath = "scores.json";

    private Dictionary<Guid, LevelScore[]> _scores = new();
    private Button? _selectedLevelButton;

    public override void Load(string id, IResources resources) {
        base.Load(id, resources);

        if(elements["back"] is Button back) back.onClick += (_, _) => {
            if(Core.engine.resources.TryGetResource(MainMenuScreen.GlobalId, out MainMenuScreen? screen))
                Core.engine.game.SwitchScreen(screen);
        };
    }

    public override void Open() {
        if(elements["levels"] is not ScrollablePanel levels ||
            elements["template_level"] is not Button levelTemplate ||
            elements["template_error_level"] is not Button errorLevelTemplate ||
            elements["template_score.score"] is not Text scoreTemplate ||
            elements["template_score.accuracy"] is not Text accuracyTemplate ||
            elements["template_score.middleDivider"] is not Text middleDividerTemplate ||
            elements["template_score.maxCombo"] is not Text maxComboTemplate ||
            elements["template_score.mini.misses"] is not Text missesTemplate ||
            elements["template_score.mini.hits"] is not Text hitsTemplate ||
            elements["template_score.mini.perfectHits"] is not Text perfectHitsTemplate ||
            elements["template_score.divider"] is not Text dividerTemplate) return;
        levelTemplate.enabled = false;
        errorLevelTemplate.enabled = false;
        scoreTemplate.enabled = false;
        accuracyTemplate.enabled = false;
        middleDividerTemplate.enabled = false;
        maxComboTemplate.enabled = false;
        missesTemplate.enabled = false;
        hitsTemplate.enabled = false;
        perfectHitsTemplate.enabled = false;
        dividerTemplate.enabled = false;

        if(TryReadScoreList(out Dictionary<Guid, LevelScore[]>? scores))
            _scores = scores;

        GenerateLevelSelector(levels, levelTemplate, errorLevelTemplate, scoreTemplate, accuracyTemplate,
            middleDividerTemplate, maxComboTemplate, missesTemplate, hitsTemplate, perfectHitsTemplate,
            dividerTemplate);
    }

    private static bool TryReadScoreList([NotNullWhen(true)] out Dictionary<Guid, LevelScore[]>? scores) {
        if(!File.Exists(ScoresPath)) {
            scores = null;
            return false;
        }

        FileStream scoresFile = File.OpenRead(ScoresPath);
        try { scores = JsonSerializer.Deserialize<Dictionary<Guid, LevelScore[]>>(scoresFile); }
        catch(JsonException) { scores = null; }
        scoresFile.Close();

        return scores is not null;
    }

    private void GenerateLevelSelector(ScrollablePanel levels, Button levelTemplate, Button errorLevelTemplate,
        Text scoreTemplate, Text accuracyTemplate, Text middleDividerTemplate, Text maxComboTemplate,
        Text missesTemplate, Text hitsTemplate, Text perfectHitsTemplate, Text dividerTemplate) {
        levels.elements.Clear();

        int y = 0;
        foreach((LevelMetadata metadata, string path, string? error) in ReadLevelList()) {
            Button template = error is null ? levelTemplate : errorLevelTemplate;

            Button levelButton = Button.Clone(template);
            levelButton.enabled = true;
            levelButton.position =
                levels.position + template.position + new Vector2Int(0, y * template.size.y + levels.scroll);
            levelButton.text =
                metadata.name.Length > template.size.x ? metadata.name[..template.size.x] : metadata.name;
            levelButton.toggled = false;
            levelButton.onHover += (_, _) => {
                if(_selectedLevelButton is not null)
                    _selectedLevelButton.toggled = false;
                levelButton.toggled = true;
                UpdateMetadataPanel(metadata);
                UpdateScoresList(metadata.guid, scoreTemplate, accuracyTemplate, middleDividerTemplate,
                    maxComboTemplate, missesTemplate, hitsTemplate, perfectHitsTemplate, dividerTemplate);
                _selectedLevelButton = levelButton;
            };
            levels.elements.Add(levelButton);
            y++;
        }
    }

    private static IEnumerable<(LevelMetadata metadata, string path, string? error)> ReadLevelList() {
        foreach(string levelDirectory in Directory.EnumerateDirectories(LevelsPath)) {
            string directoryName = Path.GetFileName(levelDirectory);
            if(directoryName == TemplateLevelName)
                continue;

            LevelMetadata metadata = new(0, Guid.Empty, directoryName, string.Empty, string.Empty, -1);
            string metadataPath = Path.Join(levelDirectory, MetadataFileName);
            if(!File.Exists(metadataPath)) {
                yield return (metadata, levelDirectory, "Metadata file not found.");
                continue;
            }

            FileStream metadataFile = File.OpenRead(metadataPath);

            JsonException? jsonException = null;
            try { metadata = JsonSerializer.Deserialize<LevelMetadata>(metadataFile); }
            catch(JsonException ex) { jsonException = ex; }

            metadataFile.Close();

            if(jsonException is not null) {
                yield return (metadata, levelDirectory,
                    $"Error in {jsonException.Path} at {jsonException.LineNumber}:\n{jsonException.Message}");
                continue;
            }

            yield return (metadata, levelDirectory, null);
        }
    }

    private void UpdateMetadataPanel(LevelMetadata metadata) {
        if(elements["metadata.difficulty"] is not Text difficulty ||
            elements["metadata.author"] is not Text author ||
            elements["metadata.description"] is not Text description) return;

        difficulty.text = metadata.difficulty.ToString();
        author.text = metadata.author;
        description.text = metadata.description;
    }

    private void UpdateScoresList(Guid levelGuid, Text scoreTemplate, Text accuracyTemplate,
        Text middleDividerTemplate, Text maxComboTemplate, Text missesTemplate, Text hitsTemplate,
        Text perfectHitsTemplate, Text dividerTemplate) {
        if(elements["scores"] is not ScrollablePanel scores)
            return;

        if(!_scores.TryGetValue(levelGuid, out LevelScore[]? currentScores))
            currentScores = Array.Empty<LevelScore>();

        foreach(Element element in scores.elements)
            element.enabled = false;

        for(int i = 0; i < currentScores.Length; i++)
            UpdateScore(scores, scoreTemplate, accuracyTemplate, middleDividerTemplate, maxComboTemplate,
                missesTemplate, hitsTemplate, perfectHitsTemplate, dividerTemplate, currentScores, i);
    }

    private static void UpdateScore(ScrollablePanel scores, Text scoreTemplate, Text accuracyTemplate,
        Text middleDividerTemplate, Text maxComboTemplate, Text missesTemplate, Text hitsTemplate,
        Text perfectHitsTemplate, Text dividerTemplate, IReadOnlyList<LevelScore> currentScores, int i) {
        LevelScore currentScore = currentScores[i];

        const int elementsPerScore = 8;
        const int scoreIndex = 0;
        const int accuracyIndex = 1;
        const int middleDividerIndex = 2;
        const int maxComboIndex = 3;
        const int missesIndex = 4;
        const int hitsIndex = 5;
        const int perfectHitsIndex = 6;

        int offset = i * elementsPerScore;

        if(scores.elements.Count <= offset)
            GenerateNewScore(scores, scoreTemplate, accuracyTemplate, middleDividerTemplate, maxComboTemplate,
                missesTemplate, hitsTemplate, perfectHitsTemplate, dividerTemplate);

        for(int j = 0; j < elementsPerScore; j++)
            scores.elements[offset + j].enabled = true;

        if(scores.elements[offset + scoreIndex] is Text score)
            score.text = string.Format(scoreTemplate.text ?? "{0}", currentScore.score.ToString());
        if(scores.elements[offset + accuracyIndex] is Text accuracy &&
            scores.elements[offset + maxComboIndex] is Text maxCombo) {
            Element middleDivider = scores.elements[offset + middleDividerIndex];

            Vector2Int tempVector = new(accuracy.text?.Length ?? 0, 0);
            middleDivider.position -= tempVector;
            maxCombo.position -= tempVector;

            accuracy.text = string.Format(accuracyTemplate.text ?? "{0}", currentScore.accuracy.ToString());
            maxCombo.text = string.Format(maxComboTemplate.text ?? "{0}", currentScore.maxCombo.ToString());

            tempVector = new Vector2Int(accuracy.text.Length, 0);
            middleDivider.position += tempVector;
            maxCombo.position += tempVector;
        }
        // ReSharper disable once InvertIf
        if(scores.elements[offset + missesIndex] is Text misses &&
            scores.elements[offset + hitsIndex] is Text hits &&
            scores.elements[offset + perfectHitsIndex] is Text perfectHits) {
            misses.text = string.Format(missesTemplate.text ?? "{0}", currentScore.scores[0].ToString());
            hits.text = string.Format(hitsTemplate.text ?? "{0}", currentScore.scores[1].ToString());
            perfectHits.text = string.Format(perfectHitsTemplate.text ?? "{0}", currentScore.scores[2].ToString());

            hits.position = new Vector2Int(misses.position.x + misses.text.Length + 1, hits.position.y);
            perfectHits.position = new Vector2Int(hits.position.x + hits.text.Length + 1, perfectHits.position.y);
        }
    }

    private static void GenerateNewScore(ScrollablePanel scores, Text scoreTemplate, Text accuracyTemplate,
        Text middleDividerTemplate, Text maxComboTemplate, Text missesTemplate, Text hitsTemplate,
        Text perfectHitsTemplate, Text dividerTemplate) {
        Vector2Int offset = new(scores.position.x, scores.elements.Count > 0 ?
            scores.elements.Select(element => element.bounds.max.y).Max() + 1 :
            scores.position.y);

        Text score = Text.Clone(scoreTemplate);
        Text accuracy = Text.Clone(accuracyTemplate);
        Text middleDivider = Text.Clone(middleDividerTemplate);
        Text maxCombo = Text.Clone(maxComboTemplate);
        Text misses = Text.Clone(missesTemplate);
        Text hits = Text.Clone(hitsTemplate);
        Text perfectHits = Text.Clone(perfectHitsTemplate);
        Text divider = Text.Clone(dividerTemplate);

        score.position += offset;
        accuracy.position += offset;
        middleDivider.position += offset;
        maxCombo.position += offset;
        misses.position += offset;
        hits.position += offset;
        perfectHits.position += offset;
        divider.position += offset;

        scores.elements.Add(score);
        scores.elements.Add(accuracy);
        scores.elements.Add(middleDivider);
        scores.elements.Add(maxCombo);
        scores.elements.Add(misses);
        scores.elements.Add(hits);
        scores.elements.Add(perfectHits);
        scores.elements.Add(divider);
    }

    public override void Close() { }

    public override void Update() {
        foreach((string id, Element element) in elements) {
            bool isPlay = id.StartsWith("play_", StringComparison.Ordinal);
            bool isEdit = !isPlay && id.StartsWith("edit_", StringComparison.Ordinal);
            if(isPlay && mode == PlayerMode.Play || isEdit && mode == PlayerMode.Edit || !isPlay && !isEdit)
                element.Update(Core.engine.clock);
        }
    }

    public override void Tick() { }
}
