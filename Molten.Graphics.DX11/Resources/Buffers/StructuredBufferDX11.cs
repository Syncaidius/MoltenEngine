using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class StructuredBufferDX11<T> : BufferDX11
        where T : unmanaged
    {
        /// <summary>Creates a new instance of <see cref="StagingBufferDX11"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="flags"></param>
        /// <param name="numElements">The maximum number of elements that the buffer can store</param>
        /// <param name="shaderResource"></param>
        /// <param name="unorderedAccess"></param>
        internal unsafe StructuredBufferDX11(DeviceDX11 device, GraphicsResourceFlags flags, uint numElements, bool unorderedAccess, bool shaderResource, void* initialData = null)
            : base(device, 
                  GraphicsBufferType.StructuredBuffer,
                  flags,
                  (shaderResource ? BindFlag.ShaderResource : BindFlag.None) | (unorderedAccess ? BindFlag.UnorderedAccess : BindFlag.None),
                  (uint)sizeof(T),
                  numElements,
                  ResourceMiscFlag.BufferStructured,
                  initialData)
        {
            
        }        
    }
}
