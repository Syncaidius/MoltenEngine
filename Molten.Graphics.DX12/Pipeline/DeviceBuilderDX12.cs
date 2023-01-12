using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using Feature = Silk.NET.Direct3D12.Feature;

namespace Molten.Graphics
{
    internal unsafe class DeviceBuilderDX12
    {
        RendererDX12 _renderer;
        D3D12 _api;
        readonly D3DFeatureLevel[] _featureLevels = new D3DFeatureLevel[]
        {
            D3DFeatureLevel.Level122,
            D3DFeatureLevel.Level121,
            D3DFeatureLevel.Level120,
            D3DFeatureLevel.Level111,
            D3DFeatureLevel.Level110
        };

        internal DeviceBuilderDX12(D3D12 api, RendererDX12 renderer)
        {
            _renderer = renderer;
            _api = api;
        }

        internal  HResult CreateDevice(
            DisplayAdapterDXGI adapter,
            out ID3D12Device10* device)
        {
            device = null;

            IUnknown* ptrAdapter = (IUnknown*)adapter.Native;
            void* ptrDevice = null;

            Guid guidDevice = ID3D12Device.Guid;
            HResult r = _api.CreateDevice(ptrAdapter, _featureLevels.Last(), &guidDevice, &ptrDevice);

            if (!r.IsFailure)
                device = (ID3D12Device10*)ptrDevice;

            return r;
        }

        internal void GetCapabilities(GraphicsSettings settings, DisplayAdapterDXGI adapter)
        {
            GraphicsCapabilities cap = adapter.Capabilities;
            ID3D12Device10* device = null;
            HResult r = CreateDevice(adapter, out device);
            if (r.IsFailure)
            {
                _renderer.Log.Error($"Failed to detect capabilities for adapter '{adapter.Name}'");
                return;
            }

            FeatureDataFeatureLevels dataFeatures = new FeatureDataFeatureLevels();
            fixed (D3DFeatureLevel* ptrLevels = _featureLevels)
            {
                dataFeatures.NumFeatureLevels = (uint)_featureLevels.Length;
                dataFeatures.PFeatureLevelsRequested = ptrLevels;
                GetFeatureSupport(device, Feature.FeatureLevels, &dataFeatures);
            }

            switch (dataFeatures.MaxSupportedFeatureLevel)
            {
                case D3DFeatureLevel.Level122:
                    cap.Api = GraphicsApi.DirectX12_2;
                    cap.UnorderedAccessBuffers.MaxSlots = 64;
                    cap.MaxShaderModel = ShaderModel.Model6_0;
                    break;

                case D3DFeatureLevel.Level121:
                    cap.Api = GraphicsApi.DirectX12_1;
                    cap.UnorderedAccessBuffers.MaxSlots = 64;
                    cap.MaxShaderModel = ShaderModel.Model6_0;
                    break;

                case D3DFeatureLevel.Level120:
                    cap.Api = GraphicsApi.DirectX12_0;
                    cap.UnorderedAccessBuffers.MaxSlots = 64;
                    cap.MaxShaderModel = ShaderModel.Model5_1;
                    break;

                case D3DFeatureLevel.Level111:
                    cap.Api = GraphicsApi.DirectX11_1;
                    cap.UnorderedAccessBuffers.MaxSlots = 64;
                    cap.MaxShaderModel = ShaderModel.Model5_1;
                    break;

                case D3DFeatureLevel.Level110:
                    cap.Api = GraphicsApi.DirectX11_0;
                    cap.UnorderedAccessBuffers.MaxSlots = 8;
                    cap.MaxShaderModel = ShaderModel.Model5_1;
                    break;
            }

            // Check if a lower shader model is supported instead of API's model.
            FeatureDataShaderModel maxSM = new FeatureDataShaderModel(cap.MaxShaderModel.ToApi());
            GetFeatureSupport(device, Feature.ShaderModel, &maxSM);
            cap.MaxShaderModel = maxSM.HighestShaderModel.FromApi();

            SilkUtil.ReleasePtr(ref device);
        }

        internal void GetFeatureSupport<T>(ID3D12Device10* device, Feature feature, T* pData) where T : unmanaged
        {
            uint sizeOf = (uint)sizeof(T);
            HResult r = device->CheckFeatureSupport(feature, pData, sizeOf);
            if (r.IsFailure)
            {
                string valName = feature.ToString().Replace("Features", "").Replace("Feature", "");
                _renderer.Log.Error($"Failed to retrieve '{valName}' features. Code: {r}");
            }
        }

        internal T GetFeatureSupport<T>(ID3D12Device10* device, Feature feature) where T : unmanaged
        {
            T data = new T();
            GetFeatureSupport(device, feature, &data);
            return data;
        }
    }
}
