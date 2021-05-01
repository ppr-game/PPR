using System.Collections.Generic;

using MoonSharp.Interpreter;

using PPR.UI.Elements;

namespace PPR.UI {
    public class Layout {
        public IReadOnlyDictionary<string, Element> elements => _elements;
        public Script script;

        private readonly Dictionary<string, Element> _elements = new Dictionary<string, Element>();
        private readonly List<string> _elementKeys = new List<string>();

        public Layout(Script script) => this.script = script;

        public void AddElement(string id, Element element) {
            if(_elements.ContainsKey(id)) return;
            
            _elements.Add(id, element);
            _elementKeys.Add(id);
        }

        public void RemoveElement(string id) {
            if(!_elements.ContainsKey(id)) return;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach((string _, Element element) in _elements)
                if(element.parent?.id == id)
                    RemoveElement(element.id);

            _elements.Remove(id);
            _elementKeys.Remove(id);
        }

        public void RemoveElement(int index) {
            if(_elementKeys.Count <= index) return;
            
            string id = _elementKeys[index];
            RemoveElement(id);
        }

        public Element GetElement(string id) => _elements[id];

        public Element GetElement(int index) => GetElement(_elementKeys[index]);

        public bool IsElementEnabled(string id) => _elements.TryGetValue(id, out Element game) && game.enabled;
    }
}
