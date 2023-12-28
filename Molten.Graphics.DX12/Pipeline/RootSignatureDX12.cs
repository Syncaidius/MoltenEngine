using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12
{
    internal unsafe class RootSignatureDX12 : GraphicsObject<DeviceDX12>
    {
        ID3D12RootSignature* _handle;

        internal RootSignatureDX12(DeviceDX12 device/*, ShaderPassDX12 pass*/) : base(device)
        {
            // TODO Check device feature support for root signature max version.

            RootParameter1* parameters = EngineUtil.AllocArray<RootParameter1>(1); // TODO Get the correct size based on the provided pass.
            RootSignatureDesc1 rootSignatureDesc1 = new()
            {
                Flags = RootSignatureFlags.None,
                NumParameters = 0, // TODO Get this from the number of input parameters (textures, constant buffers, etc) required by the shader pass.
                PParameters = parameters,
                NumStaticSamplers = 0,
                PStaticSamplers = null,
            };

            EngineUtil.Free(ref parameters);

            // TODO build based on input resources, output resources and samplers of the provided shader pass.
            // TODO Note for Vulkan: Shaders only operate on a set number of render target outputs, so we'll always know how many should be bound based on this.
            //      Example 1: If a shader only outputs to a single render target, then we know that only 1 render target will always need to be bound.
            //      Example 2: If a deferred shader (GBuffer, normals, depth, etc) outputs to 3 render targets, then we know that 3 render targets will always need to be bound.
            //      This also means we can perform validation for missing render targets if they are required.

            // Serialize the root signature.
            ID3D10Blob* signature = null;
            ID3D10Blob* error = null;
            RendererDX12 renderer = device.Renderer as RendererDX12;
            HResult hr = renderer.Api.SerializeRootSignature((RootSignatureDesc*)&rootSignatureDesc1, D3DRootSignatureVersion.Version11, &signature, &error);
            if(!device.Log.CheckResult(hr, () => "Failed to serialize root signature"))
                hr.Throw();

            // Create the root signature.
            Guid guid = ID3D12RootSignature.Guid;
            void* ptr = null;
            hr = device.Ptr->CreateRootSignature(0, signature->GetBufferPointer(), signature->GetBufferSize(), &guid, &ptr);
            if (!device.Log.CheckResult(hr, () => "Failed to create root signature"))
                hr.Throw();
        }

        protected override void OnGraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _handle);
        }
    }
}
