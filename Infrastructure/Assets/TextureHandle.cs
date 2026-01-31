namespace Infrastructure.Assets;

public class TextureHandle
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public bool IsValid => Id >= 0;

    public static TextureHandle Invalid => new()
    {
        Id = -1,
        Name = "Invalid"
    };
}