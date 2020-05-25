using System;
using System.Runtime.InteropServices;

using NLog;

using PPR.Core;
using PPR.GUI;
using PPR.Rendering;

using SFML.System;

public static class MainGame {
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool AllocConsole();
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();
    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    const int SW_HIDE = 0;
    const int SW_SHOW = 5;
    public static void ShowConsoleWindow() {
        IntPtr handle = GetConsoleWindow();
        _ = handle == IntPtr.Zero ? AllocConsole() : ShowWindow(handle, SW_SHOW);
    }
    public static void HideConsoleWindow() {
        IntPtr handle = GetConsoleWindow();
        _ = ShowWindow(handle, SW_HIDE);
    }

    public static float deltaTime = 0f;
    public static readonly Renderer renderer = new Renderer(80, 60, 0);
    public static readonly Game game = new Game();

    static void Main() {
        renderer.window.KeyPressed += game.KeyPressed;
        renderer.window.MouseWheelScrolled += game.MouseWheelScrolled;
        renderer.window.LostFocus += game.LostFocus;
        renderer.window.GainedFocus += game.GainedFocus;
        renderer.window.Closed += (_, __) => game.End();

        game.Start();

        logger.Info("Loading finished");

        Clock fpsClock = new Clock();
        while(renderer.window.IsOpen) { // Executes every frame
            renderer.window.DispatchEvents();

            renderer.Update();
            renderer.Draw();

            game.Update();

            renderer.window.Display();

            deltaTime = fpsClock.Restart().AsSeconds();
            UI.fps = (int)MathF.Round(1f / deltaTime);
            if(UI.fps < 30 && renderer.window.HasFocus()) logger.Warn("Lag detected: too low fps ({0})", UI.fps);
        }
    }
}
