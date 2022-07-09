using System;

using JetBrains.Annotations;

namespace PER.Util;

[PublicAPI]
public interface IReadOnlyStopwatch {
    public double speed { get; }
    public double preciseTicks { get; }
    public long ticks { get; }
    public TimeSpan time { get; }
}
