using Application;

namespace Core.EngineInitialization;

public class InitializeEngineRequest
{
    public EngineConfig Config { get; set; }

    public InitializeEngineRequest(EngineConfig config)
    {
        Config = config;
    }
}
