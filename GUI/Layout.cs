using System;
using System.Collections.Generic;
using System.Text;

using MoonSharp.Interpreter;

using PPR.GUI.Elements;

namespace PPR.GUI {
    public class Layout {
        public IReadOnlyDictionary<string, UIElement> elements => _elements;
        public Script script { get; }

        private readonly Dictionary<string, UIElement> _elements = new Dictionary<string, UIElement>();
        private readonly Dictionary<string, int> _elementIndices = new Dictionary<string, int>();
        private readonly Dictionary<int, string> _elementKeys = new Dictionary<int, string>();
        
        public Layout(Script script) => this.script = script;

        public void AddElement(string id, UIElement element) {
            if(_elements.ContainsKey(id)) return;
            
            _elements.Add(id, element);
            _elementIndices.Add(id, _elementIndices.Count);
            _elementKeys.Add(_elementKeys.Count, id);
            RegisterElementEvents(element, script);
        }

        public void RemoveElement(string id) {
            if(!_elements.ContainsKey(id)) return;
            
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

        private static void RegisterElementEvents(UIElement element, Script script) {
            string safeId = element.id.Replace('.', '_');

            switch(element) {
                case Button button: {
                    button.onClick = GetElementEventClosures(script, button.type, safeId, button.tags, "onClick");
                    button.onHover = GetElementEventClosures(script, button.type, safeId, button.tags, "onHover");
                    break;
                }
                case Slider slider: {
                    slider.onValueChange =
                        GetElementEventClosures(script, slider.type, safeId, slider.tags, "onValueChange");
                    break;
                }
                case Mask mask: {
                    mask.onScroll = GetElementEventClosures(script, mask.type, safeId, mask.tags, "onScroll");
                    break;
                }
            }
        }

        private static List<Closure> GetElementEventClosures(Script script, string type, string safeId,
            IReadOnlyCollection<string> tags, string eventName) {
            List<Closure> closures = new List<Closure>(tags.Count + 1);
            
            StringBuilder idEventName = new StringBuilder();
            idEventName.Append(type);
            idEventName.Append('_');
            idEventName.Append(safeId);
            idEventName.Append('_');
            idEventName.Append(eventName);

            closures.Add(script.Globals.Get(idEventName.ToString()).Function);

            foreach(string tag in tags) {
                string safeTag = tag.Replace('.', '_');
                
                StringBuilder tagEventName = new StringBuilder();
                tagEventName.Append(type);
                tagEventName.Append('_');
                tagEventName.Append(safeTag);
                tagEventName.Append('_');
                tagEventName.Append(eventName);

                closures.Add(script.Globals.Get(tagEventName.ToString()).Function);
            }

            return closures;
        }

        public bool IsElementEnabled(string id) => _elements.TryGetValue(id, out UIElement game) && game.enabled;
    }
}
