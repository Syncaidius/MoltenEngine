using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Queue = Silk.NET.Vulkan.Queue;

namespace Molten.Graphics
{
    internal unsafe class DeviceVK : GraphicsDevice<Device>
    {
        Instance* _vkInstance;
        RendererVK _renderer;
        List<CommandQueueVK> _queues;
        CommandQueueVK _gfxQueue;
        DeviceLoaderVK _loader;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="adapter"></param>
        /// <param name="instance"></param>
        /// <param name="requiredCap">Required capabilities</param>
        internal DeviceVK(RendererVK renderer, DisplayAdapterVK adapter, Instance* instance, CommandSetCapabilityFlags requiredCap) :
            base(renderer.Settings.Graphics, renderer.Log, true)
        {
            _queues = new List<CommandQueueVK>();
            _renderer = renderer;
            _vkInstance = instance;
            Adapter = adapter;
            _loader = new DeviceLoaderVK(renderer, adapter, requiredCap);
        }

        protected override void OnInitialize()
        {
            
        }

        public override IGraphicsBuffer CreateBuffer(GraphicsBufferFlags flags, BufferMode mode, uint byteCapacity, uint stride = 0)
        {
            throw new NotImplementedException();
        }

        public override IStagingBuffer CreateStagingBuffer(StagingBufferFlags staging, uint byteCapacity)
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

        public override MaterialPass CreateMaterialPass(Material material, string name = null)
        {
            return new MaterialPassVK(material, name);
        }

        protected override GraphicsSampler OnCreateSampler(ref GraphicsSamplerParameters parameters)
        {
            throw new NotImplementedException();
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
                        SupportedCommandSet set = Adapter.Capabilities.CommandSets[(int)qi.QueueFamilyIndex];
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
                Result r = extSurface.GetPhysicalDeviceSurfaceSupport(Adapter.Native, queue.FamilyIndex, surface.Native, &presentSupported);
                if (_renderer.CheckResult(r) && presentSupported)
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
            _loader.Dispose();
            _renderer.VK.DestroyDevice(*Ptr, null);

            base.OnDispose();
        }

        /// <summary>
        /// Gets the underlying <see cref="DisplayAdapterVK"/> that the current <see cref="DeviceVK"/> is bound to.
        /// </summary>
        public override DisplayAdapterVK Adapter { get; }

        public override GraphicsDisplayManager DisplayManager => _renderer.DisplayManager;

        /// <summary>
        /// Gets the <see cref="Instance"/> that the current <see cref="DeviceVK"/> is bound to.
        /// </summary>
        internal Instance* Instance => _vkInstance;

        /// <summary>
        /// Gets the underlying <see cref="CommandQueueVK"/> that should execute graphics commands.
        /// </summary>
        public override CommandQueueVK Cmd => _gfxQueue;

        internal Vk VK => _renderer.VK;

        internal Glfw GLFW => _renderer.GLFW;

        internal RendererVK Renderer => _renderer;
    }
}
