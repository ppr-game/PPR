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
                    StringBuilder onClickName = new StringBuilder();
                    onClickName.Append(button.type);
                    onClickName.Append('_');
                    onClickName.Append(safeId);
                    onClickName.Append('_');
                    onClickName.Append("onClick");

                    button.onClick = script.Globals.Get(onClickName.ToString()).Function;
                    
                    StringBuilder onHoverName = new StringBuilder();
                    onHoverName.Append(button.type);
                    onHoverName.Append('_');
                    onHoverName.Append(safeId);
                    onHoverName.Append('_');
                    onHoverName.Append("onHover");

                    button.onHover = script.Globals.Get(onHoverName.ToString()).Function;
                    break;
                }
                case Slider slider: {
                    StringBuilder onValueChangeName = new StringBuilder();
                    onValueChangeName.Append(slider.type);
                    onValueChangeName.Append('_');
                    onValueChangeName.Append(safeId);
                    onValueChangeName.Append('_');
                    onValueChangeName.Append("onValueChange");

                    slider.onValueChange = script.Globals.Get(onValueChangeName.ToString()).Function;
                    break;
                }
            }
        }

        public bool IsElementEnabled(string id) => elements.TryGetValue(id, out UIElement game) && game.enabled;
    }
}
