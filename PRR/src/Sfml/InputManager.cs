using System;
using System.Collections.Generic;

using PER.Abstractions.Input;
using PER.Util;

namespace PRR.Sfml {
    public class InputManager : IInputManager {
        public Vector2Int mousePosition { get; private set; } = new(-1, -1);
        public Vector2 accurateMousePosition { get; private set; } = new(-1f, -1f);
        public Vector2 normalizedMousePosition { get; private set; } = new(-1f, -1f);

        public event EventHandler<IInputManager.TextEnteredEventArgs> textEntered;
        public event EventHandler<IInputManager.ScrolledEventArgs> scrolled;

        public Renderer renderer { private get; init; }

        private readonly HashSet<KeyCode> _pressedKeys = new();
        private readonly HashSet<MouseButton> _pressedMouseButtons = new();

        public void Setup() {
            renderer.window.KeyPressed += (_, key) => UpdateKeyPressed(SfmlConverters.ToPerKey(key.Code), true);
            renderer.window.KeyReleased += (_, key) => UpdateKeyPressed(SfmlConverters.ToPerKey(key.Code), false);
            renderer.window.TextEntered += (_, text) => EnterText(text.Unicode);

            renderer.window.MouseButtonPressed += (_, button) =>
                UpdateMouseButtonPressed(SfmlConverters.ToPerMouseButton(button.Button), true);
            renderer.window.MouseButtonReleased += (_, button) =>
                UpdateMouseButtonPressed(SfmlConverters.ToPerMouseButton(button.Button), false);

            renderer.window.MouseMoved += (_, mouse) => UpdateMousePosition(mouse.X, mouse.Y);
            renderer.window.MouseWheelScrolled += (_, scroll) => ScrollMouse(scroll.Delta);
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

        private void EnterText(string text) => textEntered?.Invoke(this, new IInputManager.TextEnteredEventArgs(text));

        private void UpdateMouseButtonPressed(MouseButton button, bool pressed) {
            if(pressed) _pressedMouseButtons.Add(button);
            else _pressedMouseButtons.Remove(button);
        }

        private void UpdateMousePosition(int mouseX, int mouseY) {
            if(!renderer.window.HasFocus()) {
                mousePosition = new Vector2Int(-1, -1);
                accurateMousePosition = new Vector2(-1f, -1f);
                normalizedMousePosition = new Vector2(-1f, -1f);
                return;
            }

            Vector2 pixelMousePosition = new(
                mouseX - renderer.window.Size.X * 0.5f + renderer.text.imageWidth * 0.5f,
                mouseY - renderer.window.Size.Y * 0.5f + renderer.text.imageHeight * 0.5f);
            accurateMousePosition = new Vector2(
                pixelMousePosition.x / renderer.font.size.x,
                pixelMousePosition.y / renderer.font.size.y);
            mousePosition = new Vector2Int((int)accurateMousePosition.x, (int)accurateMousePosition.y);
            normalizedMousePosition =
                new Vector2(pixelMousePosition.x / (renderer.text.imageWidth - 1),
                    pixelMousePosition.y / (renderer.text.imageHeight - 1));
        }

        private void ScrollMouse(float delta) => scrolled?.Invoke(this, new IInputManager.ScrolledEventArgs(delta));
    }
}
