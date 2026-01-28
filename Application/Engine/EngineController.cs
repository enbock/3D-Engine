using Application.Container;
using Core.EngineInitialization;
using Core.EngineUpdate;
using Core.EngineRendering;
using Core.Rendering;
using Core.Input;
using Infrastructure.Rendering;
using Application.Input;
using Application.Window;
using Application.Scene;
using Core.Scene;

namespace Application.Engine;

public class EngineController : IDisposable
{
    private readonly EngineConfig config;
    private readonly ServiceContainer container;
    private WindowManagerService? windowManager;
    private InputHandlerService? inputHandler;
    private SceneEntity? scene;
    private bool isRunning;

    private InitializeEngineUseCase? initializeUseCase;
    private UpdateEngineUseCase? updateUseCase;
    private RenderEngineUseCase? renderUseCase;

    public EngineController(EngineConfig config)
    {
        this.config = config;
        this.container = new ServiceContainer();
    }

    public InitializeEngineResponse Initialize()
    {
        try
        {
            Console.WriteLine("Initializing Vulkan Raytracing Engine...");

            SetupInfrastructure();
            SetupUseCases();

            var request = new InitializeEngineRequest(config);
            var response = initializeUseCase!.Run(request);

            if (response.Success)
            {
                isRunning = true;
                Console.WriteLine("Engine initialized successfully!");
            }

            return response;
        }
        catch (Exception ex)
        {
            return InitializeEngineResponse.Error(ex.Message);
        }
    }

    private void SetupInfrastructure()
    {
        inputHandler = new InputHandlerService();
        
        var sceneBuilder = new SceneBuilderService();
        scene = sceneBuilder.CreateDemoScene();

        windowManager = new WindowManagerService(config);
        windowManager.OnLoad += OnWindowLoad;
        windowManager.OnUpdate += OnUpdate;
        windowManager.OnRender += OnRender;
        windowManager.OnResize += OnResize;
        windowManager.Initialize();
    }

    private void SetupUseCases()
    {
        initializeUseCase = new InitializeEngineUseCase(container);
        updateUseCase = new UpdateEngineUseCase(container);
        renderUseCase = new RenderEngineUseCase(container);
    }

    private void OnWindowLoad()
    {
        var renderer = new VulkanRenderer(windowManager!, config);
        renderer.Initialize();

        container.RegisterInstance(windowManager!);
        container.RegisterInstance<Renderer>(renderer);
        container.RegisterInstance<InputHandler>(inputHandler!);
        container.RegisterInstance(scene!);

        if (windowManager!.InputContext != null)
        {
            inputHandler!.Initialize(windowManager.InputContext);
        }
    }

    public void Run()
    {
        if (!isRunning)
        {
            throw new InvalidOperationException("Engine not initialized");
        }

        windowManager?.Run();
    }

    private void OnUpdate(float deltaTime)
    {
        var request = new UpdateEngineRequest(deltaTime);
        updateUseCase?.Run(request);

        if (inputHandler?.IsKeyPressed(Silk.NET.Input.Key.Escape) == true)
        {
            Stop();
        }
    }

    private void OnRender(float deltaTime)
    {
        var request = new RenderEngineRequest(deltaTime);
        renderUseCase?.Run(request);
    }

    private void OnResize(int width, int height)
    {
        var renderer = container.Resolve<Renderer>();
        renderer.Resize(width, height);

        if (scene?.Camera != null)
        {
            scene.Camera.SetAspectRatio((float)width / height);
        }
    }

    public void Stop()
    {
        isRunning = false;
        windowManager?.Close();
    }

    public void Dispose()
    {
        container.Clear();
        inputHandler?.Dispose();
        windowManager?.Dispose();
    }
}
