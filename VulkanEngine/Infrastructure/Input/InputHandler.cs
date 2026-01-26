using Silk.NET.Input;
using VulkanEngine.Core.Interfaces;

namespace VulkanEngine.Infrastructure.Input;

public class InputHandler : IInputHandler
{
    private IInputContext? _inputContext;
    private IKeyboard? _keyboard;
    private IMouse? _mouse;
    private readonly HashSet<Key> _pressedKeys = new();
    private readonly HashSet<Key> _downKeys = new();
    private readonly HashSet<MouseButton> _pressedButtons = new();
    private float _mouseX;
    private float _mouseY;
    private float _lastMouseX;
    private float _lastMouseY;
    private bool _firstMouse = true;

    public void Initialize(IInputContext inputContext)
    {
        _inputContext = inputContext;

        _keyboard = _inputContext.Keyboards.FirstOrDefault();
        if (_keyboard != null)
        {
            _keyboard.KeyDown += OnKeyDown;
            _keyboard.KeyUp += OnKeyUp;
        }

        _mouse = _inputContext.Mice.FirstOrDefault();
        if (_mouse != null)
        {
            _mouse.MouseMove += OnMouseMove;
            _mouse.MouseDown += OnMouseDown;
            _mouse.MouseUp += OnMouseUp;
        }
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        if (!_downKeys.Contains(key))
        {
            _pressedKeys.Add(key);
        }
        _downKeys.Add(key);
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int scancode)
    {
        _downKeys.Remove(key);
    }

    private void OnMouseMove(IMouse mouse, System.Numerics.Vector2 position)
    {
        if (_firstMouse)
        {
            _lastMouseX = position.X;
            _lastMouseY = position.Y;
            _firstMouse = false;
        }

        _mouseX = position.X;
        _mouseY = position.Y;
    }

    private void OnMouseDown(IMouse mouse, MouseButton button)
    {
        _pressedButtons.Add(button);
    }

    private void OnMouseUp(IMouse mouse, MouseButton button)
    {
        _pressedButtons.Remove(button);
    }

    public void Update(float deltaTime)
    {
        _pressedKeys.Clear();
        _lastMouseX = _mouseX;
        _lastMouseY = _mouseY;
    }

    public bool IsKeyPressed(Key key) => _pressedKeys.Contains(key);
    public bool IsKeyDown(Key key) => _downKeys.Contains(key);
    public bool IsMouseButtonPressed(MouseButton button) => _pressedButtons.Contains(button);
    public (float X, float Y) GetMousePosition() => (_mouseX, _mouseY);
    public (float X, float Y) GetMouseDelta() => (_mouseX - _lastMouseX, _mouseY - _lastMouseY);

    public void Dispose()
    {
        if (_keyboard != null)
        {
            _keyboard.KeyDown -= OnKeyDown;
            _keyboard.KeyUp -= OnKeyUp;
        }

        if (_mouse != null)
        {
            _mouse.MouseMove -= OnMouseMove;
            _mouse.MouseDown -= OnMouseDown;
            _mouse.MouseUp -= OnMouseUp;
        }
    }
}
