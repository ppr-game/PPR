using JetBrains.Annotations;

using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI;

[PublicAPI]
public class ScrollablePanel : Element {
    public IInput input { get; set; }

    public override bool enabled {
        get => base.enabled;
        set {
            base.enabled = value;
            if(value) input.scrolled += Scrolled;
            else input.scrolled -= Scrolled;
        }
    }

    public List<Element> elements { get; } = new();

    public int scroll {
        get => _scroll;
        set {
            int delta = value - _scroll;
            _scroll = value;
            foreach(Element element in elements)
                element.position += new Vector2Int(0, delta);
        }
    }

    private bool _allowScrolling;
    private int _scroll;

    public ScrollablePanel(IRenderer renderer, IInput input) : base(renderer) {
        this.input = input;
        input.scrolled += Scrolled;
    }

    // ReSharper disable once CognitiveComplexity
    private void Scrolled(object? o, IInput.ScrolledEventArgs args) {
        if(!input.mousePosition.InBounds(bounds) || !_allowScrolling || elements.Count == 0)
            return;
        _allowScrolling = false;

        int lowestY = int.MaxValue;
        int highestY = int.MinValue;
        foreach(Element element in elements) {
            if(element.bounds.min.y < lowestY)
                lowestY = element.bounds.min.y;
            if(element.bounds.max.y > highestY)
                highestY = element.bounds.max.y;
        }

        int delta = (int)args.delta;

        if(delta < 0 && highestY + delta < bounds.max.y ||
           delta > 0 && lowestY + delta > bounds.min.y)
            return;

        scroll += delta;
    }

    public override void Update(IReadOnlyStopwatch clock) {
        if(!enabled)
            return;
        _allowScrolling = !input.block;
        // ReSharper disable once ForCanBeConvertedToForeach
        for(int i = 0; i < elements.Count; i++) {
            Element element = elements[i];
            if(element.position.y < bounds.min.y || element.position.y > bounds.max.y)
                continue;
            elements[i].Update(clock);
        }
    }

    public override void UpdateColors(Dictionary<string, Color> colors, string layoutName, string id,
        string? special) { }
}
