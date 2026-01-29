using Core.Math;
using Silk.NET.Input;

namespace Core.Input;

public interface InputHandler : IDisposable
{
    void Initialize(IInputContext inputContext);
    void Update(float deltaTime);
    bool IsKeyPressed(Key key);
    Vector3 GetMovementInput();
    Vector3 GetMouseDelta();
    void ResetMouseDelta();
}