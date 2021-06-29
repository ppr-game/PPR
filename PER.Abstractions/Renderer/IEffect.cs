using System.Collections.Generic;

using PER.Util;

namespace PER.Abstractions.Renderer {
    public interface IEffect {
        public IEnumerable<PostProcessingStep> postProcessing { get; }
        public bool ended { get; }
    }
}
