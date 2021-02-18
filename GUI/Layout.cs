using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

using MoonSharp.Interpreter;

using PPR.GUI.Elements;

namespace PPR.GUI {
    public class Layout {
        public ConcurrentDictionary<string, UIElement> elements { get; }
        public Script script { get; }
        
        public Layout(ConcurrentDictionary<string, UIElement> elements, Script script) {
            this.elements = elements;
            this.script = script;

            foreach((string _, UIElement element) in elements) RegisterElementEvents(element, script);
        }

        public void RegisterElementEvents(string uid) {
            if(elements.TryGetValue(uid, out UIElement element)) RegisterElementEvents(element, script);
            else throw new ArgumentException($"The element with UID {uid} doesn't exist.");
        }

        public static void RegisterElementEvents(UIElement element, Script script) {
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

        public bool IsElementEnabled(string id) => elements.TryGetValue(id, out UIElement game) && game.enabled;
    }
}
