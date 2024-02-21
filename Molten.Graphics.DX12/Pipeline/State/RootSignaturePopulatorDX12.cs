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
        // Finalize sampler visibility. Iterate over all samplers used in the pass.
        for (int i = 0; i < pass.Parent.SharedStaticSamplers.Count; i++)
        {
            SamplerDX12 sampler = pass.Parent.SharedStaticSamplers[i] as SamplerDX12;
            uint visCount = 0;
            ShaderVisibility vis = ShaderVisibility.All;

            // Check all compositions of the current pass to see where the sampler is used.
            foreach (ShaderComposition sc in pass.Compositions)
            {
                // Check static samplers used in the current composition.
                for (int j = 0; j < sc.StaticSamplers.Length; j++)
                {
                    if (sc.StaticSamplers[j] == sampler)
                    {
                        visCount++;

                        // If visibility count is only 1 stage, we can use a specific visibility.
                        if (visCount == 1)
                        {
                            vis = sc.Type switch
                            {
                                ShaderType.Vertex => ShaderVisibility.Vertex,
                                ShaderType.Geometry => ShaderVisibility.Geometry,
                                ShaderType.Hull => ShaderVisibility.Hull,
                                ShaderType.Domain => ShaderVisibility.Domain,
                                ShaderType.Pixel => ShaderVisibility.Pixel,
                                ShaderType.Amplification => ShaderVisibility.Amplification,
                                ShaderType.Mesh => ShaderVisibility.Mesh,
                                _ => vis,
                            };
                        }
                        else // ... Otherwise we have to revert visibility to 'All'.
                        {
                            // Break out of nested loop since we know the visibility is mixed and requires the 'All' flag.
                            vis = ShaderVisibility.All;
                            goto NextSampler;
                        }
                    }
                }
            }

        NextSampler:
            sampler.Desc.ShaderVisibility = vis;
        }

        numSamplers = (uint)pass.StaticSamplers.Length;
        samplers = EngineUtil.AllocArray<StaticSamplerDesc>(numSamplers);
        for (uint i = 0; i < numSamplers; i++)
        {
            SamplerDX12 sampler = pass.StaticSamplers[i] as SamplerDX12;
            samplers[i] = sampler.Desc;
        }
    }

    protected unsafe RootSignatureFlags GetFlags(ref readonly GraphicsPipelineStateDesc psoDesc, ShaderPassDX12 pass)
    {
        RootSignatureFlags flags = RootSignatureFlags.None;

        if (psoDesc.StreamOutput.PSODeclaration != null)
            flags |= RootSignatureFlags.AllowStreamOutput;

        return flags;
    }
}
