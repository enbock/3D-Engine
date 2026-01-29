namespace Core.EngineInitialization;

public class InitializeEngineResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static InitializeEngineResponse Ok()
    {
        return new InitializeEngineResponse { Success = true };
    }

    public static InitializeEngineResponse Error(string message)
    {
        return new InitializeEngineResponse { Success = false, ErrorMessage = message };
    }
}