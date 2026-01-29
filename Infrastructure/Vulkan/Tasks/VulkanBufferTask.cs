using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Infrastructure.Vulkan.Tasks;

public unsafe class VulkanBufferTask(Vk vk, Device device, VulkanDeviceTask deviceTask)
{
    public void CreateBuffer(
        ulong size,
        BufferUsageFlags usage,
        MemoryPropertyFlags properties,
        out Buffer buffer,
        out DeviceMemory bufferMemory)
    {
        BufferCreateInfo bufferInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = size,
            Usage = usage,
            SharingMode = SharingMode.Exclusive
        };

        if (vk.CreateBuffer(device, &bufferInfo, null, out buffer) != Result.Success)
            throw new Exception("Failed to create buffer");

        MemoryRequirements memRequirements;
        vk.GetBufferMemoryRequirements(device, buffer, &memRequirements);

        MemoryAllocateInfo allocInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = deviceTask.FindMemoryType(memRequirements.MemoryTypeBits, properties)
        };

        if (vk.AllocateMemory(device, &allocInfo, null, out bufferMemory) != Result.Success)
            throw new Exception("Failed to allocate buffer memory");

        vk.BindBufferMemory(device, buffer, bufferMemory, 0);
    }

    public void CopyDataToBuffer<T>(DeviceMemory memory, T[] data) where T : unmanaged
    {
        ulong size = (ulong)(sizeof(T) * data.Length);
        void* mappedData;
        vk.MapMemory(device, memory, 0, size, 0, &mappedData);

        fixed (T* dataPtr = data)
        {
            System.Buffer.MemoryCopy(dataPtr, mappedData, size, size);
        }

        vk.UnmapMemory(device, memory);
    }

    public void CopyDataToBuffer<T>(DeviceMemory memory, T data) where T : unmanaged
    {
        ulong size = (ulong)sizeof(T);
        void* mappedData;
        vk.MapMemory(device, memory, 0, size, 0, &mappedData);
        *(T*)mappedData = data;
        vk.UnmapMemory(device, memory);
    }

    public void DestroyBuffer(Buffer buffer, DeviceMemory memory)
    {
        vk.DestroyBuffer(device, buffer, null);
        vk.FreeMemory(device, memory, null);
    }
}