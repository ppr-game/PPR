using System;

using PER.Util;

namespace PER.Abstractions.Input;

public interface IInput {
    public sealed class KeyDownEventArgs : EventArgs {
        public KeyCode key { get; }
        public KeyDownEventArgs(KeyCode key) => this.key = key;
    }

    public sealed class TextEnteredEventArgs : EventArgs {
        public string text { get; }
        public TextEnteredEventArgs(string text) => this.text = text;
    }

    public sealed class ScrolledEventArgs : EventArgs {
        public float delta { get; }
        public ScrolledEventArgs(float delta) => this.delta = delta;
    }

    public Vector2Int mousePosition { get; }
    public Vector2 accurateMousePosition { get; }
    public Vector2 normalizedMousePosition { get; }

    public Vector2Int previousMousePosition { get; }
    public Vector2 previousAccurateMousePosition { get; }
    public Vector2 previousNormalizedMousePosition { get; }

    public bool keyRepeat { get; set; }

    public string clipboard { get; set; }

    public event EventHandler<KeyDownEventArgs>? keyDown;
    public event EventHandler<TextEnteredEventArgs>? textEntered;
    public event EventHandler<IInput.ScrolledEventArgs>? scrolled;

    public void Reset();
    public void Update();
    public void Finish();

    public bool KeyPressed(KeyCode key);
    public bool KeysPressed(KeyCode key1, KeyCode key2);
    public bool KeysPressed(KeyCode key1, KeyCode key2, KeyCode key3);
    public bool KeysPressed(KeyCode key1, KeyCode key2, KeyCode key3, KeyCode key4);
    public bool KeysPressed(params KeyCode[] keys);

    public bool MouseButtonPressed(MouseButton button);
}
