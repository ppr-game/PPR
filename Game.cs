using System;
using System.Diagnostics;

using PPR.Core;
using PPR.GUI;
using PPR.Rendering;

using SFML.System;

public static class MainGame {
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

        renderer.window.Closed += (caller, e) => {
            RPC.client.ClearPresence();
            RPC.client.Dispose();
        };

        Clock fpsClock = new Clock();
        while(renderer.window.IsOpen) { // Executes every frame
            renderer.window.DispatchEvents();

            renderer.Update();
            renderer.Draw();

            game.Update();

            renderer.window.Display();

            deltaTime = fpsClock.Restart().AsSeconds();
            UI.fps = (int)MathF.Round(1f / deltaTime);
            if(UI.fps < 30) Debug.WriteLine("Lag detected: too low fps ({0})", UI.fps);
        }
    }
}
