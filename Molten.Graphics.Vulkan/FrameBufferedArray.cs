namespace Molten.Graphics.Vulkan
{
    /// <summary>
    /// An array which allocates an array for each expected renderer frame-in-flight.
    /// </summary>
    /// <typeparam name="T">The type of array to create.</typeparam>
    internal unsafe class FrameBufferedArray<T> : IDisposable
        where T : unmanaged
    {
        T*[] _arrays;
        T* _curArray;
        uint _curIndex;

        internal FrameBufferedArray(DeviceVK device, uint arraySize)
        {
            _arrays = new T*[device.FrameBufferSize];
            for (uint i = 0; i < _arrays.Length; i++)
                _arrays[i] = EngineUtil.AllocArray<T>(arraySize);

            _curArray = _arrays[device.FrameBufferIndex];
        }

        internal void SetFrame(DeviceVK device)
        {
            // Check if the array needs resizing to match a change in device frame buffer size.
            if(_arrays.Length < device.FrameBufferSize)
            {
                T*[] newArray = new T*[device.FrameBufferSize];
                uint copyCount = Math.Min(device.FrameBufferSize, (uint)_arrays.Length);

                // Copy existing values.
                for (uint i = 0; i < copyCount; i++)
                    newArray[i] = _arrays[i];

                // TODO - Invoke OnReleaseIndex event on each extra index, if frame buffer size decreased.
                // TODO - Invoke OnAllocIndex event on each extra index, if frame buffer size increased.
                throw new NotImplementedException();
            }

            // Switch to next array.
            _curArray = _arrays[device.FrameBufferIndex];
        }

        ~FrameBufferedArray()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            for (int i = 0; i < _arrays.Length; i++)
                EngineUtil.Free(ref _arrays[i]);

            _arrays = null;

            IsDisposed = true;
        }

        public static implicit operator T* (FrameBufferedArray<T> array)
        {
            return array._arrays[array._curIndex];
        }

        public ref T this[int index] => ref _curArray[index];

        public ref T this[uint index] => ref _curArray[index];

        public bool IsDisposed { get; private set; }
    }
}
