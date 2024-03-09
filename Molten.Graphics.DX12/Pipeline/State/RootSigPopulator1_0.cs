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
        ShaderBindManager bindings = pass.Parent.Bindings;
        ref RootSignatureDesc desc = ref versionedDesc.Desc10;
        PopulateStaticSamplers(ref desc.PStaticSamplers, ref desc.NumStaticSamplers, pass);

        List<DescriptorRange> ranges = new();
        PopulateRanges(DescriptorRangeType.Srv, ranges, bindings.Resources[(int)ShaderBindType.Resource]);
        PopulateRanges(DescriptorRangeType.Uav, ranges, bindings.Resources[(int)ShaderBindType.UnorderedAccess]);
        PopulateRanges(DescriptorRangeType.Cbv, ranges, bindings.Resources[(int)ShaderBindType.ConstantBuffer]);

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

    private void PopulateRanges<V>(DescriptorRangeType type, List<DescriptorRange> ranges, ShaderBind<V>[] variables)
        where V : ShaderVariable
    {
        uint prevBindPoint = 0;

        DescriptorRange range = new();

        for (uint i = 0; i < variables.Length; i++)
        {
            ref ShaderBind<V> bp = ref variables[i];

            if (prevBindPoint != bp.Info.BindPoint - 1)
            {
                if (range.NumDescriptors > 0)
                    ranges.Add(range);

                range = new DescriptorRange();
                range.BaseShaderRegister = bp.Info.BindPoint;
                range.RangeType = type;
                range.RegisterSpace = bp.Info.BindSpace;
                range.OffsetInDescriptorsFromTableStart = 0;
            }

            prevBindPoint = bp.Info.BindPoint;
            range.NumDescriptors++;
        }

        // Finalize the last range, if any.
        if (range.NumDescriptors > 0)
            ranges.Add(range);
    }
}
