using JetBrains.Annotations;

namespace PER.Util;

[PublicAPI]
public static class Perlin {
    // Hash lookup table as defined by Ken Perlin.  This is a randomly
    // arranged array of all numbers from 0-255 inclusive.
    private static readonly int[] permutation = {
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37,
        240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177,
        33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146,
        158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25,
        63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100,
        109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206,
        59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153,
        101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246,
        97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192,
        214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114,
        67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
    };

    private static readonly int[] p; // Doubled permutation to avoid overflow

    static Perlin() {
        p = new int[512];
        for(int x = 0; x < 512; x++)
            p[x] = permutation[x % 256];
    }

    public static float Get(float x, float y, float z) {
        int xi = (int)x & 255; // Calculate the "unit cube" that the point asked will be located in
        int yi = (int)y & 255; // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
        int zi = (int)z & 255; // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that cube.
        float xf = x - (int)x; // We also fade the location to smooth the result.
        float yf = y - (int)y;
        float zf = z - (int)z;
        float u = Fade(xf);
        float v = Fade(yf);
        float w = Fade(zf);

        int aaa = p[p[p[xi] + yi] + zi];
        int aba = p[p[p[xi] + ++yi] + zi];
        int aab = p[p[p[xi] + yi] + ++zi];
        int abb = p[p[p[xi] + ++yi] + ++zi];
        int baa = p[p[p[++xi] + yi] + zi];
        int bba = p[p[p[++xi] + ++yi] + zi];
        int bab = p[p[p[++xi] + yi] + ++zi];
        int bbb = p[p[p[++xi] + ++yi] + ++zi];

        // The gradient function calculates the dot product between a pseudorandom
        // gradient vector and the vector from the input coordinate to the 8
        // This is all then lerped together as a sort of weighted average based on the faded (u,v,w)
        // values we made earlier.
        float x1 = MoreMath.LerpUnclamped(Gradual(aaa, xf, yf, zf), Gradual(baa, xf - 1f, yf, zf), u);
        float x2 = MoreMath.LerpUnclamped(Gradual(aba, xf, yf - 1f, zf), Gradual(bba, xf - 1f, yf - 1f, zf), u);
        float y1 = MoreMath.LerpUnclamped(x1, x2, v);

        x1 = MoreMath.LerpUnclamped(Gradual(aab, xf, yf, zf - 1f), Gradual(bab, xf - 1f, yf, zf - 1f), u);
        x2 = MoreMath.LerpUnclamped(Gradual(abb, xf, yf - 1f, zf - 1f), Gradual(bbb, xf - 1f, yf - 1f, zf - 1f), u);
        float y2 = MoreMath.LerpUnclamped(x1, x2, v);

        // For convenience we bound it to 0 - 1 (theoretical min/max before is -1 - 1)
        return (MoreMath.LerpUnclamped(y1, y2, w) + 1f) / 2f;
    }

    private static float Gradual(int hash, float x, float y, float z) {
        int h = hash & 0b1111;
        float u = h < 0b1000 ? x : y;

        float v = h switch {
            // If the first and second significant bits are 0 set v = y
            < 0b0100 => y,
            // If the first and second significant bits are 1 set v = x
            0b1100 or 0b1110 => x,
            _ => z
        };

        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    // Fade function as defined by Ken Perlin. This eases coordinate values
    // so that they will "ease" towards integral values. This ends up smoothing
    // the final output.
    private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f); // 6t^5 - 15t^4 + 10t^3
}
