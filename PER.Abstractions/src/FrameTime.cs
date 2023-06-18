using System;

namespace PER.Abstractions;

public class FrameTime : IUpdatable {
    public TimeSpan frameTime { get; private set; }
    public TimeSpan averageFrameTime { get; private set; }

    public double fps => 1d / frameTime.TotalSeconds;
    public double averageFps => 1d / averageFrameTime.TotalSeconds;

    private TimeSpan _prevTime;
    private TimeSpan _lastAvgFrameTimeUpdate;
    private int _tempAvgFrameTimeCounter;

    public void Update(TimeSpan time) {
        frameTime = time - _prevTime;

        TimeSpan avgUpdateDeltaTime = time - _lastAvgFrameTimeUpdate;

        _tempAvgFrameTimeCounter++;

        // update average frame time about every second
        if(avgUpdateDeltaTime.TotalSeconds >= 1d) {
            averageFrameTime = avgUpdateDeltaTime / _tempAvgFrameTimeCounter;
            _lastAvgFrameTimeUpdate = time;
            _tempAvgFrameTimeCounter = 0;
        }
        _prevTime = time;
    }
}
