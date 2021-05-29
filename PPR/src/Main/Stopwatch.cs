using System;

namespace PPR.Main {
    public class Stopwatch : IReadOnlyStopwatch {
        public double speed {
            get => _speed;
            set {
                _savedTime += (DateTime.UtcNow.Ticks - _startTime) * speed;
                _startTime = DateTime.UtcNow.Ticks;
                _speed = value;
            }
        }

        public double preciseTicks => _savedTime + (DateTime.UtcNow.Ticks - _startTime) * speed;
        public long ticks => (long)preciseTicks;
        public TimeSpan time => new TimeSpan(ticks);

        private long _startTime;
        private double _speed = 1d;
        private double _savedTime;

        public void Reset(double startTicks = 0d) {
            _savedTime = startTicks;
            _startTime = DateTime.UtcNow.Ticks;
        }
    }
}
