using Silk.NET.Direct3D12;
using System.Runtime.InteropServices;

namespace Molten.Graphics.DX12;
internal class RootSigPopulator1_1 : RootSignaturePopulatorDX12
{
    internal override unsafe void Populate(ref VersionedRootSignatureDesc versionedDesc, 
        ref readonly GraphicsPipelineStateDesc psoDesc, 
        ShaderPassDX12 pass,
        PipelineInputLayoutDX12 layout)
    {
        ShaderBindManager bindings = pass.Parent.Bindings;
        ref RootSignatureDesc1 desc = ref versionedDesc.Desc11;
        PopulateStaticSamplers(ref desc.PStaticSamplers, ref desc.NumStaticSamplers, pass);

        List<DescriptorRange1> ranges = new();
        PopulateRanges(DescriptorRangeType.Srv, ranges, bindings.Resources[(int)ShaderBindType.Resource]);
        PopulateRanges(DescriptorRangeType.Uav, ranges, bindings.Resources[(int)ShaderBindType.UnorderedAccess]);
        PopulateRanges(DescriptorRangeType.Cbv, ranges, bindings.Resources[(int)ShaderBindType.ConstantBuffer]);

        // TODO Add support for heap-based samplers.
        // TODO Add support for static CBV (which require their own root parameter with the data_static flag set.

        desc.NumParameters = 1;
        desc.PParameters = EngineUtil.AllocArray<RootParameter1>(desc.NumParameters);
        ref RootParameter1 param = ref desc.PParameters[0];
        desc.Flags = GetFlags(in psoDesc, layout, pass);

        param.ParameterType = RootParameterType.TypeDescriptorTable;
        param.DescriptorTable.NumDescriptorRanges = (uint)ranges.Count;
        param.DescriptorTable.PDescriptorRanges = EngineUtil.AllocArray<DescriptorRange1>((uint)ranges.Count);
        param.ShaderVisibility = ShaderVisibility.All; // TODO If a parameter is only used on 1 stage, set this to that stage.

        Span<DescriptorRange1> rangeSpan = CollectionsMarshal.AsSpan(ranges);
        Span<DescriptorRange1> tableRanges = new(param.DescriptorTable.PDescriptorRanges, ranges.Count);
        rangeSpan.CopyTo(tableRanges);
    }

    internal override unsafe void Free(ref VersionedRootSignatureDesc versionedDesc)
    {
        ref RootSignatureDesc1 desc = ref versionedDesc.Desc11;

        for (int i = 0; i < desc.NumParameters; i++)
            EngineUtil.Free(ref desc.PParameters[i].DescriptorTable.PDescriptorRanges);

        EngineUtil.Free(ref desc.PParameters);
        EngineUtil.Free(ref desc.PStaticSamplers);
    }

    private void PopulateRanges<V>(DescriptorRangeType type, List<DescriptorRange1> ranges, ShaderBind<V>[] variables)
        where V: ShaderVariable
    {
        uint prevBindPoint = 0;

        DescriptorRange1 range = new();

        for (uint i = 0; i < variables.Length; i++)
        {
            ref ShaderBind<V> bp = ref variables[i];

            if(prevBindPoint != bp.Info.BindPoint - 1)
            {
                if(range.NumDescriptors > 0)
                    ranges.Add(range);

                range = new DescriptorRange1();
                range.BaseShaderRegister = bp.Info.BindPoint;
                range.RangeType = type;
                range.RegisterSpace = bp.Info.BindSpace;
                range.OffsetInDescriptorsFromTableStart = 0;
                range.Flags = DescriptorRangeFlags.None;
            }

            prevBindPoint = bp.Info.BindPoint;
            range.NumDescriptors++;
        }

        // Finalize the last range, if any.
        if (range.NumDescriptors > 0)
            ranges.Add(range);
    }
}
