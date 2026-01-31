using Application.Game;
using Application.Game.Handler;
using Application.Window;
using Core.CameraControl;
using Core.EngineRendering;
using Core.Rendering;
using Core.Scene;
using Core.World;
using Infrastructure.Rendering;
using Infrastructure.Vulkan;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using ModelLoader = Core.Assets.ModelLoader;

namespace Application.Container;

public class ServiceContainer : IDisposable
{
    private readonly Dictionary<Type, object> _services = new();

    public ServiceContainer(EngineConfig config)
    {
        WindowOptions options = WindowOptions.DefaultVulkan with
        {
            Title = config.Title,
            Size = new Vector2D<int>(config.Width, config.Height),
            VSync = config.VSync,
            API = new GraphicsAPI(ContextAPI.Vulkan, ContextProfile.Core, ContextFlags.Default, new APIVersion(1, 0))
        };

        RegisterInstance(Silk.NET.Windowing.Window.Create(options));

        RegisterInstance(new WindowManager(Resolve<IWindow>()));
        RegisterInstance(new InputHandler());

        InternalVulkanRenderer vulkanRenderer = new(Resolve<WindowManager>(), config);
        RegisterInstance(vulkanRenderer);
        RegisterInstance<Renderer>(new VulkanRenderer(vulkanRenderer));

        RegisterInstance(new WorldUseCase());
        RegisterInstance(new CameraControlUseCase());
        RegisterInstance(new CameraControlHandler(
            Resolve<CameraControlUseCase>(),
            Resolve<InputHandler>(),
            Resolve<WorldUseCase>()
        ));

        RegisterInstance(new RenderEngineUseCase(Resolve<Renderer>()));

        RegisterInstance(new GameController(
            Resolve<WindowManager>(),
            Resolve<InputHandler>(),
            Resolve<Renderer>(),
            Resolve<RenderEngineUseCase>(),
            Resolve<CameraControlHandler>(),
            Resolve<WorldUseCase>(),
            InitializeAssetLoaders
        ));
    }

    public void Dispose()
    {
        foreach (object service in _services.Values)
            if (service is IDisposable disposable)
                disposable.Dispose();

        _services.Clear();
    }

    public void InitializeAssetLoaders()
    {
        InternalVulkanRenderer vulkanRenderer = Resolve<InternalVulkanRenderer>();

        RegisterInstance(vulkanRenderer.TextureLoader);
        RegisterInstance(vulkanRenderer.TextureLoader);
        Infrastructure.Assets.ModelLoader modelLoader = new(vulkanRenderer.TextureLoader, "assets/models");
        RegisterInstance<ModelLoader>(modelLoader);
        RegisterInstance(modelLoader);

        SceneBuilderService sceneBuilder = new();
        RegisterInstance(sceneBuilder);

        WorldUseCase worldUseCase = Resolve<WorldUseCase>();
        worldUseCase.SetSceneBuilder(sceneBuilder);
        worldUseCase.SetModelLoader(modelLoader);
    }

    private void RegisterInstance<TInterface>(TInterface instance)
        where TInterface : notnull
    {
        _services[typeof(TInterface)] = instance;
    }

    public TInterface Resolve<TInterface>()
    {
        if (_services.TryGetValue(typeof(TInterface), out object? service)) return (TInterface)service;
        throw new InvalidOperationException($"Service {typeof(TInterface).Name} not registered");
    }
}