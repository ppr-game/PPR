using System;

using PPR.Core;
using PPR.GUI;
using PPR.Rendering;

using SFML.System;

public static class MainGame {
    public static float deltaTime = 0f;

    static void Main() {
        Renderer renderer = new Renderer(80, 60, 0);
        Game game = new Game();
        renderer.window.KeyPressed += game.KeyPressed;
        renderer.window.MouseWheelScrolled += game.MouseWheelScrolled;
        renderer.window.LostFocus += game.LostFocus;
        renderer.window.GainedFocus += game.GainedFocus;

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
        }
    }
}
