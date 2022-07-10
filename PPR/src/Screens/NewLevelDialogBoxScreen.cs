﻿using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Common.Effects;
using PER.Util;

using PRR.UI;

namespace PPR.Screens;

public class NewLevelDialogBoxScreen : DialogBoxScreenResourceBase {
    public const string GlobalId = "layouts/newLevelDialog";

    public Action? onCancel { get; set; }

    protected override IRenderer renderer => Core.engine.renderer;
    protected override IInput input => Core.engine.input;
    protected override IAudio audio => Core.engine.audio;

    protected override string layoutName => "newLevelDialog";
    protected override IReadOnlyDictionary<string, Type> elementTypes { get; } = new Dictionary<string, Type> {
        { "title", typeof(LayoutResourceText) },
        { "labels", typeof(LayoutResourceText) },
        { "metadata.name", typeof(LayoutResourceInputField) },
        { "metadata.description", typeof(LayoutResourceInputField) },
        { "metadata.author", typeof(LayoutResourceInputField) },
        { "metadata.difficulty", typeof(LayoutResourceSlider) },
        { "metadata.difficulty.text", typeof(LayoutResourceText) },
        { "cancel", typeof(LayoutResourceButton) },
        { "create", typeof(LayoutResourceButton) }
    };

    public NewLevelDialogBoxScreen() : base(new Vector2Int(30, 32)) { }

    public override void Load(string id, IResources resources) {
        base.Load(id, resources);

        if(elements["metadata.author"] is InputField author) author.onTextChange += (_, _) => {
            author.UpdateColors(colors.colors, layoutName, "metadata.author",
                LevelSelectScreen.authorToSpecial.TryGetValue(
                    author.value ?? string.Empty, out string? special) ? special : null);
        };

        if(elements["cancel"] is Button cancel) cancel.onClick += (_, _) => {
            onCancel?.Invoke();
        };

        if(elements["create"] is Button create) create.onClick += (_, _) => { };
    }

    public override void Close() {
        base.Close();
        onCancel = null;
    }

    public override void Tick() { }
}
