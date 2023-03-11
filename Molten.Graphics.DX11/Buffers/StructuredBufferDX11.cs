using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class StructuredBufferDX11<T> : GraphicsBuffer, IStructuredBuffer
        where T : unmanaged
    {
        /// <summary>Creates a new instance of <see cref="StagingBuffer"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="mode"></param>
        /// <param name="numElements">The maximum number of elements that the buffer can store</param>
        /// <param name="shaderResource"></param>
        /// <param name="unorderedAccess"></param>
        internal unsafe StructuredBufferDX11(DeviceDX11 device, BufferMode mode, uint numElements, bool unorderedAccess, bool shaderResource, T[] initialData = null)
            : base(device,
                  BufferMode.Default,
                  (shaderResource ? BindFlag.ShaderResource : BindFlag.None) | (unorderedAccess ? BindFlag.UnorderedAccess : BindFlag.None),
                  (uint)sizeof(T),
                  numElements,
                  ResourceMiscFlag.BufferStructured,
                  StagingBufferFlags.None,
                  initialData)
        {
            
        }        
    }
}
