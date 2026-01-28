namespace Core.Math;

public struct Color
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public Color(float r, float g, float b, float a = 1.0f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public static Color White => new(1, 1, 1);
    public static Color Black => new(0, 0, 0);
    public static Color Red => new(1, 0, 0);
    public static Color Green => new(0, 1, 0);
    public static Color Blue => new(0, 0, 1);
    public static Color Yellow => new(1, 1, 0);
    public static Color Cyan => new(0, 1, 1);
    public static Color Magenta => new(1, 0, 1);
    public static Color Gray => new(0.5f, 0.5f, 0.5f);

    public static Color operator *(Color c, float s) => new(c.R * s, c.G * s, c.B * s, c.A);
    public static Color operator *(Color a, Color b) => new(a.R * b.R, a.G * b.G, a.B * b.B, a.A * b.A);
    public static Color operator +(Color a, Color b) => new(a.R + b.R, a.G + b.G, a.B + b.B, a.A);

    public Vector3 ToVector3() => new(R, G, B);

    public override string ToString() => $"({R:F2}, {G:F2}, {B:F2}, {A:F2})";
}
