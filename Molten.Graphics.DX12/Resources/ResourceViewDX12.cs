namespace Molten.Graphics.DX12
{
    /// <summary>
    /// Represents a view for a DirectX 12 resource, such as a shader resource (SRV), unordered-access (UAV) resource or render-target (RTV).
    /// </summary>
    /// <typeparam name="D">Underlying view description type.</typeparam>
    internal class ResourceViewDX12<D>
        where D : unmanaged
    {
        D _desc;

        internal ResourceViewDX12(ResourceHandleDX12 handle)
        {
            Handle = handle;
        }

        internal ref D Desc => ref _desc;

        /// <summary>
        /// Gets the parent <see cref="ResourceHandleDX12"/>.
        /// </summary>
        internal ResourceHandleDX12 Handle { get; }
    }
}
