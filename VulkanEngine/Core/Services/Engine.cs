using VulkanEngine.Application;
using VulkanEngine.Application.Container;
using VulkanEngine.Application.Services;
using VulkanEngine.Core;
using VulkanEngine.Core.Interfaces;
using VulkanEngine.Infrastructure.Input;
using VulkanEngine.Infrastructure.Vulkan;
using VulkanEngine.Infrastructure.Window;

namespace VulkanEngine.Core.Services;

public class Engine : IDisposable
{
    private readonly EngineConfig _config;
    private readonly ServiceContainer _container;
    private WindowManager? _windowManager;
    private IRenderer? _renderer;
    private InputHandler? _inputHandler;
    private CameraController? _cameraController;
    private Scene? _scene;
    private bool _isRunning;
    private float _totalTime;

    public Engine(EngineConfig config)
    {
        _config = config;
        _container = new ServiceContainer();
    }

    public void Initialize()
    {
        Console.WriteLine("Initializing Vulkan Raytracing Engine...");

        _inputHandler = new InputHandler();
        _scene = SceneBuilder.CreateDemoScene();
        _cameraController = new CameraController(_scene.Camera, _inputHandler);

        _windowManager = new WindowManager(_config);
        _windowManager.OnLoad += OnWindowLoad;
        _windowManager.OnUpdate += Update;
        _windowManager.OnRender += Render;
        _windowManager.OnResize += Resize;
        _windowManager.Initialize();

        _isRunning = true;
    }

    private void OnWindowLoad()
    {
        _renderer = new VulkanRenderer(_windowManager!, _config);
        _renderer.Initialize();

        _container.RegisterInstance(_windowManager!);
        _container.RegisterInstance(_renderer);
        _container.RegisterInstance(_inputHandler!);
        _container.RegisterInstance(_scene!);

        if (_windowManager!.InputContext != null)
        {
            _inputHandler!.Initialize(_windowManager.InputContext);
        }

        Console.WriteLine("Engine initialized successfully!");
    }

    public void Run()
    {
        if (!_isRunning)
        {
            throw new InvalidOperationException("Engine not initialized");
        }

        _windowManager?.Run();
    }

    private void Update(float deltaTime)
    {
        _totalTime += deltaTime;

        _cameraController?.Update(deltaTime);
        _inputHandler?.Update(deltaTime);

        if (_inputHandler?.IsKeyPressed(Silk.NET.Input.Key.Escape) == true)
        {
            Stop();
        }
    }

    private void Render(float deltaTime)
    {
        if (_scene != null)
        {
            _renderer?.Render(_scene, _totalTime);
        }
    }

    private void Resize(int width, int height)
    {
        _renderer?.Resize(width, height);

        if (_scene?.Camera != null)
        {
            _scene.Camera.SetAspectRatio((float)width / height);
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _windowManager?.Close();
    }

    public void Dispose()
    {
        _renderer?.Dispose();
        _inputHandler?.Dispose();
        _windowManager?.Dispose();
        _container.Clear();
    }
}
