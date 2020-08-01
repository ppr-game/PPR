using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;

using SFML.Window;

namespace PPR.Properties {
    [TypeConverter(typeof(InputKeyConverter))]
    [SettingsSerializeAs(SettingsSerializeAs.String)]
    public class InputKey {
        public string asString {
            get {
                string fullString = "";
                if(mainModifier != null) fullString += mainModifier + "+";
                if(mainKey != null) fullString += mainKey.ToString();
                if(secondaryModifier != null) fullString += "," + secondaryModifier + "+";
                if(secondaryKey != null) fullString += (secondaryModifier == null ? "," : "") + secondaryKey;
                return fullString;
            }
            set {
                string[] mainAndSec = value.Split(',');
                if(value == "") {
                    mainKey = null;
                    mainModifier = null;
                    secondaryKey = null;
                    secondaryModifier = null;
                }
                else {
                    string[] main = mainAndSec[0].Split('+');
                    mainKey = Enum.Parse<Keyboard.Key>(main[main.Length > 1 ? 1 : 0]);
                    mainModifier = main.Length > 1 ? (Keyboard.Key?)Enum.Parse<Keyboard.Key>(main[0]) : null;
                    if(mainAndSec.Length > 1) {
                        string[] sec = mainAndSec[1].Split('+');
                        secondaryKey = Enum.Parse<Keyboard.Key>(sec[sec.Length > 1 ? 1 : 0]);
                        secondaryModifier = sec.Length > 1 ? (Keyboard.Key?)Enum.Parse<Keyboard.Key>(sec[0]) : null;
                    }
                    else {
                        secondaryKey = null;
                        secondaryModifier = null;
                    }
                }
            }
        }
        public Keyboard.Key? mainModifier { get; private set; }
        public Keyboard.Key? mainKey { get; private set; }
        public Keyboard.Key? secondaryModifier { get; private set; }
        public Keyboard.Key? secondaryKey { get; private set; }
        public InputKey() { }
        public InputKey(string fromString) {
            asString = fromString;
        }
        public bool IsPressed(Keyboard.Key key) {
            return mainKey == key || secondaryKey == key;
        }
        public bool IsPressed(Keyboard.Key modifier, Keyboard.Key key) {
            return (mainModifier == modifier && mainKey == key) || (secondaryModifier == modifier && secondaryKey == key);
        }
        public bool IsPressed(KeyEventArgs key) {
            if(mainKey == key.Code) {
                bool modifierIsAlt = mainModifier == Keyboard.Key.LAlt || mainModifier == Keyboard.Key.RAlt;
                bool modifierIsCtrl = mainModifier == Keyboard.Key.LControl || mainModifier == Keyboard.Key.RControl;
                bool modifierIsShift = mainModifier == Keyboard.Key.LShift || mainModifier == Keyboard.Key.RShift;
                bool modifierIsSys = mainModifier == Keyboard.Key.LSystem || mainModifier == Keyboard.Key.RSystem;
                bool anyModifier = modifierIsAlt || modifierIsCtrl || modifierIsShift || modifierIsSys;
                return !anyModifier ||
                    (modifierIsAlt && key.Alt) || (modifierIsCtrl && key.Control) || (modifierIsShift && key.Shift) || (modifierIsSys && key.System);
            }
            if(secondaryKey == key.Code) {
                bool modifierIsAlt = secondaryModifier == Keyboard.Key.LAlt || secondaryModifier == Keyboard.Key.RAlt;
                bool modifierIsCtrl = secondaryModifier == Keyboard.Key.LControl || secondaryModifier == Keyboard.Key.RControl;
                bool modifierIsShift = secondaryModifier == Keyboard.Key.LShift || secondaryModifier == Keyboard.Key.RShift;
                bool modifierIsSys = secondaryModifier == Keyboard.Key.LSystem || secondaryModifier == Keyboard.Key.RSystem;
                bool anyModifier = modifierIsAlt || modifierIsCtrl || modifierIsShift || modifierIsSys;
                return !anyModifier ||
                    (modifierIsAlt && key.Alt) || (modifierIsCtrl && key.Control) || (modifierIsShift && key.Shift) || (modifierIsSys && key.System);
            }
            return false;
        }
    }
    public class InputKeyConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            return value is string @string ? new InputKey(@string) : base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            return destinationType == typeof(string) ? (value as InputKey).asString : base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
