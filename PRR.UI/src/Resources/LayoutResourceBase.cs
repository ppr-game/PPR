using System.Text.Json;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

using PER.Abstractions;
using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Common.Resources;
using PER.Util;

namespace PRR.UI.Resources;

[PublicAPI]
public abstract class LayoutResourceBase : JsonResourceBase<IDictionary<string, LayoutResourceElement>> {
    [PublicAPI]
    protected class LayoutResourceText : LayoutResourceElement {
        [PublicAPI]
        public readonly struct TextFormatting {
            public string? foregroundColor { get; }
            public string? backgroundColor { get; }
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public RenderStyle? style { get; }
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public RenderOptions? options { get; }
            public string? effect { get; }

            [JsonConstructor]
            public TextFormatting(string? foregroundColor, string? backgroundColor, RenderStyle? style,
                RenderOptions? options, string? effect = null) {
                this.foregroundColor = foregroundColor;
                this.backgroundColor = backgroundColor;
                this.style = style;
                this.options = options;
                this.effect = effect;
            }

            public Formatting GetFormatting(Dictionary<string, Color> colors, Dictionary<string, IEffect?> effects) {
                Color foregroundColor = Color.white;
                Color backgroundColor = Color.transparent;
                RenderStyle style = RenderStyle.None;
                RenderOptions options = RenderOptions.Default;
                IEffect? effect = null;
                if(this.foregroundColor is not null && colors.TryGetValue(this.foregroundColor, out Color color))
                    foregroundColor = color;
                if(this.backgroundColor is not null && colors.TryGetValue(this.backgroundColor, out color))
                    backgroundColor = color;
                if(this.style.HasValue) style = this.style.Value;
                if(this.options.HasValue) options = this.options.Value;
                if(this.effect is not null) effects.TryGetValue(this.effect, out effect);
                return new Formatting(foregroundColor, backgroundColor, style, options, effect);
            }
        }

        public string? path { get; }
        public string? text { get; }
        public Dictionary<char, TextFormatting>? formatting { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HorizontalAlignment? align { get; }
        public bool? wrap { get; }

        public LayoutResourceText(bool? enabled, Vector2Int position, Vector2Int size, string? path, string? text,
            Dictionary<char, TextFormatting>? formatting, HorizontalAlignment? align, bool? wrap) :
            base(enabled, position, size) {
            this.path = path;
            this.text = text;
            this.formatting = formatting;
            this.align = align;
            this.wrap = wrap;
        }

        public override Element GetElement(IResources resources, IRenderer renderer, IInput input, IAudio audio,
            Dictionary<string, Color> colors, string layoutName, string id) {
            Text element = new(renderer) {
                position = position,
                size = size,
                text = text
            };
            if(path is not null &&
               // kill me please
               resources.TryGetPath(Path.Combine(path.Split('/').Prepend("layouts").ToArray()),
                   out string? filePath))
                element.text = File.ReadAllText(filePath);
            if(enabled.HasValue) element.enabled = enabled.Value;
            if(formatting is not null)
                foreach((char flag, TextFormatting textFormatting) in formatting)
                    element.formatting.Add(flag,
                        textFormatting.GetFormatting(colors, renderer.formattingEffects));
            if(align.HasValue) element.align = align.Value;
            if(wrap.HasValue) element.wrap = wrap.Value;
            element.UpdateColors(colors, layoutName, id, null);
            return element;
        }
    }

