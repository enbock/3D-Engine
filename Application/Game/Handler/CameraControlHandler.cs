using Core.CameraControl;
using Core.Scene.Camera;
using Core.World;

namespace Application.Game.Handler;

public class CameraControlHandler(
    CameraControlUseCase cameraControlUseCase,
    InputHandler input,
    WorldUseCase worldUseCase
)
{
    public void Update(float deltaTime)
    {
        CameraEntity camera = worldUseCase.GetCamera();
        UpdateCameraMovementRequest movementRequest = new(input.GetMovementInput(), deltaTime, camera);
        cameraControlUseCase.UpdateMovement(movementRequest);

        UpdateCameraLookRequest lookRequest = new(input.GetMouseDelta(), camera);
        cameraControlUseCase.UpdateLook(lookRequest);

        input.ResetMouseDelta();
    }

    public void Initialize()
    {
        CameraEntity camera = worldUseCase.GetCamera();
        cameraControlUseCase.Initialize(camera);
    }
}