﻿using System;

namespace PER.Util;

public class Stopwatch : IReadOnlyStopwatch {
    public double speed {
        get => _speed;
        set {
            long currentTime = DateTime.UtcNow.Ticks;
            _savedTime += (DateTime.UtcNow.Ticks - currentTime) * speed;
            _startTime = currentTime;
            _speed = value;
        }
    }

    public double preciseTicks => _savedTime + (DateTime.UtcNow.Ticks - _startTime) * speed;
    public long ticks => (long)preciseTicks;
    public TimeSpan time => new(ticks);

    private long _startTime;
    private double _speed = 1d;
    private double _savedTime;

    public void Reset(double startTicks = 0d) {
        _savedTime = startTicks;
        _startTime = DateTime.UtcNow.Ticks;
    }
}
