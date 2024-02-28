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
        for (int i = 0; i < pass.Parent.SharedSamplers.Count; i++)
        {
            StaticSamplerDX12 sampler = pass.Parent.SharedSamplers[i] as StaticSamplerDX12;
            uint visCount = 0;
            ShaderVisibility vis = ShaderVisibility.All;

            // Check all stages of the current pass to see where the sampler is used.
            foreach (ShaderPassStage passStage in pass.Stages)
            {
                // Check static samplers used in the current stage.
                for (int j = 0; j < passStage.Bindings.Samplers.Length; j++)
                {
                    if (passStage.Bindings.Samplers[j].Object.Sampler == sampler)
                    {
                        visCount++;

                        // If visibility count is only 1 stage, we can use a specific visibility.
                        if (visCount == 1)
                        {
                            vis = passStage.Type switch
                            {
                                ShaderStageType.Vertex => ShaderVisibility.Vertex,
                                ShaderStageType.Geometry => ShaderVisibility.Geometry,
                                ShaderStageType.Hull => ShaderVisibility.Hull,
                                ShaderStageType.Domain => ShaderVisibility.Domain,
                                ShaderStageType.Pixel => ShaderVisibility.Pixel,
                                ShaderStageType.Amplification => ShaderVisibility.Amplification,
                                ShaderStageType.Mesh => ShaderVisibility.Mesh,
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

        // Consolidate the samplers into the provided description array
        numSamplers = (uint)pass.Bindings.Samplers.Length;
        if (numSamplers > 0)
        {
            samplers = EngineUtil.AllocArray<StaticSamplerDesc>(numSamplers);
            for (uint i = 0; i < numSamplers; i++)
            {
                StaticSamplerDX12 sampler = pass.Bindings.Samplers[i].Object.Sampler as StaticSamplerDX12;
                samplers[i] = sampler.Desc;
            }
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
