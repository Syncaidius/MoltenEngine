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
        Vk _vk;
        Instance* _vkInstance;
        RenderChainVK _chain;
        DisplayManagerVK _displayManager;

        public RendererVK()
        {
            _vk = Vk.GetApi();
            _displayManager = new DisplayManagerVK();
            _chain = new RenderChainVK(this);
        }

        internal static uint MakeVersion(uint variant, uint major, uint minor, uint patch)
        {
            return (((variant) << 29) | ((major) << 22) | ((minor) << 12) | (patch));
        }

        protected override void OnInitialize(EngineSettings settings)
        {
            base.OnInitialize(settings);

            ApplicationInfo appInfo = new ApplicationInfo()
            {
                SType = StructureType.ApplicationInfo,
                EngineVersion = 1,
                ApiVersion = MakeVersion(0,1,0,0),
            };

            InstanceCreateInfo instanceInfo = new InstanceCreateInfo()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledLayerCount = 0,
                EnabledExtensionCount = 0,
            };

            if (settings.Graphics.EnableDebugLayer.Value == true)
            {
                EnableValidationLayers(out instanceInfo.EnabledLayerCount, out instanceInfo.PpEnabledLayerNames, 
                    "VK_LAYER_KHRONOS_validation");
            }

            _vkInstance = EngineUtil.Alloc<Instance>();
            Result r = _vk.CreateInstance(&instanceInfo, null, _vkInstance);
            LogResult(r);

            SilkMarshal.FreeString((nint)instanceInfo.PpEnabledLayerNames, NativeStringEncoding.UTF8);
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

        private void EnableValidationLayers(out uint enabledCount, out byte** pEnabledNames, params string[] layerNames)
        {
            uint lCount = 0;
            pEnabledNames = null;

            Result r = _vk.EnumerateInstanceLayerProperties(&lCount, null);
            List<string> enabledNames = new List<string>();

            if (LogResult(r))
            {
                LayerProperties* layerInfo = EngineUtil.AllocArray<LayerProperties>(lCount);
                r = _vk.EnumerateInstanceLayerProperties(&lCount, layerInfo);

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
                                goto Recode;
                            else
                                break;
                        }
                    }

                    layerInfo++;
                }

                EngineUtil.Free(ref layerInfo);
            }

            Recode:
            enabledCount = (uint)enabledNames.Count;
            pEnabledNames = (byte**)SilkMarshal.StringArrayToPtr(enabledNames.AsReadOnly(), NativeStringEncoding.UTF8);
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
                _vk.DestroyInstance(*_vkInstance, null);
                EngineUtil.Free(ref _vkInstance);
            }

            _vk.Dispose();
        }
    }
}
