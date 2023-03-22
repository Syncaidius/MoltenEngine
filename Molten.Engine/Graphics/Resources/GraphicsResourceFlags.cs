namespace Molten.Graphics
{
    [Flags]
    public enum GraphicsResourceFlags
    {
        /// <summary>
        /// No flags. This is usually invalid.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allow the CPU to read from the resource. Generally, only staging resources allow the CPU to map and read them.
        /// </summary>
        CpuRead = 1,

        /// <summary>
        /// Allow the CPU to write to the resource.
        /// </summary>
        CpuWrite = 1 << 1,

        /// <summary>
        /// Allow the GPU to read from the resource.
        /// </summary>
        GpuRead = 1 << 2,

        /// <summary>
        /// Allow the GPU to write to the resource.
        /// </summary>
        GpuWrite = 1 << 3,

        /// <summary>
        /// Each time data is uploaded to the resource, a new section of the resource's memory will be used. 
        /// Once the end of the resource has insufficient space, it will cycle back around to the start of the resource. This forms the ring.
        /// </summary>
        Ring = 1 << 4,

        /// <summary>
        /// Each data is uploaded to the resource, the current resource memory is discarded after the GPU has finished with it. New area of memory is allocated to accomodate the new data.
        /// </summary>
        Discard = 1 << 5,

        /// <summary>
        /// Allow unordered/storage access from supported shader stages.
        /// <para>These are UAV resources in DX11 and DX12.</para>
        /// <para>These are Shader Storage Objects (SSO) resources in OpenGL and Vulkan.</para>
        /// </summary>
        UnorderedAccess = 1 << 6,

        /// <summary>
        /// Do not allow shader access. For example in DX11 this would prevent a shader resource view (SRV) from being bound to the resource.
        /// </summary>
        NoShaderAccess = 1 << 7,

        /// <summary>
        /// Allows the resource to be shared between logical devices, such as ID3D11Device, ID3D12Device or vkDevice instances.
        /// </summary>
        Shared = 1 << 8,


        /// <summary>
        /// All of the GPU and CPU read/write flags.
        /// </summary>
        AllReadWrite = (CpuRead | CpuWrite | GpuRead | GpuWrite),
    }

    public static class ResourceFlagsExtensions
    {
        public static bool Has(this GraphicsResourceFlags flags, GraphicsResourceFlags value)
        {
            return (flags & value) == value;
        }

        public static bool IsImmutable(this GraphicsResourceFlags flags)
        {
            return (flags & GraphicsResourceFlags.GpuRead) == GraphicsResourceFlags.GpuRead &&
                (flags & GraphicsResourceFlags.GpuWrite) != GraphicsResourceFlags.GpuWrite &&
                (flags & GraphicsResourceFlags.CpuRead) != GraphicsResourceFlags.CpuRead &&
                (flags & GraphicsResourceFlags.CpuWrite) != GraphicsResourceFlags.CpuWrite;
        }
    }
}
