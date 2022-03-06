using System;
using System.Collections.Generic;

using PER.Abstractions.Input;
using PER.Util;

namespace PRR.Sfml;

public class InputManager : IInput {
    public Vector2Int mousePosition { get; private set; } = new(-1, -1);
    public Vector2 accurateMousePosition { get; private set; } = new(-1f, -1f);
    public Vector2 normalizedMousePosition { get; private set; } = new(-1f, -1f);

    public Vector2Int previousMousePosition { get; private set; } = new(-1, -1);
    public Vector2 previousAccurateMousePosition { get; private set; } = new(-1, -1);
    public Vector2 previousNormalizedMousePosition { get; private set; } = new(-1, -1);

    public event EventHandler<IInput.TextEnteredEventArgs>? textEntered;
    public event EventHandler<IInput.ScrolledEventArgs>? scrolled;

    private readonly HashSet<KeyCode> _pressedKeys = new();
    private readonly HashSet<MouseButton> _pressedMouseButtons = new();

    private readonly Renderer _renderer;

    public InputManager(Renderer renderer) => _renderer = renderer;

    public void Reset() {
        if(_renderer.window is null) return;

        _renderer.window.KeyPressed += (_, key) => UpdateKeyPressed(SfmlConverters.ToPerKey(key.Code), true);
        _renderer.window.KeyReleased += (_, key) => UpdateKeyPressed(SfmlConverters.ToPerKey(key.Code), false);
        _renderer.window.TextEntered += (_, text) => EnterText(text.Unicode);

        _renderer.window.MouseButtonPressed += (_, button) =>
            UpdateMouseButtonPressed(SfmlConverters.ToPerMouseButton(button.Button), true);
        _renderer.window.MouseButtonReleased += (_, button) =>
            UpdateMouseButtonPressed(SfmlConverters.ToPerMouseButton(button.Button), false);

        _renderer.window.MouseMoved += (_, mouse) => UpdateMousePosition(mouse.X, mouse.Y);
        _renderer.window.MouseWheelScrolled += (_, scroll) => ScrollMouse(scroll.Delta);
    }

    public void Update() { }
    public void Finish() { }

    public bool KeyPressed(KeyCode key) => _pressedKeys.Contains(key);

    public bool KeysPressed(KeyCode key1, KeyCode key2) => KeyPressed(key1) && KeyPressed(key2);

    public bool KeysPressed(KeyCode key1, KeyCode key2, KeyCode key3) =>
        KeyPressed(key1) && KeyPressed(key2) && KeyPressed(key3);

    public bool KeysPressed(KeyCode key1, KeyCode key2, KeyCode key3, KeyCode key4) =>
        KeyPressed(key1) && KeyPressed(key2) && KeyPressed(key3) && KeyPressed(key4);

    public bool KeysPressed(params KeyCode[] keys) {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach(KeyCode key in keys)
            if(!KeyPressed(key))
                return false;

        return true;
    }

    public bool MouseButtonPressed(MouseButton button) => _pressedMouseButtons.Contains(button);

    private void UpdateKeyPressed(KeyCode key, bool pressed) {
        if(pressed) _pressedKeys.Add(key);
        else _pressedKeys.Remove(key);
    }

    private void EnterText(string text) => textEntered?.Invoke(this, new IInput.TextEnteredEventArgs(text));

    private void UpdateMouseButtonPressed(MouseButton button, bool pressed) {
        if(pressed) _pressedMouseButtons.Add(button);
        else _pressedMouseButtons.Remove(button);
    }

    private void UpdateMousePosition(int mouseX, int mouseY) {
        if(!_renderer.focused) {
            previousMousePosition = new Vector2Int(-1, -1);
            previousAccurateMousePosition = new Vector2(-1f, -1f);
            previousNormalizedMousePosition = new Vector2(-1f, -1f);

            mousePosition = new Vector2Int(-1, -1);
            accurateMousePosition = new Vector2(-1f, -1f);
            normalizedMousePosition = new Vector2(-1f, -1f);
            return;
        }

        previousMousePosition = mousePosition;
        previousAccurateMousePosition = accurateMousePosition;
        previousNormalizedMousePosition = normalizedMousePosition;

        Vector2 pixelMousePosition = new(
            mouseX - _renderer.window?.Size.X * 0.5f + _renderer.text?.imageWidth * 0.5f ?? 0f,
            mouseY - _renderer.window?.Size.Y * 0.5f + _renderer.text?.imageHeight * 0.5f ?? 0f);
        accurateMousePosition = new Vector2(
            pixelMousePosition.x / _renderer.font?.size.x ?? 0,
            pixelMousePosition.y / _renderer.font?.size.y ?? 0);
        mousePosition = new Vector2Int((int)accurateMousePosition.x, (int)accurateMousePosition.y);
        normalizedMousePosition =
            new Vector2(pixelMousePosition.x / ((_renderer.text?.imageWidth ?? 0) - 1),
                pixelMousePosition.y / ((_renderer.text?.imageHeight ?? 0) - 1));
    }

    private void ScrollMouse(float delta) => scrolled?.Invoke(this, new IInput.ScrolledEventArgs(delta));
}
