using Silk.NET.Direct3D12;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Molten.Graphics.DX12;
internal class RootSigPopulator1_0 : RootSignaturePopulatorDX12
{
    internal override unsafe void Populate(ref VersionedRootSignatureDesc versionedDesc, 
        ref readonly GraphicsPipelineStateDesc psoDesc, 
        ShaderPassDX12 pass)
    {
        Shader parent = pass.Parent;
        ref RootSignatureDesc desc = ref versionedDesc.Desc10;
        PopulateStaticSamplers(ref desc.PStaticSamplers, ref desc.NumStaticSamplers, pass);

        List<DescriptorRange> ranges = new();
        PopulateRanges(DescriptorRangeType.Srv, ranges, parent.Resources[(int)ShaderBindType.Resource]);
        PopulateRanges(DescriptorRangeType.Uav, ranges, parent.Resources[(int)ShaderBindType.UnorderedAccess]);
        PopulateRanges(DescriptorRangeType.Cbv, ranges, parent.Resources[(int)ShaderBindType.ConstantBuffer]);

        // TODO Add support for heap-based samplers.
        // TODO Add support for static CBV (which require their own root parameter with the data_static flag set.

        desc.NumParameters = 1;
        desc.PParameters = EngineUtil.AllocArray<RootParameter>(desc.NumParameters);
        ref RootParameter param = ref desc.PParameters[0];
        desc.Flags = GetFlags(in psoDesc, pass);

        param.ParameterType = RootParameterType.TypeDescriptorTable;
        param.DescriptorTable.NumDescriptorRanges = (uint)ranges.Count;
        param.DescriptorTable.PDescriptorRanges = EngineUtil.AllocArray<DescriptorRange>((uint)ranges.Count);
        param.ShaderVisibility = ShaderVisibility.All; // TODO If a parameter is only used on 1 stage, set this to that stage.

        Span<DescriptorRange> rangeSpan = CollectionsMarshal.AsSpan(ranges);
        Span<DescriptorRange> tableRanges = new(param.DescriptorTable.PDescriptorRanges, ranges.Count);
        rangeSpan.CopyTo(tableRanges);

    }

    internal unsafe override void Free(ref VersionedRootSignatureDesc versionedDesc)
    {
        ref RootSignatureDesc desc = ref versionedDesc.Desc10;

        for (int i = 0; i < desc.NumParameters; i++)
            EngineUtil.Free(ref desc.PParameters[i].DescriptorTable.PDescriptorRanges);

        EngineUtil.Free(ref desc.PParameters);
        EngineUtil.Free(ref desc.PStaticSamplers);
    }

    private void PopulateRanges(DescriptorRangeType type, List<DescriptorRange> ranges, Array variables)
    {
        uint last = 0;
        uint i = 0;
        DescriptorRange r = new();

        for (; i < variables.Length; i++)
        {
            if (variables.GetValue(i) == null)
                continue;

            // Create a new range if there was a gap.
            uint prev = i - 1; // What the previous should be.
            if (last == i || last == prev)
            {
                // Finalize previous range
                if (last != i)
                    r.NumDescriptors = i - r.BaseShaderRegister;

                // Start new range.
                r = new DescriptorRange();
                r.BaseShaderRegister = i;
                r.RangeType = type;
                r.RegisterSpace = 0; // TODO Populate - Requires reflection enhancements to support new HLSL register syntax: register(t0, space1);
                r.OffsetInDescriptorsFromTableStart = 0;
                ranges.Add(r);
            }

            last = i;
        }

        // Finalize last range to the list
        r.NumDescriptors = i - r.BaseShaderRegister;
    }
}
