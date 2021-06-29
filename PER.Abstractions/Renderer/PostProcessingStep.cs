namespace PER.Abstractions.Renderer {
    public struct PostProcessingStep {
        public enum Type { Text, Screen, TemporaryText, TemporaryScreen, SwapBuffer, ClearBuffer }

        public Type type { get; init; }
        public string vertexShader { get; init; }
        public string fragmentShader { get; init; }
        public BlendMode blendMode { get; init; }
    }
}
