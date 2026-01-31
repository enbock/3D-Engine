namespace Core.Scene.Geometry;

public struct TextureCoordinate(float u, float v)
{
    public float U { get; set; } = u;
    public float V { get; set; } = v;

    public static TextureCoordinate Zero => new(0, 0);
}