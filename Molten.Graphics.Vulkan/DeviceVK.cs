using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Queue = Silk.NET.Vulkan.Queue;

namespace Molten.Graphics
{
    internal unsafe class DeviceVK : GraphicsDevice
    {
        public event DisplayOutputChanged OnOutputActivated;
        public event DisplayOutputChanged OnOutputDeactivated;

        DisplayManagerVK _manager;
        List<DisplayOutputVK> _outputs;
        List<DisplayOutputVK> _activeOutputs;

        PhysicalDeviceMemoryProperties2 _memProperties;

        Instance* _vkInstance;
        RendererVK _renderer;
        List<CommandQueueVK> _queues;
        CommandQueueVK _gfxQueue;
        DeviceLoaderVK _loader;
        MemoryManagerVK _memory;
        Device* _native;


        Stack<FenceVK> _freeFences;
        List<FenceVK> _fences;

        internal readonly Fence NullFence = new Fence(0);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="adapter"></param>
        /// <param name="instance"></param>
        internal DeviceVK(RendererVK renderer, DisplayManagerVK manager, PhysicalDevice pDevice, Instance* instance) :
            base(renderer, manager)
        {
            _freeFences = new Stack<FenceVK>();
            _fences = new List<FenceVK>();

            _renderer = renderer;
            _vkInstance = instance;
            _manager = manager;
            Adapter = pDevice;
            _memory = new MemoryManagerVK(this);

            PhysicalDeviceProperties2 p = new PhysicalDeviceProperties2(StructureType.PhysicalDeviceProperties2);
            _manager.Renderer.VK.GetPhysicalDeviceProperties2(Adapter, &p);

            Name = SilkMarshal.PtrToString((nint)p.Properties.DeviceName, NativeStringEncoding.UTF8);
            ID = ParseDeviceID(p.Properties.DeviceID);
            Vendor = ParseVendorID(p.Properties.VendorID);
            Type = (GraphicsDeviceType)p.Properties.DeviceType;

            Capabilities = _manager.CapBuilder.Build(this, _manager.Renderer, ref p);

#if DEBUG
            _manager.CapBuilder.LogAdditionalProperties(_manager.Renderer.Log, &p);
#endif

            _outputs = new List<DisplayOutputVK>();
            _activeOutputs = new List<DisplayOutputVK>();
            Outputs = _outputs.AsReadOnly();
            ActiveOutputs = _activeOutputs.AsReadOnly();
        }

        internal void Initialize(CommandSetCapabilityFlags capFlags)
        {
            _native = EngineUtil.Alloc<Device>();
            _queues = new List<CommandQueueVK>();
            _loader = new DeviceLoaderVK(_renderer, this, capFlags);
        }

        private DeviceVendor ParseVendorID(uint vendorID)
        {
            // From docs: If the vendor has a PCI vendor ID, the low 16 bits of vendorID must contain that PCI vendor ID, and the remaining bits must be set to zero. 
            if ((vendorID & 0xFFFF0000) == 0)
                return EngineUtil.VendorFromPCI(vendorID & 0x0000FFFF); // PCI vendor ID
            else
                return (DeviceVendor)(vendorID & 0xFFFF0000); // Vulkan Vendor ID
        }

        private DeviceID ParseDeviceID(uint deviceID)
        {
            // Docs: The vendor is also responsible for the value returned in deviceID. If the implementation is driven primarily by a PCI device with a PCI device ID,
            //      the low 16 bits of deviceID must contain that PCI device ID, and the remaining bits must be set to zero. 
            if ((deviceID & 0xFFFF0000) == 0)
                return new DeviceID((deviceID & 0x0000FFFF)); // PCI device ID.
            else
                return new DeviceID(deviceID); // OS/Platform-based device ID.
        }

        internal bool AssociateOutput(DisplayOutputVK output)
        {
            if (output.AssociatedDevice == this)
                return true;

            output.AssociatedDevice?.UnassociateOutput(output);

            _outputs.Add(output);
            output.AssociatedDevice = this;

            return false;
        }

        internal void UnassociateOutput(DisplayOutputVK output)
        {
            if (output.AssociatedDevice != this)
                return;

            int index = _activeOutputs.IndexOf(output);
            if (index > -1)
                _activeOutputs.RemoveAt(index);

            _outputs.Remove(output);
            output.AssociatedDevice = null;
        }

        /// <inheritdoc/>
        public override void AddActiveOutput(IDisplayOutput output)
        {
            if (output is DisplayOutputVK vkOutput)
            {
                if (vkOutput.AssociatedDevice != this)
                    return;

                if (!_activeOutputs.Contains(vkOutput))
                    _activeOutputs.Add(vkOutput);
            }
        }

