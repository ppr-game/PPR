using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;

using PRR.UI;
using PRR.UI.Resources;

namespace PPR.Screens.Templates;

public class LevelSelectorTemplate : ListBoxTemplateResource<LevelSerializer.LevelItem> {
    public const string GlobalId = "layouts/templates/levelItem";

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInput input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "levelItem";

    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "level", typeof(LayoutResourceButton) },
        { "error_level", typeof(LayoutResourceButton) }
    };

    private readonly LevelSelectScreen _screen;

    public LevelSelectorTemplate(LevelSelectScreen screen) => _screen = screen;

    private class Template : BasicTemplate {
        private readonly LevelSelectorTemplate _resource;
        private LevelSerializer.LevelItem _item;
        private Button? _selectedLevelButton;

        public Template(LevelSelectorTemplate resource) : base(resource) {
            _resource = resource;

            void LevelHover(object? caller, EventArgs eventArgs) {
                if(caller is not Button levelButton)
                    throw new InvalidOperationException("wtf??");

                if(_selectedLevelButton is not null)
                    _selectedLevelButton.toggled = false;
                levelButton.toggled = true;
                _resource._screen.UpdateMetadataPanel(_item.metadata);
                _resource._screen.UpdateScoreList(_item.metadata.guid);
                _selectedLevelButton = levelButton;
                if(!_item.hasErrors)
                    Conductor.SetMusic(_item.path, _item.music);
            }

            GetElement<Button>("level").onHover += LevelHover;
            GetElement<Button>("error_level").onHover += LevelHover;
        }

        public override void UpdateWithItem(int index, LevelSerializer.LevelItem item, int width) {
            _item = item;

            GetElement<Button>(item.hasErrors ? "level" : "error_level").enabled = false;
            Button levelButton = GetElement<Button>(item.hasErrors ? "error_level" : "level");
            levelButton.enabled = true;
            levelButton.text =
                item.metadata.name.Length > levelButton.size.x ? item.metadata.name[..levelButton.size.x] :
                    item.metadata.name;
            levelButton.toggled = false;
        }
    }

    public override IListBoxTemplateFactory<LevelSerializer.LevelItem>.Template CreateTemplate() => new Template(this);
}
