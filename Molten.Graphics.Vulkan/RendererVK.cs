using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Molten.Graphics
{
    public unsafe class RendererVK : RenderService
    {
        internal unsafe delegate Result EnumerateCallback<T>(uint* count, T* info) where T : unmanaged;

        RenderChainVK _chain;
        ResourceFactoryVK _resFactory;
        DisplayManagerVK _displayManager;
        InstanceLoaderVK _instanceLoader;
        Instance* _instance;
        DebugUtilsMessengerEXT* _debugMessengerHandle;
        List<DeviceVK> _devices;

        public RendererVK()
        {
            VK = Vk.GetApi();
            GLFW = Glfw.GetApi();
            GLFW.Init();
            ApiVersion = new VersionVK(0, 1, 1, 0);

            _devices = new List<DeviceVK>();
            _instanceLoader = new InstanceLoaderVK(this);
            _displayManager = new DisplayManagerVK(this);
            _chain = new RenderChainVK(this);
            ApiVersion = new VersionVK(1, 1, 0);
        }

        /// <summary>
        /// An extended version of <see cref="Vk.MakeVersion(uint, uint, uint)"/> which includes the variant bits.
        /// </summary>
        /// <param name="variant"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="patch"></param>
        /// <returns></returns>
        internal static uint MakeVersion(uint variant, uint major, uint minor, uint patch)
        {
            return (((variant) << 29) | ((major) << 22) | ((minor) << 12) | (patch));
        }

        protected override void OnInitializeApi(GraphicsSettings settings)
        {
            // TODO Store baseline profiles for each OS/platform where possible, or default to Moltens own.
            // For android see: https://developer.android.com/ndk/guides/graphics/android-baseline-profile

            if (settings.EnableDebugLayer.Value == true)
            {
                _instanceLoader.AddLayer("VK_LAYER_KHRONOS_validation");
                _instanceLoader.AddExtension<ExtDebugUtils>(SetupDebugMessenger, (ext) =>
                {
                    // Dispose of debug messenger handle.
                    if (_debugMessengerHandle != null)
                    {
                        ext.DestroyDebugUtilsMessenger(*_instance, *_debugMessengerHandle, null);
                        _debugMessengerHandle = null;
                    }
                });
            }

            _instanceLoader.AddExtension<KhrSurface>();
            _instanceLoader.AddGlfwExtensions();

            _instance = EngineUtil.Alloc<Instance>();
            if (!_instanceLoader.Build(ApiVersion, _instance))
                Log.Error($"Failed to build new instance");
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate Bool32 DebugMessengerCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity,
            DebugUtilsMessageTypeFlagsEXT messageTypes,
            DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData);

        private void SetupDebugMessenger(ExtDebugUtils ext)
        {
            // See: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/PFN_vkDebugUtilsMessengerCallbackEXT.html
            DebugMessengerCallback debugMsgCallback = new DebugMessengerCallback((messageSeverity, messageTypes, pCallbackData, pUserData) =>
            {
                StructureType pCallbackType = pCallbackData->SType;
                string msg = SilkMarshal.PtrToString((nint)pCallbackData->PMessage, NativeStringEncoding.UTF8);
                string msgIDName = SilkMarshal.PtrToString((nint)pCallbackData->PMessageIdName, NativeStringEncoding.UTF8);
                Log.WriteLine($"[Validation:{messageSeverity}] ID: {msgIDName} - MSG: {msg}");

                // From Vulkan docs: The application should always return VK_FALSE. The VK_TRUE value is reserved for use in layer development.
                return false;
            });

            PfnVoidFunction pFunc = new PfnVoidFunction(debugMsgCallback);

            DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new DebugUtilsMessengerCreateInfoEXT()
            {
                SType = StructureType.DebugUtilsMessengerCreateInfoExt,
                MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                                DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                                DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt |
                                DebugUtilsMessageSeverityFlagsEXT.InfoBitExt,
                MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                                DebugUtilsMessageTypeFlagsEXT.ValidationBitExt |
                    DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
                PfnUserCallback = *(PfnDebugUtilsMessengerCallbackEXT*)&pFunc,
                PUserData = null,
            };

            _debugMessengerHandle = EngineUtil.Alloc<DebugUtilsMessengerEXT>();
            Result r = ext.CreateDebugUtilsMessenger(*_instance, &debugCreateInfo, null, _debugMessengerHandle);
            if (!CheckResult(r))
                EngineUtil.Free(ref _debugMessengerHandle);
        }

        protected override void OnInitialize(EngineSettings settings)
        {
            base.OnInitialize(settings);

            _resFactory = new ResourceFactoryVK(this, null);

            DisplayAdapterVK adapter = _displayManager.SelectedAdapter as DisplayAdapterVK;
            Device = new DeviceVK(this, adapter, _instance, CommandSetCapabilityFlags.Graphics);
            Device.AddExtension<KhrSwapchain>();

            if (Device.Initialize())
                _devices.Add(Device);

            Assembly includeAssembly = GetType().Assembly;
            ShaderCompiler = new DxcCompiler<RendererVK, SpirVShader>(this, "\\Assets\\HLSL\\include\\", includeAssembly);
        }

        internal bool CheckResult(Result r, Func<string> getMsg = null)
        {
            if (r != Result.Success)
            {
                if (getMsg == null)
                    Log.Error($"Vulkan error: {r}");
                else
                    Log.Error($"Vulkan error: {r} -- {getMsg()}");

                return false;
            }

            return true;
        }

        internal unsafe T[] Enumerate<T>(EnumerateCallback<T> callback, string callbackName = "")
            where T : unmanaged
        {
            string GetCallbackName() => string.IsNullOrWhiteSpace(callbackName) ? callback.Method.Name : callbackName;

            uint count = 0;
            Result r = callback(&count, null);

            if (CheckResult(r, () =>$"Enumerate: Failed to get {GetCallbackName} count"))
            {
                T* items = EngineUtil.AllocArray<T>(count);
                r = callback(&count, items);

                if (CheckResult(r, () => $"Enumerate: Failed get {GetCallbackName} items"))
                {
                    T[] result = new T[count];
                    fixed (T* ptrResult = result)
                        System.Buffer.MemoryCopy(items, ptrResult, sizeof(T) * count, sizeof(T) * count);

                    return result;
                }

                EngineUtil.Free(ref items);
            }

            return new T[0];
        }

        /// <summary>
        /// Gets an extension that was loaded for the <see cref="Instance"/>.
        /// </summary>
        /// <typeparam name="E">The type of extension to retrieve.</typeparam>
        /// <returns></returns>
        internal E GetInstanceExtension<E>()
            where E : NativeExtension<Vk>
        {
            return _instanceLoader.GetExtension<E>();
        }

        public override DisplayManager DisplayManager => _displayManager;

        public override ResourceFactory Resources => _resFactory;

        public override IComputeManager Compute { get; }

        protected override SceneRenderData OnCreateRenderData()
        {
            throw new NotImplementedException();
        }        

        protected override void OnPostPresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPostRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPostRenderScene(SceneRenderData sceneData, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPrePresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPreRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPreRenderScene(SceneRenderData sceneData, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnRebuildSurfaces(uint requiredWidth, uint requiredHeight)
        {
            throw new NotImplementedException();
        }

        protected override IRenderChain Chain => _chain;

        protected override void OnDisposeBeforeRender()
        {
            ShaderCompiler.Dispose();

            foreach (DeviceVK device in _devices)
                device.Dispose();
            _devices.Clear();

            _displayManager.Dispose();
            _instanceLoader.Dispose();

            if(_instance != null)
            {
                VK.DestroyInstance(*_instance, null);
                EngineUtil.Free(ref _instance);
            }

            GLFW.Dispose();
            VK.Dispose();
        }

        /// <summary>
        /// Gets the underlying <see cref="Vk"/> API instance.
        /// </summary>
        internal Vk VK { get; }

        /// <summary>
        /// Gets the underlying <see cref="Glfw"/> API instance.
        /// </summary>
        internal Glfw GLFW { get; }

        /// <summary>
        /// Gets the underlying <see cref="Silk.NET.Vulkan.Instance"/>.
        /// </summary>
        internal Instance* Instance => _instance;

        /// <summary>
        /// Gets a <see cref="DeviceVK"/> by it's index. The primary <see cref="Device"/> is always at index 0, if it exists.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal DeviceVK this[int index] => _devices[index];

        /// <summary>
        /// Gets the main <see cref="DeviceVK"/>.
        /// </summary>
        internal DeviceVK Device { get; private set; }

        internal DxcCompiler<RendererVK, SpirVShader> ShaderCompiler { get; private set; }

        internal VersionVK ApiVersion { get; }
    }
}
