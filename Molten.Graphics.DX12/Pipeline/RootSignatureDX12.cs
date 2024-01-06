using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal unsafe class RootSignatureDX12 : GraphicsObject<DeviceDX12>
{
    ID3D12RootSignature* _handle;

    internal RootSignatureDX12(DeviceDX12 device/*, ShaderPassDX12 pass*/) : base(device)
    {
        VersionedRootSignatureDesc sigDesc = new VersionedRootSignatureDesc()
        {
            Version = device.CapabilitiesDX12.RootSignatureVersion,
        };

        sigDesc.Desc10 = new RootSignatureDesc
        {
            Flags = RootSignatureFlags.None,
            NumParameters = 0, // TODO Get this from the number of input parameters (textures, constant buffers, etc) required by the shader pass.
            NumStaticSamplers = 0,
            PStaticSamplers = null
        };

        switch(sigDesc.Version)
        {
            case D3DRootSignatureVersion.Version10:
                sigDesc.Desc10.PParameters = null; // EngineUtil.AllocArray<RootParameter>(1); // TODO Get the correct size based on the provided pass.
                break;

            case D3DRootSignatureVersion.Version11:
                sigDesc.Desc11.PParameters = null; // EngineUtil.AllocArray<RootParameter1>(1); // TODO Get the correct size based on the provided pass.
                break;

            default:
                throw new NotSupportedException($"Unsupported root signature version: {sigDesc.Version}.");
        }

        // TODO build based on input resources, output resources and samplers of the provided shader pass.
        // TODO Note for Vulkan: Shaders only operate on a set number of render target outputs, so we'll always know how many should be bound based on this.
        //      Example 1: If a shader only outputs to a single render target, then we know that only 1 render target will always need to be bound.
        //      Example 2: If a deferred shader (GBuffer, normals, depth, etc) outputs to 3 render targets, then we know that 3 render targets will always need to be bound.
        //      This also means we can perform validation for missing render targets if they are required.

        // Serialize the root signature.
        ID3D10Blob* signature = null;
        ID3D10Blob* error = null;
        RendererDX12 renderer = device.Renderer as RendererDX12;
        HResult hr = renderer.Api.SerializeVersionedRootSignature(&sigDesc, &signature, &error);
        if(!device.Log.CheckResult(hr, () => "Failed to serialize root signature"))
            hr.Throw();

        // Create the root signature.
        Guid guid = ID3D12RootSignature.Guid;
        void* ptr = null;
        hr = device.Ptr->CreateRootSignature(0, signature->GetBufferPointer(), signature->GetBufferSize(), &guid, &ptr);
        if (!device.Log.CheckResult(hr, () => "Failed to create root signature"))
            hr.Throw();

        EngineUtil.Free(ref sigDesc.Desc10.PParameters);
    }

    public static implicit operator ID3D12RootSignature*(RootSignatureDX12 sig) => sig._handle;

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _handle);
    }
}
