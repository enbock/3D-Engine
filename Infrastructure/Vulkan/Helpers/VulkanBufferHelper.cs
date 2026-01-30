using System.Numerics;
using Application;
using Core.Math;
using Core.Scene;
using Core.Scene.Acceleration;
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
        out Buffer settingsBuffer, out DeviceMemory settingsBufferMemory,
        out Buffer bvhBuffer, out DeviceMemory bvhBufferMemory)
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

        CreateTriangleBufferWithBvh(bufferTask, scene,
            out triangleBuffer, out triangleBufferMemory,
            out bvhBuffer, out bvhBufferMemory);
    }

    private static void CreateTriangleBufferWithBvh(
        VulkanBufferTask bufferTask,
        SceneEntity scene,
        out Buffer triangleBuffer, out DeviceMemory triangleBufferMemory,
        out Buffer bvhBuffer, out DeviceMemory bvhBufferMemory)
    {
        List<TriangleEntity> triangles = scene.Triangles;

        if (triangles.Count == 0)
        {
            triangles =
            [
                new TriangleEntity(
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Color(1, 0, 0))
            ];
        }

        BvhBuilderService bvhBuilder = new();
        BvhNodeEntity bvhRoot = bvhBuilder.Build(triangles);
        (List<FlatBvhNode> flatNodes, List<int> reorderedIndices) = bvhBuilder.FlattenForGpu(bvhRoot);

        TriangleData[] triangleData = new TriangleData[triangles.Count];
        for (int i = 0; i < triangles.Count; i++)
        {
            int originalIdx = reorderedIndices[i];
            TriangleEntity tri = triangles[originalIdx];
            triangleData[i] = new TriangleData
            {
                V0 = new System.Numerics.Vector3(tri.V0.X, tri.V0.Y, tri.V0.Z),
                V1 = new System.Numerics.Vector3(tri.V1.X, tri.V1.Y, tri.V1.Z),
                V2 = new System.Numerics.Vector3(tri.V2.X, tri.V2.Y, tri.V2.Z),
                Color = new System.Numerics.Vector3(tri.Color.R, tri.Color.G, tri.Color.B),
                Transparency = tri.Material.Transparency,
                N0 = new System.Numerics.Vector3(tri.N0.X, tri.N0.Y, tri.N0.Z),
                IndexOfRefraction = tri.Material.IndexOfRefraction,
                N1 = new System.Numerics.Vector3(tri.N1.X, tri.N1.Y, tri.N1.Z),
                Reflectivity = tri.Material.Reflectivity,
                N2 = new System.Numerics.Vector3(tri.N2.X, tri.N2.Y, tri.N2.Z),
                EnableSchlieren = tri.Material.EnableSchlieren ? 1.0f : 0.0f
            };
        }

        ulong triangleBufferSize = (ulong)(sizeof(TriangleData) * triangles.Count);
        bufferTask.CreateBuffer(triangleBufferSize, BufferUsageFlags.StorageBufferBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out triangleBuffer, out triangleBufferMemory);
        bufferTask.CopyDataToBuffer(triangleBufferMemory, triangleData);

        BvhNodeData[] bvhData = new BvhNodeData[flatNodes.Count];
        for (int i = 0; i < flatNodes.Count; i++)
        {
            FlatBvhNode node = flatNodes[i];
            bvhData[i] = new BvhNodeData
            {
                BoundsMin = new System.Numerics.Vector3(node.BoundsMin.X, node.BoundsMin.Y, node.BoundsMin.Z),
                LeftChild = node.LeftChild,
                BoundsMax = new System.Numerics.Vector3(node.BoundsMax.X, node.BoundsMax.Y, node.BoundsMax.Z),
                RightChild = node.RightChild,
                TriangleStart = node.TriangleStart,
                TriangleCount = node.TriangleCount,
                Pad0 = 0,
                Pad1 = 0
            };
        }

        ulong bvhBufferSize = (ulong)(sizeof(BvhNodeData) * Math.Max(flatNodes.Count, 1));
        bufferTask.CreateBuffer(bvhBufferSize, BufferUsageFlags.StorageBufferBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out bvhBuffer, out bvhBufferMemory);

        if (bvhData.Length > 0)
        {
            bufferTask.CopyDataToBuffer(bvhBufferMemory, bvhData);
        }

        Console.WriteLine($"BVH created: {flatNodes.Count} nodes for {triangles.Count} triangles");
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
                ShadowSoftness = lights[i].ShadowSoftness,
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

        RenderSettings settings = RenderSettings.Quality;
        RenderSettingsData settingsData = new()
        {
            MaxBounces = settings.MaxBounces,
            EnableShadows = settings.EnableShadows ? 1 : 0,
            EnableReflections = settings.EnableReflections ? 1 : 0,
            ReflectionStrength = settings.ReflectionStrength,
            ShadowSamples = settings.ShadowSamples,
            Pad1 = 0,
            Pad2 = new Vector2(0, 0)
        };

        bufferTask.CopyDataToBuffer(settingsBufferMemory, settingsData);
    }
}