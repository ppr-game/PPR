using System;

namespace PER.Abstractions;

public interface ITickable {
    public void Tick(TimeSpan time);
}