    [PublicAPI]
    protected class LayoutResourceButton : LayoutResourceElement {
        public string? text { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RenderStyle? style { get; }
        public bool? active { get; }
        public bool? toggled { get; }

        public LayoutResourceButton(bool? enabled, Vector2Int position, Vector2Int size, string? text,
            RenderStyle? style, bool? active, bool? toggled) : base(enabled, position, size) {
            this.text = text;
            this.style = style;
            this.active = active;
            this.toggled = toggled;
        }

        public override Element GetElement(IResources resources, IRenderer renderer, IInput input, IAudio audio,
            Dictionary<string, Color> colors, string layoutName, string id) {
            Button element = new(renderer, input, audio) {
                position = position,
                size = size,
                text = text
            };
            if(enabled.HasValue) element.enabled = enabled.Value;
            if(style.HasValue) element.style = style.Value;
            if(active.HasValue) element.active = active.Value;
            if(toggled.HasValue) element.toggled = toggled.Value;
            element.UpdateColors(colors, layoutName, id, null);
            return element;
        }
    }

    [PublicAPI]
    protected class LayoutResourceInputField : LayoutResourceElement {
        public string? value { get; }
        public string? placeholder { get; }
        public bool? wrap { get; }
        public int? cursor { get; }
        public float? blinkRate { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RenderStyle? style { get; }
        public bool? active { get; }

        public LayoutResourceInputField(bool? enabled, Vector2Int position, Vector2Int size, string? value,
            string? placeholder, bool? wrap, int? cursor, float? blinkRate, RenderStyle? style, bool? active) :
            base(enabled, position, size) {
            this.value = value;
            this.placeholder = placeholder;
            this.wrap = wrap;
            this.cursor = cursor;
            this.blinkRate = blinkRate;
            this.style = style;
            this.active = active;
        }

        public override Element GetElement(IResources resources, IRenderer renderer, IInput input, IAudio audio,
            Dictionary<string, Color> colors, string layoutName, string id) {
            InputField element = new(renderer, input, audio) {
                position = position,
                size = size,
                value = value,
                placeholder = placeholder
            };
            if(enabled.HasValue) element.enabled = enabled.Value;
            if(wrap.HasValue) element.wrap = wrap.Value;
            if(cursor.HasValue) element.cursor = cursor.Value;
            if(blinkRate.HasValue) element.blinkRate = blinkRate.Value;
            if(style.HasValue) element.style = style.Value;
            if(active.HasValue) element.active = active.Value;
            element.UpdateColors(colors, layoutName, id, null);
            return element;
        }
    }

    [PublicAPI]
    protected class LayoutResourceSlider : LayoutResourceElement {
        public int? width { get; }
        public float? value { get; }
        public float? minValue { get; }
        public float? maxValue { get; }
        public bool? active { get; }

        public LayoutResourceSlider(bool? enabled, Vector2Int position, Vector2Int size, int? width, float? value,
            float? minValue, float? maxValue, bool? active) : base(enabled, position, size) {
            this.width = width;
            this.value = value;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.active = active;
        }

        // ReSharper disable once CognitiveComplexity
        public override Element GetElement(IResources resources, IRenderer renderer, IInput input, IAudio audio,
            Dictionary<string, Color> colors, string layoutName, string id) {
            Slider element = new(renderer, input, audio) {
                position = position,
                size = size
            };
            if(enabled.HasValue) element.enabled = enabled.Value;
            if(width.HasValue) element.width = width.Value;
            if(minValue.HasValue) element.minValue = minValue.Value;
            if(maxValue.HasValue) element.maxValue = maxValue.Value;
            if(value.HasValue) element.value = value.Value;
            if(active.HasValue) element.active = active.Value;
            element.UpdateColors(colors, layoutName, id, null);
            return element;
        }
    }

    [PublicAPI]
    protected class LayoutResourceScrollablePanel : LayoutResourceElement {
        public LayoutResourceScrollablePanel(bool? enabled, Vector2Int position, Vector2Int size) :
            base(enabled, position, size) { }

        public override Element GetElement(IResources resources, IRenderer renderer, IInput input, IAudio audio,
            Dictionary<string, Color> colors, string layoutName, string id) {
            ScrollablePanel element = new(renderer, input) {
                position = position,
                size = size
            };
            if(enabled.HasValue) element.enabled = enabled.Value;
            element.UpdateColors(colors, layoutName, id, null);
            return element;
        }
    }

