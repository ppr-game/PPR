using System;

namespace PPR.Main {
    public interface IReadOnlyStopwatch {
        public double speed { get; }
        public double preciseTicks { get; }
        public long ticks { get; }
        public TimeSpan time { get; }
    }
}
