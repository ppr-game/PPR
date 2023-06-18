using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace PER.Abstractions.Environment;

[PublicAPI]
public class Level : IUpdatable, ITickable {
    public List<ILevelObject> objects { get; } = new();

    public void Update(TimeSpan time) {
        foreach(ILevelObject obj in objects)
            obj.Update(time);
    }

    public void Tick(TimeSpan time) {
        foreach(ILevelObject obj in objects)
            obj.Tick(time);
    }
}
