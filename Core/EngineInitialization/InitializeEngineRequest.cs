using Application;

namespace Core.EngineInitialization;

public class InitializeEngineRequest
{
    public InitializeEngineRequest(EngineConfig config)
    {
        Config = config;
    }

    public EngineConfig Config { get; set; }
}