using Application;
using Application.Engine;
using Core.EngineInitialization;

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
            EnableValidation = true
        };

        try
        {
            EngineController engine = new(config);
            InitializeEngineResponse response = engine.Initialize();

            if (response.Success)
            {
                engine.Run();
            }
            else
            {
                string errorMsg = response.ErrorMessage ?? "Unknown error";
                Console.WriteLine($"Engine initialization failed: {errorMsg}");
            }

            engine.Dispose();
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