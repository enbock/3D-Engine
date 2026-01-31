namespace Infrastructure.Assets;

public interface ITextureLoader
{
    TextureHandle LoadTexture(string filePath);
    TextureHandle LoadTextureFromBytes(byte[] data, string name);
    void DisposeTexture(TextureHandle handle);
    void DisposeAll();
}