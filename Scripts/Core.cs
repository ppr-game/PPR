using PPR.Main;

using PRR;

namespace PPR.Scripts {
    public class Core {
        public static string version => PPR.Core.version;
        public static string prrVersion => PPR.Core.prrVersion;
        public static float deltaTime => PPR.Core.deltaTime;
        public static Game game => PPR.Core.game;
        public static Renderer renderer => PPR.Core.renderer;
    }
}
