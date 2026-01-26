using Silk.NET.Input;

namespace VulkanEngine.Core.Interfaces;

public interface IInputHandler : IDisposable
{
    void Initialize(IInputContext inputContext);
    void Update(float deltaTime);
    bool IsKeyPressed(Key key);
    bool IsKeyDown(Key key);
    bool IsMouseButtonPressed(MouseButton button);
    (float X, float Y) GetMousePosition();
    (float X, float Y) GetMouseDelta();
}
