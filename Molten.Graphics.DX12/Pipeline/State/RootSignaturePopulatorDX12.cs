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
    /// <returns></returns>
    internal abstract void Populate(ref VersionedRootSignatureDesc versionedDesc, ShaderPassDX12 pass);

    internal abstract void Free(ref VersionedRootSignatureDesc versionedDesc);

    internal unsafe void PopulateStaticSamplers(ref StaticSamplerDesc* samplers, ref uint numSamplers, ShaderPassDX12 pass)
    {
        throw new NotImplementedException();
    }
}
