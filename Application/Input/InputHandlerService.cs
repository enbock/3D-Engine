using Silk.NET.Input;
using Core.Input;
using Core.Math;

namespace Application.Input;

public class InputHandlerService : InputHandler
{
    private IInputContext? inputContext;
    private IKeyboard? keyboard;
    private IMouse? mouse;
    private Vector3 mouseDelta;
    private Vector3 lastMousePosition;
    private bool isFirstMouse = true;

    public void Initialize(IInputContext context)
    {
        inputContext = context;
        keyboard = inputContext.Keyboards.FirstOrDefault();
        mouse = inputContext.Mice.FirstOrDefault();

        if (mouse != null)
        {
            mouse.Cursor.CursorMode = CursorMode.Raw;
            mouse.MouseMove += OnMouseMove;
        }
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
        var movement = Vector3.Zero;

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

    private void OnMouseMove(IMouse mouse, System.Numerics.Vector2 position)
    {
        if (isFirstMouse)
        {
            lastMousePosition = new Vector3(position.X, position.Y, 0);
            isFirstMouse = false;
            return;
        }

        var currentPos = new Vector3(position.X, position.Y, 0);
        mouseDelta = currentPos - lastMousePosition;
        lastMousePosition = currentPos;
    }

    public void Dispose()
    {
        if (mouse != null)
        {
            mouse.MouseMove -= OnMouseMove;
        }
    }
}
