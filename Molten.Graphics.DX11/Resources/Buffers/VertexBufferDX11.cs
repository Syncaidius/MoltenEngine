using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public abstract unsafe class VertexBufferDX11 : BufferDX11
    {
        protected VertexBufferDX11(DeviceDX11 device, GraphicsResourceFlags mode, uint stride, uint numElements, void* initialData, uint initialBytes) :
            base(device, GraphicsBufferType.Vertex, mode, stride, numElements, initialData, initialBytes)
        {
            
        }
    }

    public unsafe class VertexBufferDX11<T> : VertexBufferDX11
        where T : unmanaged, IVertexType
    {
        public unsafe VertexBufferDX11(DeviceDX11 device, GraphicsResourceFlags mode, uint numElements, void* initialData, uint initialBytes) : 
            base(device, mode, (uint)sizeof(T), numElements, initialData, initialBytes)
        {
            VertexFormat = device.VertexFormatCache.Get<T>();
        }
    }
}
