using System.IO;

using JetBrains.Annotations;

using PER.Abstractions.Rendering;
using PER.Util;

using QoiSharp;

namespace PRR;

[PublicAPI]
public class Font : BasicFont {
    public Font(string imagePath, string mappingsPath) : base(imagePath, mappingsPath) { }
    public Font(Image image, string mappings, Vector2Int size, char backgroundCharacter) :
        base(image, mappings, size, backgroundCharacter) { }

    protected override Image ReadImage(string path) {
        QoiImage qoiImage = QoiDecoder.Decode(File.ReadAllBytes(path));

        byte channels = (byte)qoiImage.Channels;
        Image image = new(qoiImage.Width, qoiImage.Height);
        for(int i = 0; i < qoiImage.Data.Length; i += channels) {
            int pixelIndex = i / channels;
            int x = pixelIndex % image.width;
            int y = pixelIndex / image.width;
            byte alpha = channels > 3 ? qoiImage.Data[i + 3] : byte.MaxValue;
            image[x, y] = new Color(qoiImage.Data[i], qoiImage.Data[i + 1], qoiImage.Data[i + 2], alpha);
        }
        return image;
    }
}
