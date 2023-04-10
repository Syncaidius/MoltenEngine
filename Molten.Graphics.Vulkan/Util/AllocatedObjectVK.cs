namespace Molten.Graphics.Vulkan
{
    public unsafe abstract class AllocatedObjectVK<T> : EngineObject
        where T : unmanaged
    {
        T* _ptr;

        internal AllocatedObjectVK()
        {
            _ptr = EngineUtil.Alloc<T>();
        }

        protected override void OnDispose()
        {
            EngineUtil.Free(ref _ptr);
        }

        /// <summary>
        /// Gets the underlying pointer of the <see cref="AllocatedObjectVK{T}"/>.
        /// </summary>
        internal T* Ptr => _ptr;
    }
}
