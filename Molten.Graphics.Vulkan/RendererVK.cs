using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core;
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
        InstanceManager _instanceManager;
        DebugUtilsMessengerEXT* _debugMessengerHandle;

        public RendererVK()
        {
            VK = Vk.GetApi();
            _instanceManager = new InstanceManager(this);
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

        internal unsafe delegate Result CreateDebugUtilsMessengerEXT();
        protected override void OnInitializeApi(GraphicsSettings settings)
        {
            _instanceManager.BeginNew();
            // TODO Store baseline profiles for each OS/platform where possible, or default to Moltens own.
            // For android see: https://developer.android.com/ndk/guides/graphics/android-baseline-profile

            if (settings.EnableDebugLayer.Value == true)
            {
                _instanceManager.AddLayer("VK_LAYER_KHRONOS_validation");
                _instanceManager.AddExtension<ExtDebugUtils>(SetupDebugMessenger, (ext) =>
                {
                    // Dispose of debug messenger handle.
                    if (_debugMessengerHandle != null)
                    {
                        ext.DestroyDebugUtilsMessenger(*_vkInstance, *_debugMessengerHandle, null);
                        _debugMessengerHandle = null;
                    }
                });
            }

            if (!_instanceManager.Build(out _vkInstance))
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
            Result r = ext.CreateDebugUtilsMessenger(*_vkInstance, &debugCreateInfo, null, _debugMessengerHandle);
            if (!LogResult(r))
                EngineUtil.Free(ref _debugMessengerHandle);
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
            _instanceManager.Dispose();

            if (_vkInstance != null)
            {
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
