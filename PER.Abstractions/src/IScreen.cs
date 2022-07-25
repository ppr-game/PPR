using System;

using JetBrains.Annotations;

namespace PER.Abstractions;

[PublicAPI]
public interface IScreen {
    public void Open();
    public void Close();
    public void Update(TimeSpan time);
    public void Tick(TimeSpan time);
}
