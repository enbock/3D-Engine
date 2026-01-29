using Application.Container;
using Application.Input;
using Application.Scene;
using Application.Window;
using Core.EngineInitialization;
using Core.EngineRendering;
using Core.EngineUpdate;
using Core.Input;
using Core.Rendering;
using Core.Scene;
using Infrastructure.Rendering;
using Silk.NET.Input;

namespace Application.Engine;

public class EngineController : IDisposable
{
    private readonly EngineConfig config;
    private readonly ServiceContainer container;

    private InitializeEngineUseCase? initializeUseCase;
    private InputHandlerService? inputHandler;
    private bool isRunning;
    private RenderEngineUseCase? renderUseCase;
    private SceneEntity? scene;
    private UpdateEngineUseCase? updateUseCase;
    private WindowManagerService? windowManager;

    public EngineController(EngineConfig config)
    {
        this.config = config;
        container = new ServiceContainer();
    }

    public void Dispose()
    {
        container.Clear();
        inputHandler?.Dispose();
        windowManager?.Dispose();
    }

    public InitializeEngineResponse Initialize()
    {
        try
        {
            Console.WriteLine("Initializing Vulkan Raytracing Engine...");

            SetupInfrastructure();
            SetupUseCases();

            InitializeEngineRequest request = new(config);
            InitializeEngineResponse response = initializeUseCase!.Run(request);

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

        SceneBuilderService sceneBuilder = new();
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
        VulkanRenderer renderer = new(windowManager!, config);
        renderer.Initialize();

        container.RegisterInstance(windowManager!);
        container.RegisterInstance<Renderer>(renderer);
        container.RegisterInstance<InputHandler>(inputHandler!);
        container.RegisterInstance(scene!);

        if (windowManager!.InputContext != null) inputHandler!.Initialize(windowManager.InputContext);
    }

    public void Run()
    {
        if (!isRunning) throw new InvalidOperationException("Engine not initialized");

        windowManager?.Run();
    }

    private void OnUpdate(float deltaTime)
    {
        UpdateEngineRequest request = new(deltaTime);
        updateUseCase?.Run(request);

        if (inputHandler?.IsKeyPressed(Key.Escape) == true) Stop();
    }

    private void OnRender(float deltaTime)
    {
        RenderEngineRequest request = new(deltaTime);
        renderUseCase?.Run(request);
    }

    private void OnResize(int width, int height)
    {
        Renderer renderer = container.Resolve<Renderer>();
        renderer.Resize(width, height);

        if (scene?.Camera != null) scene.Camera.SetAspectRatio((float)width / height);
    }

    public void Stop()
    {
        isRunning = false;
        windowManager?.Close();
    }
}