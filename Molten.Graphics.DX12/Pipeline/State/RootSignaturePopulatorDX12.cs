using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

/// <summary>
/// A base class for root signature builders. DirectX 12 supports at least two versions of root signature layouts.
/// </summary>
internal abstract class RootSignaturePopulatorDX12
{
    /// <summary>
    /// Invoked when the builder is being used to create a root signature description.
    /// </summary>
    /// <param name="pass"></param>
    /// <param name="psoDesc"></param>
    /// <param name="versionedDesc"></param>
    /// <returns></returns>
    internal abstract void Populate(ref VersionedRootSignatureDesc versionedDesc, 
        ref readonly GraphicsPipelineStateDesc psoDesc, 
        ShaderPassDX12 pass);

    internal abstract void Free(ref VersionedRootSignatureDesc versionedDesc);

    protected unsafe void PopulateStaticSamplers(ref StaticSamplerDesc* samplers, ref uint numSamplers, ShaderPassDX12 pass)
    {
        throw new NotImplementedException();
    }

    protected unsafe RootSignatureFlags GetFlags(ref readonly GraphicsPipelineStateDesc psoDesc, ShaderPassDX12 pass)
    {
        RootSignatureFlags flags = RootSignatureFlags.None;

        if (psoDesc.StreamOutput.PSODeclaration != null)
            flags |= RootSignatureFlags.AllowStreamOutput;

        return flags;
    }
}
