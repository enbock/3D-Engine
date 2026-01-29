using Application;

namespace Core.EngineInitialization;

public class InitializeEngineRequest(EngineConfig config)
{
    public EngineConfig Config { get; set; } = config;
}