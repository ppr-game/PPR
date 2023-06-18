using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;

namespace PPR.Screens;

public class PlayScreen : GameScreen {
    public const string GlobalId = "layouts/play";

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInput input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "play";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> { };

    public override void Open() { }
    public override void Close() { }
    public override void Update(TimeSpan time) { }
    public override void Tick(TimeSpan time) { }
}
