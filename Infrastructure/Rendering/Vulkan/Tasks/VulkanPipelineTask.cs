using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace Infrastructure.Rendering.Vulkan.Tasks;

public unsafe class VulkanPipelineTask(Vk vk, Device device)
{
    public DescriptorSetLayout CreateDescriptorSetLayout(DescriptorSetLayoutBinding[] bindings)
    {
        fixed (DescriptorSetLayoutBinding* bindingsPtr = bindings)
        {
            DescriptorSetLayoutCreateInfo layoutInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = (uint)bindings.Length,
                PBindings = bindingsPtr
            };

            if (vk.CreateDescriptorSetLayout(device, &layoutInfo, null, out DescriptorSetLayout layout) !=
                Result.Success) throw new Exception("Failed to create descriptor set layout");

            return layout;
        }
    }

    public DescriptorPool CreateDescriptorPool(DescriptorPoolSize[] poolSizes, uint maxSets)
    {
        fixed (DescriptorPoolSize* poolSizesPtr = poolSizes)
        {
            DescriptorPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = (uint)poolSizes.Length,
                PPoolSizes = poolSizesPtr,
                MaxSets = maxSets
            };

            if (vk.CreateDescriptorPool(device, &poolInfo, null, out DescriptorPool pool) != Result.Success)
                throw new Exception("Failed to create descriptor pool");

            return pool;
        }
    }

    public DescriptorSet AllocateDescriptorSet(DescriptorPool pool, DescriptorSetLayout layout)
    {
        DescriptorSetAllocateInfo allocInfo = new()
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = pool,
            DescriptorSetCount = 1,
            PSetLayouts = &layout
        };

        if (vk.AllocateDescriptorSets(device, &allocInfo, out DescriptorSet descriptorSet) != Result.Success)
            throw new Exception("Failed to allocate descriptor set");

        return descriptorSet;
    }

    public void UpdateDescriptorSets(WriteDescriptorSet[] writes)
    {
        fixed (WriteDescriptorSet* writesPtr = writes)
        {
            vk.UpdateDescriptorSets(device, (uint)writes.Length, writesPtr, 0, null);
        }
    }

    public ShaderModule CreateShaderModule(byte[] code)
    {
        fixed (byte* codePtr = code)
        {
            ShaderModuleCreateInfo createInfo = new()
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)code.Length,
                PCode = (uint*)codePtr
            };

            if (vk.CreateShaderModule(device, &createInfo, null, out ShaderModule shaderModule) != Result.Success)
                throw new Exception("Failed to create shader module");

            return shaderModule;
        }
    }

    public Pipeline CreateComputePipeline(ShaderModule shaderModule, PipelineLayout layout)
    {
        PipelineShaderStageCreateInfo shaderStageInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.ComputeBit,
            Module = shaderModule,
            PName = (byte*)Marshal.StringToHGlobalAnsi("main")
        };

        ComputePipelineCreateInfo pipelineInfo = new()
        {
            SType = StructureType.ComputePipelineCreateInfo,
            Stage = shaderStageInfo,
            Layout = layout
        };

        Pipeline pipeline;
        if (vk.CreateComputePipelines(device, default, 1, &pipelineInfo, null, &pipeline) != Result.Success)
            throw new Exception("Failed to create compute pipeline");

        Marshal.FreeHGlobal((nint)shaderStageInfo.PName);

        return pipeline;
    }

    public PipelineLayout CreatePipelineLayout(DescriptorSetLayout descriptorSetLayout)
    {
        PipelineLayoutCreateInfo pipelineLayoutInfo = new()
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 1,
            PSetLayouts = &descriptorSetLayout
        };

        if (vk.CreatePipelineLayout(device, &pipelineLayoutInfo, null, out PipelineLayout layout) != Result.Success)
            throw new Exception("Failed to create pipeline layout");

        return layout;
    }

    public void DestroyPipeline(Pipeline pipeline, PipelineLayout layout, ShaderModule shaderModule,
        DescriptorSetLayout descriptorSetLayout)
    {
        vk.DestroyPipeline(device, pipeline, null);
        vk.DestroyPipelineLayout(device, layout, null);
        vk.DestroyShaderModule(device, shaderModule, null);
        vk.DestroyDescriptorSetLayout(device, descriptorSetLayout, null);
    }
}