using System.Numerics;
using Application;
using Core.Math;
using Core.Scene;
using Core.Scene.Geometry;
using Core.Scene.Light;
using Infrastructure.Vulkan.Data;
using Infrastructure.Vulkan.Tasks;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;
using Vector3 = Core.Math.Vector3;

namespace Infrastructure.Vulkan.Helpers;

public static unsafe class VulkanBufferHelper
{
    public static void CreateAndFillSceneBuffers(
        VulkanBufferTask bufferTask,
        SceneEntity scene,
        out Buffer cameraBuffer, out DeviceMemory cameraBufferMemory,
        out Buffer triangleBuffer, out DeviceMemory triangleBufferMemory,
        out Buffer lightBuffer, out DeviceMemory lightBufferMemory,
        out Buffer settingsBuffer, out DeviceMemory settingsBufferMemory)
    {
        bufferTask.CreateBuffer((ulong)sizeof(CameraUniformData), BufferUsageFlags.UniformBufferBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out cameraBuffer, out cameraBufferMemory);

        bufferTask.CreateBuffer((ulong)sizeof(LightUniformData), BufferUsageFlags.StorageBufferBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out lightBuffer, out lightBufferMemory);

        bufferTask.CreateBuffer((ulong)sizeof(RenderSettingsData), BufferUsageFlags.UniformBufferBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out settingsBuffer, out settingsBufferMemory);

        CreateTriangleBuffer(bufferTask, scene, out triangleBuffer, out triangleBufferMemory);
    }

    private static void CreateTriangleBuffer(VulkanBufferTask bufferTask, SceneEntity scene,
        out Buffer triangleBuffer, out DeviceMemory triangleBufferMemory)
    {
        TriangleEntity[] triangles = scene.Triangles.ToArray();
        if (triangles.Length == 0)
            triangles =
            [
                new TriangleEntity(
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Color(1, 0, 0))
            ];

        ulong bufferSize = (ulong)(sizeof(TriangleData) * triangles.Length);
        bufferTask.CreateBuffer(bufferSize, BufferUsageFlags.StorageBufferBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out triangleBuffer, out triangleBufferMemory);

        TriangleData[] triangleData = new TriangleData[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
            triangleData[i] = new TriangleData
            {
                V0 = new System.Numerics.Vector3(triangles[i].V0.X, triangles[i].V0.Y, triangles[i].V0.Z),
                V1 = new System.Numerics.Vector3(triangles[i].V1.X, triangles[i].V1.Y, triangles[i].V1.Z),
                V2 = new System.Numerics.Vector3(triangles[i].V2.X, triangles[i].V2.Y, triangles[i].V2.Z),
                Color = new System.Numerics.Vector3(triangles[i].Color.R, triangles[i].Color.G, triangles[i].Color.B)
            };

        bufferTask.CopyDataToBuffer(triangleBufferMemory, triangleData);
    }

    public static void UpdateSceneBuffers(
        VulkanBufferTask bufferTask,
        SceneEntity scene,
        DeviceMemory cameraBufferMemory,
        DeviceMemory lightBufferMemory,
        DeviceMemory settingsBufferMemory,
        Vector2 resolution,
        float time)
    {
        CameraUniformData cameraData = new()
        {
            Position = new System.Numerics.Vector3(scene.Camera.Position.X, scene.Camera.Position.Y,
                scene.Camera.Position.Z),
            Target = new System.Numerics.Vector3(scene.Camera.Target.X, scene.Camera.Target.Y, scene.Camera.Target.Z),
            Resolution = resolution,
            Time = time,
            Fov = scene.Camera.Fov
        };

        bufferTask.CopyDataToBuffer(cameraBufferMemory, cameraData);

        LightEntity[] lights = scene.Lights.Take(8).ToArray();
        LightUniformData lightData = new() { NumLights = lights.Length };

        for (int i = 0; i < lights.Length; i++)
        {
            LightData lightEntry = new()
            {
                Type = (int)lights[i].Type,
                Intensity = lights[i].Intensity,
                PositionX = lights[i].Position.X,
                PositionY = lights[i].Position.Y,
                PositionZ = lights[i].Position.Z,
                DirectionX = lights[i].Direction.X,
                DirectionY = lights[i].Direction.Y,
                DirectionZ = lights[i].Direction.Z,
                ColorR = lights[i].Color.R,
                ColorG = lights[i].Color.G,
                ColorB = lights[i].Color.B
            };
            lightData.SetLight(i, lightEntry);
        }

        bufferTask.CopyDataToBuffer(lightBufferMemory, lightData);

        RenderSettings settings = RenderSettings.Default;
        RenderSettingsData settingsData = new()
        {
            MaxBounces = settings.MaxBounces,
            EnableShadows = settings.EnableShadows ? 1 : 0,
            EnableReflections = settings.EnableReflections ? 1 : 0,
            ReflectionStrength = settings.ReflectionStrength,
            ShadowSamples = settings.ShadowSamples,
            ShadowSoftness = settings.ShadowSoftness,
            Pad = new Vector2(0, 0)
        };

        bufferTask.CopyDataToBuffer(settingsBufferMemory, settingsData);
    }
}