    [PublicAPI]
    protected class LayoutResourceListBox<TItem> : LayoutResourceScrollablePanel {
        public string template { get; }

        public LayoutResourceListBox(bool? enabled, Vector2Int position, Vector2Int size, string template) :
            base(enabled, position, size) => this.template = template;

        public override Element GetElement(IResources resources, IRenderer renderer, IInput input, IAudio audio,
            Dictionary<string, Color> colors, string layoutName, string id) {
            if(!resources.TryGetResource($"layouts/templates/{template}",
                out ListBoxTemplateResourceBase<TItem>? templateFactory))
                throw new InvalidOperationException("Missing dependency.");
            ListBox<TItem> element = new(renderer, input, templateFactory) {
                position = position,
                size = size
            };
            if(enabled.HasValue) element.enabled = enabled.Value;
            element.UpdateColors(colors, layoutName, id, null);
            return element;
        }
    }

    protected abstract IRenderer renderer { get; }
    protected abstract IInput input { get; }
    protected abstract IAudio audio { get; }

    protected virtual string layoutsPath => "layouts";
    protected abstract string layoutName { get; }
    protected abstract IReadOnlyDictionary<string, Type> elementTypes { get; }

    protected IEnumerable<KeyValuePair<string, Element>> elements => _elements;
    protected ColorsResource colors { get; private set; } = new();

    private Dictionary<string, Element> _elements = new();

    public override void Load(string id, IResources resources) {
        if(!resources.TryGetResource(ColorsResource.GlobalId, out ColorsResource? colors))
            throw new InvalidOperationException("Missing colors resource.");
        this.colors = colors;

        Dictionary<string, LayoutResourceElement> layoutElements = new(elementTypes.Count);
        DeserializeAllJson(resources, Path.Combine(layoutsPath, $"{layoutName}.json"), layoutElements,
            () => layoutElements.Count == elementTypes.Count);

        // didn't load all the elements
        if(layoutElements.Count != elementTypes.Count)
            throw new InvalidOperationException("Not all elements were loaded.");

        _elements.Clear();
        foreach((string elementId, LayoutResourceElement layoutElement) in layoutElements) {
            Element element = layoutElement.GetElement(resources, renderer,
                input, audio, colors.colors, layoutName, elementId);
            _elements.Add(elementId, element);
        }
    }

    protected override void DeserializeJson(string path, IDictionary<string, LayoutResourceElement> deserialized) {
        FileStream file = File.OpenRead(path);
        Dictionary<string, JsonElement>? layout = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(file);
        file.Close();

        if(layout is null)
            return;

        foreach((string elementId, Type type) in elementTypes) {
            if(!layout.TryGetValue(elementId, out JsonElement jsonElement))
                throw new InvalidOperationException($"Element {elementId} is missing.");
            CheckElementType(type);
            LayoutResourceElement? layoutElement = (LayoutResourceElement?)jsonElement.Deserialize(type);
            if(layoutElement is null)
                throw new InvalidOperationException($"Failed to deserialize {elementId} as {type.Name}.");
            if(!deserialized.ContainsKey(elementId))
                deserialized.Add(elementId, layoutElement);
        }
    }

    private static void CheckElementType(Type? type) {
        do {
            type = type?.BaseType;
            if(type is null)
                throw new InvalidOperationException(
                    $"Element type specs can only inherit from {nameof(LayoutResourceElement)}.");
        } while(type != typeof(LayoutResourceElement));
    }

    public override void Unload(string id, IResources resources) => _elements.Clear();

    protected Element GetElement(string id) {
        if(!_elements.ContainsKey(id))
            throw new InvalidOperationException($"Element {id} does not exist.");
        return _elements[id];
    }

    protected T GetElement<T>(string id) where T : Element {
        Element element = GetElement(id);
        if(element is not T typedElement)
            throw new InvalidOperationException($"Element {id} is not {nameof(T)}.");

        return typedElement;
    }
}
