using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    public abstract unsafe class VertexBufferDX11 : BufferDX11
    {
        protected VertexBufferDX11(DeviceDX11 device, GraphicsResourceFlags flags, uint stride, uint numElements, void* initialData, uint initialBytes) :
            base(device, GraphicsBufferType.Vertex, flags, GraphicsFormat.Unknown, stride, numElements, initialData, initialBytes)
        {
            
        }
    }

    public unsafe class VertexBufferDX11<T> : VertexBufferDX11
        where T : unmanaged, IVertexType
    {
        internal unsafe VertexBufferDX11(DeviceDX11 device, GraphicsResourceFlags mode, uint numElements, void* initialData, uint initialBytes) : 
            base(device, mode, (uint)sizeof(T), numElements, initialData, initialBytes)
        {
            VertexFormat = device.VertexFormatCache.Get<T>();
        }
    }
}
