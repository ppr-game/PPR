using System.Text;

using PPROld.UI;

using PRR.Sfml;

namespace PPROld.Main;

public static class StringExtensions {
    public static string AddSpaces(this string text) {
        if(string.IsNullOrWhiteSpace(text))
            return "";
        StringBuilder newText = new StringBuilder(text.Length * 2);
        _ = newText.Append(text[0]);
        for(int i = 1; i < text.Length; i++) {
            if(char.IsUpper(text[i]) && text[i - 1] != ' ')
                _ = newText.Append(' ');
            _ = newText.Append(text[i]);
        }
        return newText.ToString();
    }
}

public static class RendererExtensions {
    public static void UpdateFramerateSetting(this Renderer renderer) {
        int setting = Settings.GetInt("fpsLimit");
        renderer.framerate = renderer.focused ? setting < 60 ? -1 : setting > 960 ? 0 : setting : 30;
    }

    public static void ResetBackground(this Renderer renderer) =>
        renderer.background = SfmlConverters.ToPerColor(ColorScheme.GetColor("background"));
}
