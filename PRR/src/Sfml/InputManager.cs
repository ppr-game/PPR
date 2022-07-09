using System;
using System.Collections.Generic;

using PER.Abstractions.Input;
using PER.Util;

using SFML.Window;

namespace PRR.Sfml;

public class InputManager : IInput {
    public bool block { get; set; }

    public Vector2Int mousePosition => block ? new Vector2Int(-1, -1) : _mousePosition;
    public Vector2 accurateMousePosition => block ? new Vector2(-1f, -1f) : _accurateMousePosition;
    public Vector2 normalizedMousePosition => block ? new Vector2(-1f, -1f) : _normalizedMousePosition;

    public Vector2Int previousMousePosition => block ? new Vector2Int(-1, -1) : _previousMousePosition;
    public Vector2 previousAccurateMousePosition => block ? new Vector2(-1f, -1f) : _previousAccurateMousePosition;
    public Vector2 previousNormalizedMousePosition => block ? new Vector2(-1f, -1f) : _previousNormalizedMousePosition;

    public bool keyRepeat {
        get => _keyRepeat;
        set {
            if(_renderer.window is null)
                return;
            _keyRepeat = value;
            _renderer.window.SetKeyRepeatEnabled(value);
        }
    }

    public string clipboard {
        get => Clipboard.Contents;
        set => Clipboard.Contents = value;
    }

    public event EventHandler<IInput.KeyDownEventArgs>? keyDown;
    public event EventHandler<IInput.TextEnteredEventArgs>? textEntered;
    public event EventHandler<IInput.ScrolledEventArgs>? scrolled;

    private Vector2Int _mousePosition = new(-1, -1);
    private Vector2 _accurateMousePosition = new(-1f, -1f);
    private Vector2 _normalizedMousePosition = new(-1f, -1f);
    private Vector2Int _previousMousePosition = new(-1, -1);
    private Vector2 _previousAccurateMousePosition = new(-1f, -1f);
    private Vector2 _previousNormalizedMousePosition = new(-1f, -1f);

    private readonly HashSet<KeyCode> _pressedKeys = new();
    private readonly HashSet<MouseButton> _pressedMouseButtons = new();
    private bool _keyRepeat;

    private readonly Renderer _renderer;

    public InputManager(Renderer renderer) => _renderer = renderer;

    public void Reset() {
        if(_renderer.window is null)
            return;

        _renderer.window.SetKeyRepeatEnabled(false);

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

    public void Update() {
        _previousMousePosition = _mousePosition;
        _previousAccurateMousePosition = _accurateMousePosition;
        _previousNormalizedMousePosition = _normalizedMousePosition;
    }

    public void Finish() { }

    public bool KeyPressed(KeyCode key) => !block && _pressedKeys.Contains(key);

    public bool KeysPressed(KeyCode key1, KeyCode key2) => !block && KeyPressed(key1) && KeyPressed(key2);

    public bool KeysPressed(KeyCode key1, KeyCode key2, KeyCode key3) =>
        !block && KeyPressed(key1) && KeyPressed(key2) && KeyPressed(key3);

    public bool KeysPressed(KeyCode key1, KeyCode key2, KeyCode key3, KeyCode key4) =>
        !block && KeyPressed(key1) && KeyPressed(key2) && KeyPressed(key3) && KeyPressed(key4);

    public bool KeysPressed(params KeyCode[] keys) {
        if(block)
            return false;

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach(KeyCode key in keys)
            if(!KeyPressed(key))
                return false;

        return true;
    }

    public bool MouseButtonPressed(MouseButton button) => !block && _pressedMouseButtons.Contains(button);

    private void UpdateKeyPressed(KeyCode key, bool pressed) {
        if(pressed) {
            _pressedKeys.Add(key);
            keyDown?.Invoke(this, new IInput.KeyDownEventArgs(key));
        }
        else _pressedKeys.Remove(key);
    }

    private void EnterText(string text) => textEntered?.Invoke(this, new IInput.TextEnteredEventArgs(text));

    private void UpdateMouseButtonPressed(MouseButton button, bool pressed) {
        if(pressed) _pressedMouseButtons.Add(button);
        else _pressedMouseButtons.Remove(button);
    }

    private void UpdateMousePosition(int mouseX, int mouseY) {
        if(!_renderer.focused) {
            _mousePosition = new Vector2Int(-1, -1);
            _accurateMousePosition = new Vector2(-1f, -1f);
            _normalizedMousePosition = new Vector2(-1f, -1f);
            return;
        }

        Vector2 pixelMousePosition = new(
            mouseX - _renderer.window?.Size.X * 0.5f + _renderer.text?.imageWidth * 0.5f ?? 0f,
            mouseY - _renderer.window?.Size.Y * 0.5f + _renderer.text?.imageHeight * 0.5f ?? 0f);
        _accurateMousePosition = new Vector2(
            pixelMousePosition.x / _renderer.font?.size.x ?? 0,
            pixelMousePosition.y / _renderer.font?.size.y ?? 0);
        _mousePosition = new Vector2Int((int)_accurateMousePosition.x, (int)_accurateMousePosition.y);
        _normalizedMousePosition =
            new Vector2(pixelMousePosition.x / ((_renderer.text?.imageWidth ?? 0) - 1),
                pixelMousePosition.y / ((_renderer.text?.imageHeight ?? 0) - 1));
    }

    private void ScrollMouse(float delta) => scrolled?.Invoke(this, new IInput.ScrolledEventArgs(delta));
}
