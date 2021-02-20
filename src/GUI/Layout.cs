using System.Collections.Generic;

using MoonSharp.Interpreter;

using PPR.GUI.Elements;

namespace PPR.GUI {
    public class Layout {
        public IReadOnlyDictionary<string, UIElement> elements => _elements;

        private readonly Dictionary<string, UIElement> _elements = new Dictionary<string, UIElement>();
        private readonly Dictionary<string, int> _elementIndices = new Dictionary<string, int>();
        private readonly Dictionary<int, string> _elementKeys = new Dictionary<int, string>();

        public void AddElement(string id, UIElement element) {
            if(_elements.ContainsKey(id)) return;
            
            _elements.Add(id, element);
            _elementIndices.Add(id, _elementIndices.Count);
            _elementKeys.Add(_elementKeys.Count, id);
        }

        public void RemoveElement(string id) {
            if(!_elements.ContainsKey(id)) return;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach((string _, UIElement element) in _elements)
                if(element.parent?.id == id)
                    RemoveElement(element.id);

            _elements.Remove(id);
            _elementKeys.Remove(_elementIndices[id]);
            _elementIndices.Remove(id);
        }

        public void RemoveElement(int index) {
            if(!_elementKeys.ContainsKey(index)) return;
            
            string id = _elementKeys[index];
            RemoveElement(id);
        }

        public UIElement GetElement(string id) => _elements[id];

        public UIElement GetElement(int index) => GetElement(_elementKeys[index]);

        public bool IsElementEnabled(string id) => _elements.TryGetValue(id, out UIElement game) && game.enabled;
    }
}
