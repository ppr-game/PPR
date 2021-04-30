using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

namespace PPR.GUI.Elements {
    public struct DeserializedUIElement {
        public string type;
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool enabled;
        public List<string> tags;
        public Dictionary<string, int> position;
        public Dictionary<string, int> size;
        public Dictionary<string, float> anchor;
        public string parent;
        public string path;
        public string text;
        public string align;
        public bool replacingSpaces;
        public bool invertOnDarkBackground;
        public int width;
        public int minValue;
        public int maxValue;
        public int defaultValue;
        public string leftText;
        public string rightText;
        public bool swapTexts;
    }
}
