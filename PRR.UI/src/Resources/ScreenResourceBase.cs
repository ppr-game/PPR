using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

using PER.Abstractions;
using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Common.Resources;
using PER.Util;

namespace PRR.UI.Resources;

public abstract class ScreenResourceBase : JsonResourceBase<IDictionary<string, LayoutResourceElement>>, IScreen {
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    protected class LayoutResourceText : LayoutResourceElement {
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

        public LayoutResourceText(bool? enabled, Vector2Int position, Vector2Int size, string? path, string? text,
            Dictionary<char, TextFormatting>? formatting, HorizontalAlignment? align) :
            base(enabled, position, size) {
            this.path = path;
            this.text = text;
            this.formatting = formatting;
            this.align = align;
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
               resources.TryGetPath(Path.Join(path.Split('/').Prepend("layouts").ToArray()),
                   out string? filePath))
                element.text = File.ReadAllText(filePath);
            if(enabled.HasValue) element.enabled = enabled.Value;
            if(formatting is not null)
                foreach((char flag, TextFormatting textFormatting) in formatting)
                    element.formatting.Add(flag,
                        textFormatting.GetFormatting(colors, renderer.formattingEffects));
            if(align.HasValue) element.align = align.Value;

            Color foregroundColor = Color.white;
            Color backgroundColor = Color.transparent;
            if(TryGetColor(colors, "text", layoutName, id, "fg", out Color color)) foregroundColor = color;
            if(TryGetColor(colors, "text", layoutName, id, "bg", out color)) backgroundColor = color;
            element.formatting.Add('\0', new Formatting(foregroundColor, backgroundColor));
            return element;
        }
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
            if(TryGetColor(colors, "button", layoutName, id, "inactive", out Color color))
                element.inactiveColor = color;
            if(TryGetColor(colors, "button", layoutName, id, "idle", out color))
                element.idleColor = color;
            if(TryGetColor(colors, "button", layoutName, id, "hover", out color))
                element.hoverColor = color;
            if(TryGetColor(colors, "button", layoutName, id, "click", out color))
                element.clickColor = color;
            return element;
        }
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
            if(value.HasValue) element.value = value.Value;
            if(minValue.HasValue) element.minValue = minValue.Value;
            if(maxValue.HasValue) element.maxValue = maxValue.Value;
            if(active.HasValue) element.active = active.Value;
            if(TryGetColor(colors, "slider", layoutName, id, "inactive", out Color color))
                element.inactiveColor = color;
            if(TryGetColor(colors, "slider", layoutName, id, "idle", out color))
                element.idleColor = color;
            if(TryGetColor(colors, "slider", layoutName, id, "hover", out color))
                element.hoverColor = color;
            if(TryGetColor(colors, "slider", layoutName, id, "click", out color))
                element.clickColor = color;
            return element;
        }
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
            return element;
        }
    }

    protected abstract IRenderer renderer { get; }
    protected abstract IInput input { get; }
    protected abstract IAudio audio { get; }

    protected abstract string layoutName { get; }
    protected abstract IReadOnlyDictionary<string, Type> elementTypes { get; }

    protected IReadOnlyDictionary<string, Element> elements { get; private set; } = new Dictionary<string, Element>();
    protected ColorsResource? colors { get; private set; }

    public override void Load(string id, IResources resources) {
        if(!resources.TryGetResource(ColorsResource.GlobalId, out ColorsResource? colors))
            throw new InvalidOperationException("Missing colors resource.");
        this.colors = colors;

        Dictionary<string, LayoutResourceElement> layoutElements = new(elementTypes.Count);
        DeserializeAllJson(resources, Path.Join("layouts", $"{layoutName}.json"), layoutElements,
            () => layoutElements.Count == elementTypes.Count);

        // didn't load all the elements
        if(layoutElements.Count != elementTypes.Count)
            throw new InvalidOperationException("Not all elements were loaded.");

        Dictionary<string, Element> elements = new();
        foreach((string elementId, LayoutResourceElement layoutElement) in layoutElements) {
            Element element = layoutElement.GetElement(resources, renderer,
                input, audio, colors.colors, layoutName, elementId);
            elements.Add(elementId, element);
        }

        this.elements = elements;
    }

    protected override void DeserializeJson(string path, IDictionary<string, LayoutResourceElement> deserialized) {
        Dictionary<string, JsonElement>? layout =
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(path));
        if(layout is null)
            return;

        foreach((string? elementId, Type? type) in elementTypes) {
            if(!layout.TryGetValue(elementId, out JsonElement jsonElement))
                throw new InvalidOperationException($"Element {elementId} is missing.");
            if(type.BaseType != typeof(LayoutResourceElement))
                throw new InvalidOperationException(
                    $"Element types specs can only inherit from {nameof(LayoutResourceElement)}");
            LayoutResourceElement? layoutElement = (LayoutResourceElement?)jsonElement.Deserialize(type);
            if(layoutElement is null)
                throw new InvalidOperationException($"Failed to deserialize {elementId} as {type.Name}");
            if(!deserialized.ContainsKey(elementId))
                deserialized.Add(elementId, layoutElement);
        }
    }

    public override void Unload(string id, IResources resources) => elements = new Dictionary<string, Element>();

    public abstract void Open();
    public abstract void Close();
    public abstract void Update();
    public abstract void Tick();
}
