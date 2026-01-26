using VulkanEngine.Application;
using VulkanEngine.Core.Services;

namespace VulkanEngine;

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
            Width = 1280,
            Height = 720,
            VSync = true,
            EnableValidation = true
        };

        try
        {
            var engine = new Engine(config);
            engine.Initialize();
            engine.Run();
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
