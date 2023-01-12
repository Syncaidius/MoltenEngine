using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
        DisplayManagerVK _displayManager;
        InstanceManager _instance;
        DebugUtilsMessengerEXT* _debugMessengerHandle;

        List<DeviceVK> _devices;
        VersionVK _apiVersion;

        public RendererVK()
        {
            VK = Vk.GetApi();
            GLFW = Glfw.GetApi();
            GLFW.Init();

            _devices = new List<DeviceVK>();
            _instance = new InstanceManager(this);
            _displayManager = new DisplayManagerVK(this);
            _chain = new RenderChainVK(this);
            _apiVersion = new VersionVK(1, 1, 0);
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
                _instance.AddLayer("VK_LAYER_KHRONOS_validation");
                _instance.AddExtension<KhrSurface>();
                _instance.AddExtension<ExtDebugUtils>(SetupDebugMessenger, (ext) =>
                {
                    // Dispose of debug messenger handle.
                    if (_debugMessengerHandle != null)
                    {
                        ext.DestroyDebugUtilsMessenger(*_instance.Ptr, *_debugMessengerHandle, null);
                        _debugMessengerHandle = null;
                    }
                });

            }

            _instance.AddGlfwExtensions();
            if (!_instance.Build(_apiVersion))
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
            Result r = ext.CreateDebugUtilsMessenger(*_instance.Ptr, &debugCreateInfo, null, _debugMessengerHandle);
            if (!CheckResult(r))
                EngineUtil.Free(ref _debugMessengerHandle);
        }

        protected override void OnInitialize(EngineSettings settings)
        {
            base.OnInitialize(settings);

            DisplayAdapterVK adapter = _displayManager.SelectedAdapter as DisplayAdapterVK;
            DeviceVK mainDevice = new DeviceVK(this, adapter, _instance, CommandSetCapabilityFlags.Graphics);

            if (mainDevice.Build(_apiVersion))
                _devices.Add(mainDevice);
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

        public override DisplayManager DisplayManager => _displayManager;

        public override ResourceFactory Resources { get; }

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
            foreach (DeviceVK device in _devices)
                device.Dispose();

            _devices.Clear();

            _displayManager.Dispose();
            _instance.Dispose();

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
        /// Gets the underlying <see cref="InstanceManager"/> which manages a <see cref="Silk.NET.Vulkan.Instance"/> object.
        /// </summary>
        internal InstanceManager Instance => _instance;

        /// <summary>
        /// Gets a <see cref="DeviceVK"/> by it's index. The primary <see cref="Device"/> is always at index 0, if it exists.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal DeviceVK this[int index] => _devices[index];

        /// <summary>
        /// Gets the main <see cref="DeviceVK"/>.
        /// </summary>
        internal DeviceVK Device { get; }
    }
}
