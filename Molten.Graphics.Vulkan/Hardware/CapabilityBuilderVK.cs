using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal class CapabilityBuilderVK
{
    internal unsafe struct PropertiesRef
    {
        public StructureType SType;

        public void* PNext;
    }

    internal unsafe GraphicsCapabilities Build(DeviceVK device, RendererVK renderer, ref PhysicalDeviceProperties properties)
    {
        PhysicalDeviceFeatures features;

        if (renderer.ApiVersion < new VersionVK(1, 1))
        {
            features = renderer.VK.GetPhysicalDeviceFeatures(device);
        }
        else
        {
            PhysicalDeviceFeatures2 dFeatures = new PhysicalDeviceFeatures2(StructureType.PhysicalDeviceFeatures2);
            renderer.VK.GetPhysicalDeviceFeatures2(device, &dFeatures);
            features = dFeatures.Features;
        }

        ref PhysicalDeviceLimits limits = ref properties.Limits;
        GraphicsCapabilities cap = new GraphicsCapabilities();

        cap.Flags = GraphicsCapabilityFlags.DepthBoundsTesting;
        cap.Flags |= features.ImageCubeArray ? GraphicsCapabilityFlags.TextureCubeArrays : GraphicsCapabilityFlags.None;
        cap.Flags |= features.LogicOp ? GraphicsCapabilityFlags.BlendLogicOp : GraphicsCapabilityFlags.None;

        cap.MaxTexture1DSize = limits.MaxImageDimension1D;
        cap.MaxTexture2DSize = limits.MaxImageDimension2D;
        cap.MaxTexture3DSize = limits.MaxImageDimension3D;
        cap.MaxTextureCubeSize = limits.MaxImageDimensionCube;
        cap.MaxAnisotropy = features.SamplerAnisotropy ? limits.MaxSamplerAnisotropy : 0;
        cap.MaxTextureArraySlices = limits.MaxImageArrayLayers;
        cap.PixelShader.MaxOutputTargets = limits.MaxFragmentOutputAttachments;
        cap.MaxAllocatedSamplers = limits.MaxSamplerAllocationCount;

        uint variant, major, minor, patch;
        UnpackVersion(properties.ApiVersion, out variant, out major, out minor, out patch);

        cap.ApiVersion = $"{variant}.{major}.{minor}.{patch}";
        if (major == 1)
        {
            minor = Math.Min(minor, 3); // NOTE: Vulkan 1.3 is the highest we support currently.
            cap.Api = GraphicsApi.Vulkan1_0 + (int)minor;
        }
        else if (major > 1)
        {
            cap.Api = GraphicsApi.Vulkan1_3; // For now, default to the highest known vulkan version.
        }
        else
        {
            cap.Api = GraphicsApi.Unsupported;
        }

        cap.VertexShader.Flags |= ShaderCapabilityFlags.IsSupported;
        cap.PixelShader.Flags |= ShaderCapabilityFlags.IsSupported;
        cap.Compute.Flags |= ShaderCapabilityFlags.IsSupported;
        cap.Flags |= GraphicsCapabilityFlags.ConcurrentResourceCreation;
        cap.GeometryShader.Flags |= features.GeometryShader.ToCapFlag(ShaderCapabilityFlags.IsSupported);
        cap.HullShader.Flags |= features.TessellationShader.ToCapFlag(ShaderCapabilityFlags.IsSupported);
        cap.DomainShader.Flags |= features.TessellationShader.ToCapFlag(ShaderCapabilityFlags.IsSupported);

        if (features.ShaderFloat64)
            cap.AddShaderCap(ShaderCapabilityFlags.Float64);

        if(features.ShaderInt16)
            cap.AddShaderCap(ShaderCapabilityFlags.Int16);

        if(features.ShaderInt64)
            cap.AddShaderCap(ShaderCapabilityFlags.Int64);

        cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxInResources), limits.MaxPerStageResources);

        cap.VertexBuffers.MaxSlots = limits.MaxVertexInputBindings;
        cap.VertexBuffers.MaxElementStride = limits.MaxVertexInputBindingStride;

        cap.ConstantBuffers.MaxSlots = limits.MaxPerStageDescriptorUniformBuffers;
        cap.ConstantBuffers.MaxBytes = limits.MaxUniformBufferRange;

        cap.UnorderedAccessBuffers.MaxSlots = limits.MaxPerStageDescriptorStorageBuffers;
        cap.UnorderedAccessBuffers.MaxBytes = limits.MaxStorageBufferRange;

        cap.StructuredBuffers.MaxSlots = limits.MaxPerStageDescriptorStorageBuffers;
        cap.StructuredBuffers.MaxBytes = limits.MaxStorageBufferRange;

        // See: maxComputeWorkGroupCount[3] - https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkPhysicalDeviceLimits.html
        cap.Compute.MaxGroupCountX = limits.MaxComputeWorkGroupCount[0];
        cap.Compute.MaxGroupCountY = limits.MaxComputeWorkGroupCount[1];
        cap.Compute.MaxGroupCountZ = limits.MaxComputeWorkGroupCount[2];

        // See: maxComputeWorkGroupSize[3] -  https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkPhysicalDeviceLimits.html
        cap.Compute.MaxGroupSizeX = limits.MaxComputeWorkGroupSize[0];
        cap.Compute.MaxGroupSizeY = limits.MaxComputeWorkGroupSize[1];
        cap.Compute.MaxGroupSizeZ = limits.MaxComputeWorkGroupSize[2];

        QueueFamilyProperties[] queueFamilies = renderer.Enumerate<QueueFamilyProperties>((count, items) =>
        {
            renderer.VK.GetPhysicalDeviceQueueFamilyProperties(device, count, items);
            return Result.Success;
        }, "queue families");

        /// Command list functionality.
        cap.DeferredCommandLists = CommandListSupport.Supported;

        for(uint famIndex = 0; famIndex < queueFamilies.Length; famIndex++)
        { 
            ref QueueFamilyProperties p = ref queueFamilies[famIndex];

            cap.CommandSets.Add(new SupportedCommandSet()
            {
                MaxQueueCount = p.QueueCount,
                CapabilityFlags = (CommandSetCapabilityFlags)p.QueueFlags,
                TimeStampBits = p.TimestampValidBits,
            });
        }

        PopulateMemoryProperties(device, cap);
        return cap;
    }

    private void PopulateMemoryProperties(DeviceVK device, GraphicsCapabilities cap)
    {
        for(uint i = 0; i < device.Memory.HeapCount; i++)
        {
            MemoryHeapVK heap = device.Memory[i];
            double heapSize = ByteMath.ToMegabytes(heap.Size);

            if (heap.HasFlags(MemoryHeapFlags.DeviceLocalBit))
                cap.DedicatedVideoMemory += heapSize;
            else
                cap.SharedVideoMemory += heapSize;
        }
    }

    private void UnpackVersion(uint value, out uint variant, out uint major, out uint minor, out uint patch)
    {
        variant = (value >> 29);
        major = (value >> 22) & 0x7FU;
        minor = (value >> 12) & 0x3FFU;
        patch = value & 0xFFFU;
    }

    internal unsafe void LogAdditionalProperties(Logger log, PhysicalDeviceProperties2* properties)
    {
        PropertiesRef* pRef = (PropertiesRef*)properties;
        while(pRef->PNext != null)
        {
            pRef = (PropertiesRef*)pRef->PNext;
            log.WriteLine($"Addtional device properties found: {pRef->SType}");
        }
    }
}
