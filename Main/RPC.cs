using DiscordRPC;

using NLog;

namespace PPR.Main {
    public static class RPC {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static DiscordRpcClient client;
        public static void Initialize() {
            client = new DiscordRpcClient("699266677698723941");
            client.OnError += (sender, e) => logger.Error(e.Message);
            _ = client.Initialize();
            client.SetPresence(new RichPresence {
                Details = "In main menu",
                Timestamps = Timestamps.Now
            });
        }
    }
}
