using Application.Game.Handler;
using Application.Window;
using Core.EngineRendering;
using Core.Scene;
using Core.World;
using Silk.NET.Input;

namespace Application.Game;

public class GameController(
    WindowManager windowManager,
    InputHandler inputHandler,
    Renderer renderer,
    RenderEngineUseCase renderUseCase,
    CameraControlHandler cameraControlHandler,
    WorldUseCase worldUseCase,
    Action initializeAssetLoaders
) : IDisposable
{
    private bool isRunning;

    public void Dispose()
    {
        windowManager.Dispose();
    }

    public bool Initialize()
    {
        try
        {
            Console.WriteLine("Initializing Vulkan Raytracing Engine...");

            windowManager.OnLoad += OnWindowLoad;
            windowManager.OnUpdate += OnUpdate;
            windowManager.OnRender += OnRender;
            windowManager.OnResize += OnResize;
            windowManager.Initialize();

            isRunning = true;
            Console.WriteLine("Engine initialized.");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Engine error:{ex.Message}");
            return false;
        }
    }

    private void OnWindowLoad()
    {
        renderer.Initialize();
        initializeAssetLoaders();
        inputHandler.Initialize(windowManager.GetInputContext());
        cameraControlHandler.Initialize();

        worldUseCase.InitializeWithTeapot();

        Console.WriteLine("World initialized with Teapot scene.");
    }

    public void Run()
    {
        if (!isRunning) throw new InvalidOperationException("Engine not initialized");

        windowManager.Run();
    }

    private void OnUpdate(float deltaTime)
    {
        cameraControlHandler.Update(deltaTime);
        inputHandler.Update(deltaTime);

        if (inputHandler.IsKeyPressed(Key.Escape)) Stop();
    }

    private void OnRender(float deltaTime)
    {
        SceneEntity scene = worldUseCase.GetScene();
        RenderEngineRequest request = new(deltaTime, scene);
        renderUseCase.Run(request);
    }

    private void OnResize(int width, int height)
    {
        renderer.Resize(width, height);
        worldUseCase.UpdateAspectRatio((float)width / height);
    }

    private void Stop()
    {
        isRunning = false;
        windowManager.Close();
    }
}