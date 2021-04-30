namespace PPR.LuaConsole.Rendering {
    public class Renderer : Scripts.Rendering.Renderer {
        public static bool keyRepeat {
            set => Core.renderer.window.SetKeyRepeatEnabled(value);
        }
    }
}
