using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal class CapabilityBuilder
    {
        internal unsafe struct PropertiesRef
        {
            public StructureType SType;

            public void* PNext;
        }

        internal unsafe GraphicsCapabilities Build(DeviceVK device, RendererVK renderer, ref PhysicalDeviceProperties2 properties, ref PhysicalDeviceMemoryProperties2 mem)
        {
            PhysicalDeviceFeatures2 dFeatures = new PhysicalDeviceFeatures2(StructureType.PhysicalDeviceFeatures2);
            renderer.VK.GetPhysicalDeviceFeatures2(device, &dFeatures);
            PhysicalDeviceFeatures features = dFeatures.Features;
            ref PhysicalDeviceLimits limits = ref properties.Properties.Limits;
            GraphicsCapabilities cap = new GraphicsCapabilities();

            cap.MaxTexture1DSize = limits.MaxImageDimension1D;
            cap.MaxTexture2DSize = limits.MaxImageDimension2D;
            cap.MaxTexture3DSize = limits.MaxImageDimension3D;
            cap.MaxTextureCubeSize = limits.MaxImageDimensionCube;
            cap.DepthBoundsTesting = true;
            cap.MaxAnisotropy = features.SamplerAnisotropy ? limits.MaxSamplerAnisotropy : 0;
            cap.MaxTextureArraySlices = limits.MaxImageArrayLayers;
            cap.TextureCubeArrays = features.ImageCubeArray;
            cap.PixelShader.MaxOutputTargets = limits.MaxFragmentOutputAttachments;
            cap.BlendLogicOp = features.LogicOp;
            cap.MaxAllocatedSamplers = limits.MaxSamplerAllocationCount;

            GetMemoryProperties(device, ref mem);

            uint variant, major, minor, patch;
            UnpackVersion(properties.Properties.ApiVersion, out variant, out major, out minor, out patch);

            cap.ApiVersion = $"{variant}.{major}.{minor}.{patch}";
            if (major == 1)
                cap.Api = GraphicsApi.Vulkan1_0 + (int)minor;
            else if (major > 1)
                cap.Api = GraphicsApi.Vulkan1_3; // For now, default to the highest known vulkan version.
            else
                cap.Api = GraphicsApi.Unsupported;

            cap.SetShaderCap(nameof(ShaderStageCapabilities.Float64), features.ShaderFloat64);
            cap.SetShaderCap(nameof(ShaderStageCapabilities.Int16), features.ShaderInt16);
            cap.SetShaderCap(nameof(ShaderStageCapabilities.Int64), features.ShaderInt64);
            cap.SetShaderCap(nameof(ShaderStageCapabilities.IsSupported), true);
            cap.SetShaderCap(nameof(ShaderStageCapabilities.MaxInResources), limits.MaxPerStageResources);

            cap.GeometryShader.IsSupported =  features.GeometryShader;
            cap.HullShader.IsSupported = features.TessellationShader;
            cap.DomainShader.IsSupported = features.TessellationShader;

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
            cap.ConcurrentResourceCreation = true;

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

            return cap;
        }

        private void GetMemoryProperties(DeviceVK device, ref PhysicalDeviceMemoryProperties2 mem)
        {
            Dictionary<uint, MemoryPropertyFlags> heapFlags = new Dictionary<uint, MemoryPropertyFlags>();

            // Search here for device spec references: http://vulkan.gpuinfo.org/listdevices.php
            for (int i = 0; i < mem.MemoryProperties.MemoryTypeCount; i++)
            {
                ref MemoryType mType = ref mem.MemoryProperties.MemoryTypes[i];
                if (!heapFlags.ContainsKey(mType.HeapIndex))
                    heapFlags[mType.HeapIndex] = mType.PropertyFlags;
                else
                    heapFlags[mType.HeapIndex] |= mType.PropertyFlags; 
            }

            foreach(uint heapIndex in heapFlags.Keys)
            {
                ref MemoryHeap mHeap = ref mem.MemoryProperties.MemoryHeaps[(int)heapIndex];
                MemoryPropertyFlags flags = heapFlags[heapIndex];

                double heapSize = ByteMath.ToMegabytes(mHeap.Size);

                if ((flags & MemoryPropertyFlags.DeviceLocalBit) == MemoryPropertyFlags.DeviceLocalBit)
                    device.Capabilities.DedicatedVideoMemory += heapSize;

                MemoryPropertyFlags hostFlags = MemoryPropertyFlags.HostCoherentBit | MemoryPropertyFlags.HostVisibleBit;
                if ((flags & hostFlags) == hostFlags)
                    device.Capabilities.DedicatedSystemMemory += heapSize;

                MemoryPropertyFlags sharedFlags = hostFlags | MemoryPropertyFlags.DeviceLocalBit;
                if ((flags & sharedFlags) == sharedFlags)
                    device.Capabilities.SharedVideoMemory += heapSize;
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
}
