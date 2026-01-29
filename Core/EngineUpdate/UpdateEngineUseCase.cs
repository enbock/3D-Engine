using Application.CameraControl;
using Application.Container;
using Core.Input;
using Core.Scene;

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

        if (cameraControlUseCase == null && container.TryResolve(out SceneEntity? scene))
        {
            InputHandler inputHandler = container.Resolve<InputHandler>();
            cameraControlUseCase = new CameraControlUseCase(scene!.Camera, inputHandler);
        }

        cameraControlUseCase?.Run(request.DeltaTime);

        if (container.TryResolve(out InputHandler? input)) input?.Update(request.DeltaTime);
    }
}