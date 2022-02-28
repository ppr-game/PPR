using DiscordRPC;

using NLog;

namespace PPROld.Main {
    public static class RichPresence {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static DiscordRpcClient client;
        public static void Initialize() {
            client = new DiscordRpcClient("699266677698723941");
            client.OnError += (sender, e) => logger.Error(e.Message);
            _ = client.Initialize();
            SetPresence("In main menu");
        }
        public static void SetPresence(string details = "", string state = "", string largeImageText = "", Timestamps timestamps = null) => client.SetPresence(new DiscordRPC.RichPresence {
            Details = details,
            State = state,
            Assets = new Assets {
                LargeImageKey = "icon",
                LargeImageText = Core.version
            },
            Timestamps = timestamps ?? Timestamps.Now
        });
    }
}
