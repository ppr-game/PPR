using System;

namespace PER.Abstractions;

public interface IUpdatable {
    public void Update(TimeSpan time);
}
