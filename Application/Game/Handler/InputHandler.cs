using System.Numerics;
using Silk.NET.Input;
using Vector3 = Core.Math.Vector3;

namespace Application.Game.Handler;

public class InputHandler : IDisposable
{
    private IInputContext? inputContext;
    private bool isFirstMouse = true;
    private IKeyboard? keyboard;
    private Vector3 lastMousePosition;
    private Vector3 mouseDelta;
    private IMouse? mouseDevice;

    public void Dispose()
    {
        if (mouseDevice != null)
        {
            mouseDevice.MouseMove -= OnMouseDeviceMove;
        }
    }

    public void Initialize(IInputContext context)
    {
        inputContext = context;
        keyboard = inputContext.Keyboards.FirstOrDefault();
        mouseDevice = inputContext.Mice.FirstOrDefault();

        if (mouseDevice == null) return;

        mouseDevice.Cursor.CursorMode = CursorMode.Raw;
        mouseDevice.MouseMove += OnMouseDeviceMove;
    }

    public void Update(float deltaTime)
    {
    }

    public bool IsKeyPressed(Key key)
    {
        return keyboard?.IsKeyPressed(key) ?? false;
    }

    public Vector3 GetMovementInput()
    {
        Vector3 movement = Vector3.Zero;

        if (keyboard == null) return movement;

        if (keyboard.IsKeyPressed(Key.W)) movement.Z -= 1;
        if (keyboard.IsKeyPressed(Key.S)) movement.Z += 1;
        if (keyboard.IsKeyPressed(Key.A)) movement.X -= 1;
        if (keyboard.IsKeyPressed(Key.D)) movement.X += 1;
        if (keyboard.IsKeyPressed(Key.Q)) movement.Y -= 1;
        if (keyboard.IsKeyPressed(Key.E)) movement.Y += 1;

        return movement;
    }

    public Vector3 GetMouseDelta()
    {
        return mouseDelta;
    }

    public void ResetMouseDelta()
    {
        mouseDelta = Vector3.Zero;
    }

    private void OnMouseDeviceMove(IMouse mouse, Vector2 position)
    {
        if (isFirstMouse)
        {
            lastMousePosition = new Vector3(position.X, position.Y, 0);
            isFirstMouse = false;
            return;
        }

        Vector3 currentPos = new(position.X, position.Y, 0);
        mouseDelta = currentPos - lastMousePosition;
        lastMousePosition = currentPos;
    }
}