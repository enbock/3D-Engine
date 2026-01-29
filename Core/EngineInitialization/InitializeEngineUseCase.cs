using Application.Container;

namespace Core.EngineInitialization;

public class InitializeEngineUseCase(ServiceContainer container)
{
    private readonly ServiceContainer container = container;

    public InitializeEngineResponse Run(InitializeEngineRequest request)
    {
        try
        {
            return InitializeEngineResponse.Ok();
        }
        catch (Exception ex)
        {
            return InitializeEngineResponse.Error(ex.Message);
        }
    }
}