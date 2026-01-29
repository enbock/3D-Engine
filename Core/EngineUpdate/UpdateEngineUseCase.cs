using Application.CameraControl;
using Application.Container;
using Core.Input;
using Core.Scene;

namespace Core.EngineUpdate;

public class UpdateEngineUseCase(ServiceContainer container)
{
    private CameraControlUseCase? cameraControlUseCase;

    public void Run(UpdateEngineRequest request)
    {
        if (cameraControlUseCase == null && container.TryResolve(out SceneEntity? scene))
        {
            InputHandler inputHandler = container.Resolve<InputHandler>();
            cameraControlUseCase = new CameraControlUseCase(scene!.Camera, inputHandler);
        }

        cameraControlUseCase?.Run(request.DeltaTime);

        if (container.TryResolve(out InputHandler? input)) input?.Update(request.DeltaTime);
    }
}