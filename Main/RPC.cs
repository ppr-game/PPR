using DiscordRPC;

using NLog;

namespace PPR.Main {
    public static class RPC {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static DiscordRpcClient client;
        public static void Initialize() {
            client = new DiscordRpcClient("699266677698723941");
            client.OnError += (sender, e) => logger.Error(e.Message);
            _ = client.Initialize();
            SetPresence("In main menu");
        }
        public static void SetPresence(string details = "", string state = "", string largeImageText = "") => client.SetPresence(new RichPresence {
            Details = details,
            State = state,
            Assets = new Assets {
                LargeImageKey = "icon",
                LargeImageText = Core.version
            },
            Timestamps = Timestamps.Now
        });
    }
}
