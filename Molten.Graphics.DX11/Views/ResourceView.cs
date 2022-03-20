using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="V">Underlying view type.</typeparam>
    /// <typeparam name="D">Underlying view description type.</typeparam>
    internal unsafe abstract class ResourceView<V, D>
        where V : unmanaged
        where D : unmanaged
    {
        V* _native;
        D _desc;

        internal ResourceView(Device device)
        {
            Device = device;
        }

        internal unsafe V* Ptr => _native;

        internal ref D Desc => ref _desc;

        internal Device Device { get; }

        internal void Create(ID3D11Resource* resource)
        {
            SilkUtil.ReleasePtr(ref _native);
            OnCreateView(resource, ref _desc, ref _native);
        }

        protected abstract void OnCreateView(ID3D11Resource* resource, ref D desc, ref V* view);

        internal void Release()
        {
            SilkUtil.ReleasePtr(ref _native);
        }
    }
}
