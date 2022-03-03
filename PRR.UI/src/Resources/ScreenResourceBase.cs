﻿using System.Text.Json;
using System.Text.Json.Serialization;

using PER.Abstractions;
using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Renderer;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Common.Resources;
using PER.Util;

namespace PRR.UI.Resources;

public abstract class ScreenResourceBase : JsonResourceBase<IDictionary<string, LayoutResourceElement>>, IScreen {
    protected class LayoutResourceText : LayoutResourceElement {
        public readonly struct TextFormatting {
            private string? foregroundColor { get; }
            private string? backgroundColor { get; }
            [JsonConverter(typeof(JsonStringEnumConverter))]
            private RenderStyle? style { get; }
            [JsonConverter(typeof(JsonStringEnumConverter))]
            private RenderOptions? options { get; }
            private string? effect { get; }

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

        private string? path { get; }
        private string? text { get; }
        private Dictionary<char, TextFormatting>? formatting { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        private HorizontalAlignment? align { get; }

        public LayoutResourceText(bool? enabled, Vector2Int position, Vector2Int size, string? path, string? text,
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
                        textFormatting.GetFormatting(colors, renderer.formattingEffects));
            if(align.HasValue) element.align = align.Value;
            return element;
        }
    }

    protected class LayoutResourceButton : LayoutResourceElement {
        private string? text { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        private RenderStyle? style { get; }
        private bool? active { get; }
        private bool? toggled { get; }

        public LayoutResourceButton(bool? enabled, Vector2Int position, Vector2Int size, string? text,
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

    protected abstract IRenderer renderer { get; }
    protected abstract IInputManager input { get; }
    protected abstract IAudio audio { get; }

    protected abstract string layoutName { get; }
    protected abstract IReadOnlyDictionary<string, Type> elementTypes { get; }

    protected IReadOnlyDictionary<string, Element> elements { get; private set; } = new Dictionary<string, Element>();

    public override bool Load(string id, IResources resources) {
        if(!resources.TryGetResource(ColorsResource.GlobalId, out ColorsResource? colors)) return false;

        Dictionary<string, LayoutResourceElement> layoutElements = new(elementTypes.Count);
        DeserializeAllJson(resources, Path.Join("layouts", $"{layoutName}.json"), layoutElements,
            () => layoutElements.Count == elementTypes.Count);

        // didn't load all the elements
        if(layoutElements.Count != elementTypes.Count) return false;

        Dictionary<string, Element> elements = new();
        foreach((string elementId, LayoutResourceElement layoutElement) in layoutElements) {
            Element element = layoutElement.GetElement(resources, renderer,
                input, audio, colors!.colors, layoutName, elementId);
            elements.Add(elementId, element);
        }

        this.elements = elements;
        return true;
    }

    protected override bool DeserializeJson(string path, IDictionary<string, LayoutResourceElement> deserialized) {
        Dictionary<string, JsonElement>? layout =
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(path));
        if(layout is null) return false;

        foreach((string? elementId, Type? type) in elementTypes) {
            if(!layout.TryGetValue(elementId, out JsonElement jsonElement) ||
               type.BaseType != typeof(LayoutResourceElement)) return false;
            LayoutResourceElement? layoutElement = (LayoutResourceElement?)jsonElement.Deserialize(type);
            if(layoutElement is null) return false;
            if(!deserialized.ContainsKey(elementId)) deserialized.Add(elementId, layoutElement);
        }

        return true;
    }

    public override bool Unload(string id, IResources resources) {
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