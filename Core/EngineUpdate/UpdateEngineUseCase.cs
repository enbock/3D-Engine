using Application.Container;
using Core.Input;
using Application.CameraControl;
using CoreScene = VulkanEngine.Core.Scene;

namespace Core.EngineUpdate;

public class UpdateEngineUseCase
{
    private readonly ServiceContainer container;
    private CameraControlUseCase? cameraControlUseCase;
    private float totalTime;

    public UpdateEngineUseCase(ServiceContainer container)
    {
        this.container = container;
    }

    public void Run(UpdateEngineRequest request)
    {
        totalTime += request.DeltaTime;

        if (cameraControlUseCase == null && container.TryResolve<CoreScene.SceneEntity>(out var scene))
        {
            var inputHandler = container.Resolve<InputHandler>();
            cameraControlUseCase = new CameraControlUseCase(scene!.Camera, inputHandler);
        }

        cameraControlUseCase?.Run(request.DeltaTime);

        if (container.TryResolve<InputHandler>(out var input))
        {
            input?.Update(request.DeltaTime);
        }
    }
}
