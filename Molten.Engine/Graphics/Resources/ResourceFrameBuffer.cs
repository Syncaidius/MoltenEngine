namespace Molten.Graphics;

/// <summary>
/// Creates multiple instances a resource, one for each buffered frame.
/// </summary>
/// <typeparam name="T">The type of resource to be buffered.</typeparam>
public class ResourceFrameBuffer<T> : GraphicsObject
    where T : GraphicsResource
{
    public delegate T CreateResourceCallback(GraphicsDevice device);

    CreateResourceCallback _createCallback;
    ulong _lastFrameResized;
    uint _knownFrameBufferSize;
    uint _requestedFrameBufferSize;
    T[] _resources;

    public ResourceFrameBuffer(GraphicsDevice device, CreateResourceCallback createCallback) : 
        base(device)
    {
        _createCallback = createCallback;
        Device_OnFrameBufferSizeChanged(0, device.FrameBufferSize);
        device.OnFrameBufferSizeChanged += Device_OnFrameBufferSizeChanged;
        Prepare();
    }

    private void Device_OnFrameBufferSizeChanged(uint oldSize, uint newSize)
    {
        _requestedFrameBufferSize = newSize;
    }

    /// <summary>
    /// Prepares the current frame-buffered resource and returns the correct <see cref="GraphicsResource"/> instance to use.
    /// </summary>
    /// <returns></returns>
    public T Prepare()
    {
        if (_requestedFrameBufferSize != _knownFrameBufferSize)
        {
            if (_resources == null || _resources.Length < _requestedFrameBufferSize)
            {
                T[] newResources = new T[_requestedFrameBufferSize];

                // Create any new resource instances if the array has grown.
                for(uint i = _knownFrameBufferSize; i < _requestedFrameBufferSize; i++)
                    newResources[i] = _createCallback(Device);

                _resources = newResources;
            }

            _lastFrameResized = Device.Renderer.FrameID;
            _knownFrameBufferSize = _requestedFrameBufferSize;
        }


        // Dispose of any extra resource instances, if safe to do so.
        // We wait until we have surpassed the minimum frame-age threshold before disposing of any leftover resources.
        if (_resources.Length > Device.FrameBufferSize)
        {
            ulong resizeAge = Device.Renderer.FrameID - _lastFrameResized;

            if (resizeAge > Device.FrameBufferSize)
            {
                for (uint i = Device.FrameBufferSize; i < _resources.Length; i++)
                    _resources[i]?.Dispose();

                Array.Resize(ref _resources, (int)Device.FrameBufferSize);
            }
        }

        return _resources[Device.FrameBufferIndex];
    }

    protected override void OnGraphicsRelease()
    {
        for (int i = 0; i < _resources.Length; i++)
            _resources[i]?.Dispose(true);

        _resources = null;
    }
}
