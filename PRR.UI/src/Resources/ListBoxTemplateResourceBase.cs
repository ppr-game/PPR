using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI.Resources;

public abstract class ListBoxTemplateResourceBase<TItem> : LayoutResourceBase, IListBoxTemplateFactory<TItem> {
    protected override string layoutsPath => Path.Join(base.layoutsPath, "templates");

    protected abstract class TemplateBase : IListBoxTemplateFactory<TItem>.Template {
        public override IEnumerable<Element> elements => idElements.Values;
        protected IReadOnlyDictionary<string, Element> idElements { get; }
        protected Dictionary<string, Vector2Int> offsets { get; }
        protected int height { get; init; }

        protected TemplateBase(ListBoxTemplateResourceBase<TItem> resource) {
            Dictionary<string, Element> elements = new();
            Dictionary<string, Vector2Int> offsets = new();

            foreach((string id, Element element) in resource.elements) {
                elements.Add(id, element.Clone());
                offsets.Add(id, element.position);
            }

            idElements = elements;
            this.offsets = offsets;

            height = resource.elements.Select(element => element.Value.bounds.max.y).Max() + 1;
        }

        public override void MoveTo(Vector2Int origin, int index) {
            int yOffset = height * index;
            foreach((string? id, Element? element) in idElements)
                element.position = origin + offsets[id] + new Vector2Int(0, yOffset);
        }

        public override void Enable() {
            foreach(Element element in elements)
                element.enabled = true;
        }

        public override void Disable() {
            foreach(Element element in elements)
                element.enabled = false;
        }

        protected Element GetElement(string id) {
            if(!idElements.ContainsKey(id))
                throw new InvalidOperationException($"Element {id} does not exist.");
            return idElements[id];
        }

        protected T GetElement<T>(string id) where T : Element {
            Element element = GetElement(id);
            if(element is not T typedElement)
                throw new InvalidOperationException($"Element {id} is not {nameof(T)}.");

            return typedElement;
        }
    }

    public abstract IListBoxTemplateFactory<TItem>.Template CreateTemplate();
}
