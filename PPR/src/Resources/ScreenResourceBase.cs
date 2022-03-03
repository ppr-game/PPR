using System.Text.Json;
using System.Text.Json.Serialization;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Util;

using PPR.Screens;

using PRR.UI;

namespace PPR.Resources;

public abstract class ScreenResourceBase : IScreen, IResource {
    protected abstract class LayoutElement {
        public bool? enabled { get; }
        public Vector2Int position { get; }
        public Vector2Int size { get; }

        protected LayoutElement(bool? enabled, Vector2Int position, Vector2Int size) {
            this.enabled = enabled;
            this.position = position;
            this.size = size;
        }

        public abstract Element GetElement(IResources resources, IRenderer renderer, IInputManager input, IAudio audio,
            Dictionary<string, Color> colors, string layoutName, string id);

        protected static bool TryGetColor(Dictionary<string, Color> colors, string type, string layoutName, string id,
            string colorName, out Color color) =>
            colors.TryGetValue($"{type}_{layoutName}.{id}_{colorName}", out color) ||
            colors.TryGetValue($"{type}_@{id}_{colorName}", out color);
    }

    protected class LayoutText : LayoutElement {
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

            public Formatting GetFormatting(Dictionary<string, Color> colors, Dictionary<string, IEffect?> effects,
                string layoutName, string id) {
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

        public LayoutText(bool? enabled, Vector2Int position, Vector2Int size, string? path, string? text,
            Dictionary<char, TextFormatting>? formatting, HorizontalAlignment? align) :
            base(enabled, position, size) {
            this.path = path;
            this.text = text;
            this.formatting = formatting;
            this.align = align;
        }

        public override Element GetElement(IResources resources, IRenderer renderer, IInputManager input, IAudio audio,
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
                        textFormatting.GetFormatting(colors, renderer.formattingEffects, layoutName, id));
            if(align.HasValue) element.align = align.Value;
            return element;
        }
    }

    protected class LayoutButton : LayoutElement {
        public string? text { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RenderStyle? style { get; }
        public bool? active { get; }
        public bool? toggled { get; }

        public LayoutButton(bool? enabled, Vector2Int position, Vector2Int size, string? text,
            RenderStyle? style, bool? active, bool? toggled) : base(enabled, position, size) {
            this.text = text;
            this.style = style;
            this.active = active;
            this.toggled = toggled;
        }

        public override Element GetElement(IResources resources, IRenderer renderer, IInputManager input, IAudio audio,
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
            if(TryGetColor(colors, "button", layoutName, id, "inactive_toggled", out color))
                element.inactiveToggledColor = color;
            if(TryGetColor(colors, "button", layoutName, id, "idle", out color))
                element.idleColor = color;
            if(TryGetColor(colors, "button", layoutName, id, "hover", out color))
                element.hoverColor = color;
            if(TryGetColor(colors, "button", layoutName, id, "click", out color))
                element.clickColor = color;
            return element;
        }
    }

    protected abstract string layoutName { get; }
    protected abstract IReadOnlyDictionary<string, Type> elementTypes { get; }

    protected IReadOnlyDictionary<string, Element> elements { get; private set; } = new Dictionary<string, Element>();

    public bool Load(string id, IResources resources) {
        if(!Core.engine.resources.TryGetResource("graphics/colors", out ColorsResource? colors)) return false;

        Dictionary<string, LayoutElement> layoutElements = new(elementTypes.Count);
        foreach(string path in Core.engine.resources.GetAllPathsReverse(Path.Join("layouts", $"{layoutName}.json"))) {
            if(!DeserializeLayout(path, layoutElements)) return false;
            if(layoutElements.Count == elementTypes.Count) break; // already loaded all the elements
        }

        // didn't load all the elements
        if(layoutElements.Count != elementTypes.Count) return false;

        Dictionary<string, Element> elements = new();
        foreach((string elementId, LayoutElement layoutElement) in layoutElements) {
            Element element = layoutElement.GetElement(resources, Core.engine.renderer,
                Core.engine.input, Core.engine.audio, colors!.colors, layoutName, elementId);
            elements.Add(elementId, element);
        }

        this.elements = elements;
        return true;
    }

    private bool DeserializeLayout(string path, IDictionary<string, LayoutElement> currentValues) {
        Dictionary<string, JsonElement>? layout =
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(path));
        if(layout is null) return false;

        foreach((string? elementId, Type? type) in elementTypes) {
            if(!layout.TryGetValue(elementId, out JsonElement jsonElement) ||
               type.BaseType != typeof(LayoutElement)) return false;
            LayoutElement? layoutElement = (LayoutElement?)jsonElement.Deserialize(type);
            if(layoutElement is null) return false;
            if(!currentValues.ContainsKey(elementId)) currentValues.Add(elementId, layoutElement);
        }

        return true;
    }

    public bool Unload(string id, IResources resources) {
        elements = new Dictionary<string, Element>();
        return true;
    }

    public abstract void Enter();
    public abstract void Quit();
    public abstract bool QuitUpdate();
    public abstract void Open();
    public abstract void Close();
    public abstract void Update();
    public abstract void Tick();
}
