using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    public unsafe class RendererVK : RenderService
    {
        Instance* _vkInstance;
        RenderChainVK _chain;
        DisplayManagerVK _displayManager;

        public RendererVK()
        {
            VK = Vk.GetApi();
            _displayManager = new DisplayManagerVK(this);
            _chain = new RenderChainVK(this);
        }

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

        protected override void OnInitializeApi(GraphicsSettings settings)
        {
            ApplicationInfo appInfo = new ApplicationInfo()
            {
                SType = StructureType.ApplicationInfo,
                EngineVersion = 1,
                ApiVersion = MakeVersion(0, 1, 0, 0),
            };

            InstanceCreateInfo instanceInfo = new InstanceCreateInfo()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledLayerCount = 0,
                EnabledExtensionCount = 0,
            };

            if (settings.EnableDebugLayer.Value == true)
            {
                List<string> enabledNames = EnableValidationLayers("VK_LAYER_KHRONOS_validation");

                instanceInfo.EnabledLayerCount = (uint)enabledNames.Count;
                instanceInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(enabledNames.AsReadOnly(), NativeStringEncoding.UTF8);
            }

            _vkInstance = EngineUtil.Alloc<Instance>();
            Result r = VK.CreateInstance(&instanceInfo, null, _vkInstance);
            LogResult(r);

            if (settings.EnableDebugLayer.Value == true)
                SilkMarshal.FreeString((nint)instanceInfo.PpEnabledLayerNames, NativeStringEncoding.UTF8);
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

        private List<string> EnableValidationLayers(params string[] layerNames)
        {
            uint lCount = 0;

            Result r = VK.EnumerateInstanceLayerProperties(&lCount, null);
            List<string> enabledNames = new List<string>();

            if (LogResult(r))
            {
                LayerProperties* layerInfo = EngineUtil.AllocArray<LayerProperties>(lCount);
                r = VK.EnumerateInstanceLayerProperties(&lCount, layerInfo);

                for (uint i = 0; i < lCount; i++)
                {
                    string name = SilkMarshal.PtrToString((nint)layerInfo, NativeStringEncoding.UTF8);

                    // Compare name
                    foreach(string layerName in layerNames)
                    {
                        if (name == layerName)
                        {
                            enabledNames.Add(layerName);
                            Log.WriteLine($"Enabled validation layer: {name}");

                            if (enabledNames.Count == layerNames.Length)
                                return enabledNames;
                                
                            break;
                        }
                    }

                    layerInfo++;
                }

                EngineUtil.Free(ref layerInfo);
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
