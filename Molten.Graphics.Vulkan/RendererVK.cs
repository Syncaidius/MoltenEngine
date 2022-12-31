using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Molten.Font;
using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;

namespace Molten.Graphics
{
    public unsafe class RendererVK : RenderService
    {
        Instance* _vkInstance;
        RenderChainVK _chain;
        DisplayManagerVK _displayManager;

        List<string> _instanceExtensions;
        ExtDebugUtils _extDebugUtils;
        DebugUtilsMessengerEXT* _debugMessengerHandle;

        public RendererVK()
        {
            VK = Vk.GetApi();
            _displayManager = new DisplayManagerVK(this);
            _chain = new RenderChainVK(this);
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

        internal static void UnpackVersion(uint value, out uint variant, out uint major, out uint minor, out uint patch)
        {
            variant = (value >> 29);
            major = (value >> 22) & 0x7FU;
            minor = (value >> 12) & 0x3FFU;
            patch = value & 0xFFFU;
        }

        internal unsafe delegate Result CreateDebugUtilsMessengerEXT();
        protected override void OnInitializeApi(GraphicsSettings settings)
        {
            _instanceExtensions = new List<string>();

            ApplicationInfo appInfo = new ApplicationInfo()
            {
                SType = StructureType.ApplicationInfo,
                EngineVersion = 1,
                ApiVersion = MakeVersion(0, 1, 3, 0),
            };

            InstanceCreateInfo createInfo = new InstanceCreateInfo()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledLayerCount = 0,
                EnabledExtensionCount = 0,
            };

            // TODO Store baseline profiles for each OS/platform where possible, or default to Moltens own.
            // For android see: https://developer.android.com/ndk/guides/graphics/android-baseline-profile

            if (settings.EnableDebugLayer.Value == true)
                SetupValidationLayers(ref createInfo);

            _vkInstance = EngineUtil.Alloc<Instance>();
            Result r = VK.CreateInstance(&createInfo, null, _vkInstance);
            LogResult(r);

            if (settings.EnableDebugLayer.Value == true)
            {
                SilkMarshal.FreeString((nint)createInfo.PpEnabledLayerNames, NativeStringEncoding.UTF8);
                SetupDebugMessenger();
            }
        }

        private void SetupValidationLayers(ref InstanceCreateInfo createInfo)
        {
            List<string> layerNames = Enable<LayerProperties>(VK.EnumerateInstanceLayerProperties, "VK_LAYER_KHRONOS_validation");

            createInfo.EnabledLayerCount = (uint)layerNames.Count;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(layerNames.AsReadOnly(), NativeStringEncoding.UTF8);

            // Enable extension needed for custom handling of validation layer messages.
            _instanceExtensions = Enable<ExtensionProperties>((count, infoArray) =>
            {
                byte* ptrLayer = null;
                return VK.EnumerateInstanceExtensionProperties(ptrLayer, count, infoArray);
            }, "VK_EXT_debug_utils");

            createInfo.EnabledExtensionCount = (uint)_instanceExtensions.Count;
            createInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(_instanceExtensions.AsReadOnly(), NativeStringEncoding.UTF8);

            foreach (string layerName in layerNames)
                Log.WriteLine($"Enabled validation layer: {layerName}");

            foreach (string extName in _instanceExtensions)
                Log.WriteLine($"Enabled extension: {extName}");
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate Bool32 DebugMessengerCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity,
            DebugUtilsMessageTypeFlagsEXT messageTypes,
            DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData);

        private void SetupDebugMessenger()
        {
            if (_instanceExtensions.Contains("VK_EXT_debug_utils"))
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

                PfnVoidFunction pFunc  = new PfnVoidFunction(debugMsgCallback);

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

                if (!VK.TryGetInstanceExtension(*_vkInstance, out _extDebugUtils))
                    Log.Error($"Failed to get instance extension: VK_EXT_debug_utils");

                _debugMessengerHandle = EngineUtil.Alloc<DebugUtilsMessengerEXT>();
                Result r = _extDebugUtils.CreateDebugUtilsMessenger(*_vkInstance, &debugCreateInfo, null, _debugMessengerHandle);
                if (!LogResult(r))
                    EngineUtil.Free(ref _debugMessengerHandle);
            }
        }

        protected override void OnInitialize(EngineSettings settings)
        {
            base.OnInitialize(settings);
        }

        internal bool LogResult(Result r, string msg = "")
        {
            if (r != Result.Success)
            {
                if (string.IsNullOrWhiteSpace(msg))
                    Log.Error($"Vulkan error: {r}");
                else
                    Log.Error($"Vulkan error: {r} -- {msg}");

                return false;
            }

            return true;
        }

        private unsafe delegate Result EnumerateInstanceCallback<T>(uint* count, T* info) where T : unmanaged;

        private List<string> Enable<T>(EnumerateInstanceCallback<T> callback, params string[] names)
            where T : unmanaged
        {
            uint lCount = 0;

            Result r = callback(&lCount, null);
            List<string> enabledNames = new List<string>();

            if (LogResult(r))
            {
                T* infoArray = EngineUtil.AllocArray<T>(lCount);
                r = callback(&lCount, infoArray);

                for (uint i = 0; i < lCount; i++)
                {
                    string name = SilkMarshal.PtrToString((nint)infoArray, NativeStringEncoding.UTF8);

                    // Compare name
                    foreach(string enumName in names)
                    {
                        if (name == enumName)
                        {
                            enabledNames.Add(enumName);
                            if (enabledNames.Count == names.Length)
                                return enabledNames;
                                
                            break;
                        }
                    }

                    infoArray++;
                }

                EngineUtil.Free(ref infoArray);
            }

            return enabledNames;
        }

        public override IDisplayManager DisplayManager => _displayManager;

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
            _displayManager.Dispose();

            if (_vkInstance != null)
            {
                // Dispose of debug messenger.
                if (_debugMessengerHandle != null)
                {
                    _extDebugUtils.DestroyDebugUtilsMessenger(*_vkInstance, *_debugMessengerHandle, null);
                    _extDebugUtils.Dispose();
                }

                VK.DestroyInstance(*_vkInstance, null);
                EngineUtil.Free(ref _vkInstance);
            }

            VK.Dispose();
        }

        /// <summary>
        /// Gets the underlying <see cref="Vk"/> API instance.
        /// </summary>
        internal Vk VK { get; }

        /// <summary>
        /// Gets the underlying <see cref="Instance"/> pointer.
        /// </summary>
        internal Instance* Ptr => _vkInstance;
    }
}
