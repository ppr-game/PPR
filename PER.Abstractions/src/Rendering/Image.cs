using JetBrains.Annotations;

using PER.Util;

namespace PER.Abstractions.Rendering;

[PublicAPI]
public readonly record struct Image(Color[,] pixels) {
    public Image(int width, int height) : this(new Color[height, width]) { }

    public Color this[int x, int y] {
        get {
            if(x < 0 || x >= width || y < 0 || y >= height)
                return Color.transparent;
            return pixels[y, x];
        }
        set {
            if(x < 0 || x >= width || y < 0 || y >= height)
                return;
            pixels[y, x] = value;
        }
    }

    public void DrawImage(Vector2Int position, Image image) {
        for(int y = 0; y < image.height; y++)
            for(int x = 0; x < image.width; x++)
                this[position.x + x, position.y + y] = image[x, y];
    }

    public int width => pixels.GetLength(1);
    public int height => pixels.GetLength(0);
}
