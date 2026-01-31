using Application;
using Application.Container;
using Application.Game;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("   Vulkan Raytracing Engine");
        Console.WriteLine("   Native C# with Silk.NET");
        Console.WriteLine("===========================================\n");

        EngineConfig config = new()
        {
            Title = "Vulkan Raytracing Engine - C#",
            Width = 2560,
            Height = 1440,
            VSync = true,
            EnableValidation = true,
            EnableHdr10 = false,
            HdrMinNits = 0.0f,
            HdrMaxNits = 400.0f,
            Exposure = 0.65f,
            Gamma = 2.2f,
            ToneMapping = ToneMappingOperator.AcesFilmic
        };

        try
        {
            ServiceContainer container = new(config);

            GameController gameController = container.Resolve<GameController>();
            bool isRunning = gameController.Initialize();

            if (isRunning)
            {
                gameController.Run();
            }
            else
            {
                Console.WriteLine("Engine initialization failed.");
            }

            container.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }

        Console.WriteLine("\nEngine shutdown complete.");
    }
}