        /// <inheritdoc/>
        public override void RemoveActiveOutput(IDisplayOutput output)
        {
            if (output is DisplayOutputVK vkOutput)
            {
                if (vkOutput.AssociatedDevice != this)
                    return;

                _activeOutputs.Remove(vkOutput);
            }
        }

        /// <inheritdoc/>
        public override void RemoveAllActiveOutputs()
        {
            throw new NotImplementedException();
        }

        internal void AddExtension<E>(Action<E> loadCallback = null, Action<E> destroyCallback = null)
            where E : NativeExtension<Vk>
        {
            _loader.AddExtension(loadCallback, destroyCallback);
        }

        internal E GetExtension<E>()
            where E : NativeExtension<Vk>
        {
            return _loader.GetExtension<E>();
        }

        internal bool Initialize()
        {
            if (_loader.Build(_renderer.ApiVersion, Ptr))
            {
                for (int i = 0; i < _loader.QueueCount; i++)
                {
                    ref DeviceQueueCreateInfo qi = ref _loader.QueueInfo[i];

                    for (uint index = 0; index < qi.QueueCount; index++)
                    {
                        Queue q = new Queue();
                        _renderer.VK.GetDeviceQueue(*Ptr, qi.QueueFamilyIndex, index, &q);
                        SupportedCommandSet set = Capabilities.CommandSets[(int)qi.QueueFamilyIndex];
                        CommandQueueVK queue = new CommandQueueVK(_renderer, this, qi.QueueFamilyIndex, q, index, set);
                        _queues.Add(queue);

                        // TODO maybe find the best queue, rather than first match?
                        if (_gfxQueue == null && queue.HasFlags(CommandSetCapabilityFlags.Graphics))
                            _gfxQueue = queue;

                        _renderer.Log.Write($"Instantiated command queue -- Family: {qi.QueueFamilyIndex} -- Index: {index} -- Flags: {set.CapabilityFlags}");
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds a <see cref="CommandQueueVK"/> that can present the provided <see cref="WindowSurfaceVK"/>.
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        internal CommandQueueVK FindPresentQueue(WindowSurfaceVK surface)
        {
            KhrSurface extSurface = _renderer.GetInstanceExtension<KhrSurface>();
            Bool32 presentSupported = false;

            foreach (CommandQueueVK queue in _queues)
            {
                Result r = extSurface.GetPhysicalDeviceSurfaceSupport(Adapter, queue.FamilyIndex, surface.Native, &presentSupported);
                if (r.Check(_renderer) && presentSupported)
                    return queue;
            }

            return null;
        }

        /// <summary>
        /// Retrieves the <see cref="SharingMode"/> for a resource, based on <see cref="CommandQueueVK"/> queues that may potentionally access it.
        /// </summary>
        /// <param name="expectedQueues">The <see cref="CommandQueueVK"/> queues that are expected to share.</param>
        /// <returns></returns>
        internal (SharingMode, CommandQueueVK[]) GetSharingMode(params CommandQueueVK[] expectedQueues)
        {
            HashSet<CommandQueueVK> set = new HashSet<CommandQueueVK>();
            for (int i = 0; i < expectedQueues.Length; i++)
                set.Add(expectedQueues[i]);

            if (set.Count <= 1)
                return (SharingMode.Exclusive, set.ToArray());
            else
                return (SharingMode.Concurrent, set.ToArray());
        }

        protected override void OnDispose()
        {
            // Dispose of fences
            for(int i = 0; i < _fences.Count; i++)
                _fences[i].Dispose();

            _fences.Clear();
            while(_freeFences.Count > 0)
                _freeFences.Pop().Dispose();   

            _loader.Dispose();
            _renderer.VK.DestroyDevice(*Ptr, null);
            EngineUtil.Free(ref _native);

            base.OnDispose();
        }

        internal FenceVK GetFence(FenceCreateFlags flags = FenceCreateFlags.None)
        {
            if (_freeFences.Count > 0)
                return _freeFences.Pop();

            FenceVK fence = new FenceVK(this, flags);
            _fences.Add(fence);
            return fence;
        }

        internal void ProcessFences()
        {
            Span<Fence> f = stackalloc Fence[1];

            for (int i = _fences.Count -1; i >= 0; i--)
            {
                FenceVK fence = _fences[i];
                if (fence.CheckStatus())
                {
                    f[0] = fence.Ptr;
                    VK.ResetFences(*_native, f);
                    _fences.RemoveAt(i);
                    _freeFences.Push(fence);
                }
            }
        }

        protected override HlslPass OnCreateShaderPass(HlslShader shader, string name)
        {
            return new MaterialPassVK(shader, name);
        }

        protected override ShaderSampler OnCreateSampler(ref ShaderSamplerParameters parameters)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBuffer CreateVertexBuffer<T>(GraphicsResourceFlags mode, uint numVertices, T[] initialData = null)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags mode, uint numIndices, ushort[] initialData = null)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags mode, uint numIndices, uint[] initialData = null)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBuffer CreateStructuredBuffer<T>(GraphicsResourceFlags flags, uint numElements, T[] initialData = null)
        {
            uint stride = (uint)sizeof(T);
            uint totalBytes = stride * numElements;
            fixed(T* ptrData = initialData)
                return new BufferVK(this, GraphicsBufferType.Structured, flags, BufferUsageFlags.None, stride, numElements, ptrData, totalBytes);
        }

        public override GraphicsBuffer CreateStagingBuffer(bool allowCpuRead, bool allowCpuWrite, uint byteCapacity)
        {
            return new BufferVK(this, GraphicsBufferType.Staging, GraphicsResourceFlags.CpuRead | GraphicsResourceFlags.CpuWrite | GraphicsResourceFlags.GpuWrite,
                BufferUsageFlags.None, 1, byteCapacity, null, 0);
        }

        public override INativeSurface CreateControlSurface(string controlTitle, string controlName, uint mipCount = 1)
        {
            throw new NotImplementedException();
        }

        public override IDepthStencilSurface CreateDepthSurface(uint width, uint height,
            DepthFormat format = DepthFormat.R24G8_Typeless, 
            GraphicsResourceFlags flags = GraphicsResourceFlags.None | GraphicsResourceFlags.GpuWrite, 
            uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override IRenderSurface2D CreateSurface(uint width, uint height, 
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm, 
            GraphicsResourceFlags flags = GraphicsResourceFlags.None | GraphicsResourceFlags.GpuWrite, 
            uint mipCount = 1, uint arraySize = 1, 
            AntiAliasLevel aaLevel = AntiAliasLevel.None, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override INativeSurface CreateFormSurface(string formTitle, string formName, uint mipCount = 1)
        {
            return new WindowSurfaceVK(_renderer.NativeDevice, GraphicsFormat.B8G8R8A8_UNorm, formTitle, 1024, 800);
        }

        public override void ResolveTexture(ITexture source, ITexture destination)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTexture(ITexture source, ITexture destination, uint sourceMipLevel, uint sourceArraySlice, uint destMiplevel, uint destArraySlice)
        {
            throw new NotImplementedException();
        }

        public override ITexture CreateTexture1D(Texture1DProperties properties, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture CreateTexture1D(TextureData data, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture2D CreateTexture2D(Texture2DProperties properties, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture2D CreateTexture2D(TextureData data, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture3D CreateTexture3D(Texture3DProperties properties, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITexture3D CreateTexture3D(TextureData data, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITextureCube CreateTextureCube(Texture2DProperties properties, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public override ITextureCube CreateTextureCube(TextureData data, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PhysicalDevice(DeviceVK device)
        {
            return device.Adapter;
        }

        public static implicit operator Device(DeviceVK device)
        {
            return *device._native;
        }

        /// <inheritdoc/>
        public override DeviceID ID { get; }

        /// <inheritdoc/>
        public override DeviceVendor Vendor { get; }

        /// <inheritdoc/>
        public override GraphicsDeviceType Type { get; }

        /// <inheritdoc/>
        public override IReadOnlyList<IDisplayOutput> Outputs { get; }

        /// <inheritdoc/>
        public override IReadOnlyList<IDisplayOutput> ActiveOutputs { get; }

        internal PhysicalDevice Adapter { get; private set; }

        internal ref PhysicalDeviceMemoryProperties2 MemoryProperties => ref _memProperties;

        /// <summary>
        /// Gets the <see cref="Instance"/> that the current <see cref="DeviceVK"/> is bound to.
        /// </summary>
        internal Instance* Instance => _vkInstance;

        /// <summary>
        /// The underlying, native device pointer.
        /// </summary>
        internal Device* Ptr => _native;

        /// <summary>
        /// Gets a protected reference to the underlying device pointer.
        /// </summary>
        protected ref Device* PtrRef => ref _native;

        /// <summary>
        /// Gets the underlying <see cref="CommandQueueVK"/> that should execute graphics commands.
        /// </summary>
        public override CommandQueueVK Queue => _gfxQueue;

        internal Vk VK => _renderer.VK;

        internal Glfw GLFW => _renderer.GLFW;

        internal MemoryManagerVK Memory => _memory;
    }
}
