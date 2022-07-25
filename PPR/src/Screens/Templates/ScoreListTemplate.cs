using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Util;

using PRR.UI;
using PRR.UI.Resources;

namespace PPR.Screens.Templates;

public class ScoreListTemplate : ListBoxTemplateResource<LevelSerializer.LevelScore> {
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

    private string _scoreTemplate = "{0}";
    private string _accuracyTemplate = "{0}";
    private string _maxComboTemplate = "{0}";
    private string _missesTemplate = "{0}";
    private string _hitsTemplate = "{0}";
    private string _perfectHitsTemplate = "{0}";

    public override void Load(string id) {
        base.Load(id);
        _scoreTemplate = GetElement<Text>("score").text ?? _scoreTemplate;
        _accuracyTemplate = GetElement<Text>("accuracy").text ?? _accuracyTemplate;
        _maxComboTemplate = GetElement<Text>("maxCombo").text ?? _maxComboTemplate;
        _missesTemplate = GetElement<Text>("mini.misses").text ?? _maxComboTemplate;
        _hitsTemplate = GetElement<Text>("mini.hits").text ?? _hitsTemplate;
        _perfectHitsTemplate = GetElement<Text>("mini.perfectHits").text ?? _perfectHitsTemplate;
    }

    private class Template : BasicTemplate {
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
                _resource.colors.colors.TryGetValue(item.accuracy >= 100 ? "accuracy_good" :
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
