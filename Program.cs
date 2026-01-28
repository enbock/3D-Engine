using Application;
using Application.Engine;


public static class Program
{
    public static void Main()
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("   Vulkan Raytracing Engine");
        Console.WriteLine("   Native C# with Silk.NET");
        Console.WriteLine("===========================================\n");

        var config = new EngineConfig
        {
            Title = "Vulkan Raytracing Engine - C#",
            Width = 1920,
            Height = 1080,
            VSync = true,
            EnableValidation = true
        };

        try
        {
            var engine = new EngineController(config);
            var response = engine.Initialize();
            
            if (response.Success)
            {
                engine.Run();
            }
            else
            {
                var errorMsg = response.ErrorMessage ?? "Unknown error";
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
