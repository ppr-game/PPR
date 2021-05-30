using System.Collections.ObjectModel;

namespace PPR.UI.Animations {
    public readonly struct AnimationSettings {
        public readonly string id;
        public readonly float time;
        public readonly bool endState;
        public readonly ReadOnlyDictionary<string, double> consts;
        
        public AnimationSettings(string id, float time, bool endState, ReadOnlyDictionary<string, double> consts) {
            this.id = id;
            this.time = time;
            this.endState = endState;
            this.consts = consts;
        }
    }
}
