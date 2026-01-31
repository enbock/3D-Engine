using Infrastructure.Assets;

namespace Core.Assets;

public interface TextureLoader : IDisposable
{
    TextureHandle LoadTexture(string filePath);
    TextureHandle LoadTextureFromBytes(byte[] data, string name);
    TextureHandle LoadNormalMapFromBytes(byte[] data, string name);
    TextureHandle LoadNormalMap(string filePath);
    Texture? GetTexture(int id);
    void DisposeTexture(TextureHandle handle);
    void DisposeAll();
}