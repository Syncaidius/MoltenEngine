using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal class CapabilityBuilder
    {
        internal unsafe GraphicsCapabilities Build(ref PhysicalDeviceLimits limits, ref PhysicalDeviceFeatures features)
        {
            GraphicsCapabilities cap = new GraphicsCapabilities();

            cap.MaxTexture1DSize = limits.MaxImageDimension1D;
            cap.MaxTexture2DSize = limits.MaxImageDimension2D;
            cap.MaxTexture3DSize = limits.MaxImageDimension3D;
            cap.MaxTextureCubeSize = limits.MaxImageDimensionCube;
            cap.MaxAnisotropy = features.SamplerAnisotropy ? limits.MaxSamplerAnisotropy : 0;
            cap.MaxTextureArraySlices = limits.MaxImageArrayLayers;
            cap.TextureCubeArrays = features.ImageCubeArray;
            cap.PixelShader.MaxOutResources = limits.MaxFragmentOutputAttachments;
            cap.BlendLogicOp = features.LogicOp;
            cap.MaxAllocatedSamplers = limits.MaxSamplerAllocationCount;

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

            return cap;
        }
    }
}
