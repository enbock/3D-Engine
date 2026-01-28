using Application.Container;

namespace Core.EngineInitialization;

public class InitializeEngineUseCase
{
    private readonly ServiceContainer container;

    public InitializeEngineUseCase(ServiceContainer container)
    {
        this.container = container;
    }

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
