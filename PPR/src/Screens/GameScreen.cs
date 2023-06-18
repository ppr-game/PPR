using PER.Abstractions;

using PRR.UI.Resources;

namespace PPR.Screens;

public abstract class GameScreen : LayoutResource, IScreen {
    public void LoadLevel(LevelSerializer.LevelItem levelItem, bool resourcesNeedReload) {
        if(levelItem.hasErrors)
            return;

    }

    public abstract void Open();
    public abstract void Close();
    public abstract void Update(TimeSpan time);
    public abstract void Tick(TimeSpan time);
}
