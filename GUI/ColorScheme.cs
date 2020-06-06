using System.IO;
using System.Linq;

using PPR.Properties;

using SFML.Graphics;

namespace PPR.GUI {
    public static class ColorScheme {
        public static Color black;
        public static Color white;
        public static Color gray;
        public static Color red;
        public static Color green;
        public static Color blue;
        public static Color cyan;
        public static Color yellow;
        public static Color orange;
        public static Color lightDarkGray;
        public static Color lightDarkRed;
        public static Color lightDarkGreen;
        public static Color lightDarkBlue;
        public static Color lightDarkCyan;
        public static Color lightDarkYellow;
        public static Color lightDarkOrange;
        public static Color darkGray;
        public static Color darkRed;
        public static Color darkGreen;
        public static Color darkCyan;
        public static Color darkBlue;
        public static Color darkYellow;
        public static Color darkOrange;
        public static void Reload() {
            string filePath = Path.Combine("resources", "colors", Settings.Default.colorScheme, "colors.txt");
            string[] lines = File.ReadAllLines(filePath);
            foreach(string line in lines) {
                string[] keyValue = line.Split('=');
                string key = keyValue[0];
                string value = keyValue[1];
                byte[] values = value.Split(',').Select(val => byte.Parse(val)).ToArray();
                Color color = new Color(values[0], values[1], values[2]);
                switch(key) {
                    case "black":
                        black = color;
                        break;
                    case "white":
                        white = color;
                        break;
                    case "gray":
                        gray = color;
                        break;
                    case "red":
                        red = color;
                        break;
                    case "green":
                        green = color;
                        break;
                    case "blue":
                        blue = color;
                        break;
                    case "cyan":
                        cyan = color;
                        break;
                    case "yellow":
                        yellow = color;
                        break;
                    case "orange":
                        orange = color;
                        break;
                    case "light_dark_gray":
                        lightDarkGray = color;
                        break;
                    case "light_dark_red":
                        lightDarkRed = color;
                        break;
                    case "light_dark_green":
                        lightDarkGreen = color;
                        break;
                    case "light_dark_blue":
                        lightDarkBlue = color;
                        break;
                    case "light_dark_cyan":
                        lightDarkCyan = color;
                        break;
                    case "light_dark_yellow":
                        lightDarkYellow = color;
                        break;
                    case "light_dark_orange":
                        lightDarkOrange = color;
                        break;
                    case "dark_gray":
                        darkGray = color;
                        break;
                    case "dark_red":
                        darkRed = color;
                        break;
                    case "dark_green":
                        darkGreen = color;
                        break;
                    case "dark_blue":
                        darkBlue = color;
                        break;
                    case "dark_cyan":
                        darkCyan = color;
                        break;
                    case "dark_yellow":
                        darkYellow = color;
                        break;
                    case "dark_orange":
                        darkOrange = color;
                        break;
                }
            }

            Core.game.ReloadSettings();
        }
    }
}
