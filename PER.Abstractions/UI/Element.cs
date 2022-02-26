using PER.Abstractions.Renderer;
using PER.Util;

namespace PER.Abstractions.UI;

public abstract class Element {
    public IRenderer renderer { get; set; }
    public virtual bool enabled { get; set; } = true;
    public virtual Vector2Int position { get; set; }
    public virtual Vector2Int size { get; set; }

    public virtual Bounds bounds {
        get {
            Vector2Int position = this.position;
            Vector2Int size = this.size;
            return new Bounds(position, new Vector2Int(position.x + size.x - 1, position.y + size.y - 1));
        }
    }

    public virtual Vector2Int center =>
        new(position.x + (int)(size.x / 2f - 0.5f), position.y + (int)(size.y / 2f - 0.5f));

    public IEffect? effect { get; set; }

    protected Element(IRenderer renderer) => this.renderer = renderer;

    public abstract void Update(IReadOnlyStopwatch clock);
}
