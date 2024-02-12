using Molten.Cache;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using System;

namespace Molten.Graphics.DX12.Pipeline;

internal class PipelineStateBuilderDX12
{
    public struct CacheInfo : IEquatable<CacheInfo>
    {
        public ShaderPassDX12 Pass;

        public PipelineInputLayoutDX12 InputLayout;

        public RootSignatureDX12 RootSignature;

       public bool Equals(CacheInfo other)
        {
           return Pass == other.Pass
               && InputLayout == other.InputLayout 
               && RootSignature == other.RootSignature;
       }

        public override bool Equals(object obj)
        {
            if(obj is CacheInfo other)
                return Equals(other);

            return false;
        }
    }

    KeyedObjectCache<CacheInfo, PipelineStateDX12> _cache = new();
    Dictionary<ulong, PipelineStateDX12> _states = new();

    internal unsafe PipelineStateDX12 Build(
        ShaderPassDX12 pass, 
        PipelineInputLayoutDX12 layout, 
        IndexBufferStripCutValue indexStripCutValue = IndexBufferStripCutValue.ValueDisabled,
        CachedPipelineState? cachedState = null)
    {
        DeviceDX12 device = pass.Device as DeviceDX12;
        PipelineStateDX12 result = null;

        CacheInfo cacheInfo = new()
        {
            Pass = pass,
            InputLayout = layout,
            RootSignature = BuildRootSignature(pass, device),
        };

        // Return the cached pipeline state.
        if (_cache.Check(ref cacheInfo, ref result))
            return result;

        // Proceed to create new pipeline state.
        GraphicsPipelineStateDesc desc = new()
        {
            InputLayout = layout.Desc,
            Flags = PipelineStateFlags.None,
            RasterizerState = pass.RasterizerState.Desc,
            BlendState = pass.BlendState.Description.Desc,
            SampleMask = pass.BlendState.Description.BlendSampleMask,
            DepthStencilState = pass.DepthState.Description.Desc,
            VS = pass.GetBytecode(ShaderType.Vertex),
            GS = pass.GetBytecode(ShaderType.Geometry),
            DS = pass.GetBytecode(ShaderType.Domain),
            HS = pass.GetBytecode(ShaderType.Hull),
            PS = pass.GetBytecode(ShaderType.Pixel),
            PrimitiveTopologyType = pass.GeometryPrimitive.ToApiToplogyType(),
            
            PRootSignature = cacheInfo.RootSignature,
            NodeMask = 0,               // TODO Set this to the node mask of the device.
            IBStripCutValue = IndexBufferStripCutValue.ValueDisabled,
        };

        // Check if cache data can be set.
        if(cachedState.HasValue)
        {
            if (cachedState.Value.PCachedBlob == null)
                throw new Exception("The provided cached state does not contain a valid blob pointer.");

            if (cachedState.Value.CachedBlobSizeInBytes == 0)
                throw new Exception("The provided cached state cannot have a blob size of 0.");

            desc.CachedPSO = cachedState.Value;
        }

        // Populate render target formats if a pixel shader is present in the pass.
        ShaderComposition ps = pass[ShaderType.Pixel];
        if (ps != null)
        {
            desc.NumRenderTargets = (uint)ps.OutputLayout.Metadata.Length;

            for (int i = 0; i < desc.NumRenderTargets; i++)
                desc.RTVFormats[i] = (Format)pass.FormatLayout.RawFormats[i];
        }
        else // ... If no pixel shader is present, but a geometry shader is, populate the stream output format.
        {
            ShaderComposition gs = pass[ShaderType.Geometry];
            if (gs != null)
            {
                int numEntries = gs.OutputLayout.Metadata.Length;   
                SODeclarationEntry* entries = stackalloc SODeclarationEntry[numEntries];
                for(int i = 0; i < numEntries; i++)
                {
                    ref ShaderIOLayout.ElementMetadata meta = ref gs.OutputLayout.Metadata[i];

                    entries[i] = new SODeclarationEntry()
                    {
                        Stream = meta.StreamOutput,
                        SemanticName = (byte*)SilkMarshal.StringToPtr(meta.Name),
                        SemanticIndex = meta.SemanticIndex,
                        StartComponent = 0, // TODO populate StartComponent
                        ComponentCount = (byte)meta.ComponentCount,
                        OutputSlot = 0, // TODO populate - 0 to 3 only.
                    };
                }

                // TODO populate this properly.
                desc.StreamOutput = new StreamOutputDesc()
                {
                    RasterizedStream = pass.RasterizedStreamOutput,
                    NumEntries = (byte)numEntries, 
                    NumStrides = 0,
                    PBufferStrides = null,
                    PSODeclaration = entries,
                };
            }
        }

        // Populate depth surface format if depth and/or stencil testing is enabled
        if (pass.DepthState.Description.Desc.DepthEnable
            || pass.DepthState.Description.Desc.StencilEnable)
        {
            Format format = (Format)pass.FormatLayout.Depth.ToGraphicsFormat();
            desc.DSVFormat = format;
        }

        // Check multi-sample settings
        if(pass.RasterizerState.Desc.MultisampleEnable)
        {
            desc.SampleDesc = default;       // TODO Implement multisampling
        }

        Guid guid = ID3D12PipelineState.Guid;
        void* ptr = null;
        HResult hr = device.Ptr->CreateGraphicsPipelineState(desc, &guid, &ptr);
        if (!device.Log.CheckResult(hr, () => "Failed to create pipeline state object (PSO)"))
            return null;

        result = new PipelineStateDX12(device, (ID3D12PipelineState*)ptr);

        // Add the new pipeline state to the cache.
        _cache.Add(ref cacheInfo, ref result);

        // Free all GS stream output semantic name pointers, if any.
        for(int i = 0; i < desc.StreamOutput.NumEntries; i++)
        {
            SODeclarationEntry* entry = &desc.StreamOutput.PSODeclaration[i];
            SilkMarshal.Free((IntPtr)entry->SemanticName);
        }

        return result;
    }

    private unsafe RootSignatureDX12 BuildRootSignature(ShaderPassDX12 pass, DeviceDX12 device)
    {
        // TODO Check root signature cache for existing root signature.

        VersionedRootSignatureDesc sigDesc = new()
        {
            Version = device.CapabilitiesDX12.RootSignatureVersion,
        };

        sigDesc.Desc10 = new RootSignatureDesc()
        {
            Flags = RootSignatureFlags.None,
            NumParameters = 0, // TODO Get this from the number of input parameters (textures, constant buffers, etc) required by the shader pass.
            NumStaticSamplers = 0,
            PStaticSamplers = null
        };

        switch (sigDesc.Version)
        {
            case D3DRootSignatureVersion.Version10:
                PopulateRootVersion1Desc(ref sigDesc.Desc10, pass);
                break;

            case D3DRootSignatureVersion.Version11:
                throw new NotImplementedException();
                break;

            default:
                throw new NotSupportedException($"Unsupported root signature version: {sigDesc.Version}.");
        }


        // Serialize the root signature.
        ID3D10Blob* signature = null;
        ID3D10Blob* error = null;

        HResult hr = device.Renderer.Api.SerializeVersionedRootSignature(&sigDesc, &signature, &error);
        if (!device.Log.CheckResult(hr, () => "Failed to serialize root signature"))
            hr.Throw();

        // Create the root signature.
        Guid guid = ID3D12RootSignature.Guid;
        void* ptr = null;
        hr = device.Ptr->CreateRootSignature(0, signature->GetBufferPointer(), signature->GetBufferSize(), &guid, &ptr);
        if (!device.Log.CheckResult(hr, () => "Failed to create root signature"))
            hr.Throw();

        // Freeing the Desc10 parameter pointer also frees Desc11 since they share the same memory space.
        EngineUtil.Free(ref sigDesc.Desc10.PParameters);
        EngineUtil.Free(ref sigDesc.Desc10.PStaticSamplers);

        RootSignatureDX12 result = new RootSignatureDX12(device, (ID3D12RootSignature*)ptr);

        // Free allocated unmanaged memory.
        switch(sigDesc.Version)
        {
            case D3DRootSignatureVersion.Version10:
                for(int i = 0; i < sigDesc.Desc10.NumParameters; i++)
                    EngineUtil.Free(ref sigDesc.Desc10.PParameters[i].DescriptorTable.PDescriptorRanges);

                EngineUtil.Free(ref sigDesc.Desc10.PParameters);
                break;

            case D3DRootSignatureVersion.Version11:
                for (int i = 0; i < sigDesc.Desc11.NumParameters; i++)
                    EngineUtil.Free(ref sigDesc.Desc11.PParameters[i].DescriptorTable.PDescriptorRanges);

                EngineUtil.Free(ref sigDesc.Desc11.PParameters);
                break;
        }

        return result;
    }

    private unsafe void PopulateRootVersion1Desc(ref RootSignatureDesc desc, ShaderPassDX12 pass)
    {
        desc.NumParameters = (uint)pass.Parent.Resources.Length;
        desc.PParameters = EngineUtil.AllocArray<RootParameter>(desc.NumParameters);

        List<DescriptorRange> ranges = new();
        PopulateRanges(DescriptorRangeType.Srv, ranges, pass.Parent.Resources);
        PopulateRanges(DescriptorRangeType.Uav, ranges, pass.Parent.UAVs);
        PopulateRanges(DescriptorRangeType.Cbv, ranges, pass.Parent.ConstBuffers);

        desc.PParameters = EngineUtil.AllocArray<RootParameter>((uint)ranges.Count);
        for(int i = 0; i < ranges.Count; i++)
        {
            ref RootParameter param = ref desc.PParameters[i];

            param.ParameterType = RootParameterType.TypeDescriptorTable;
            param.DescriptorTable.NumDescriptorRanges = 1;
            param.DescriptorTable.PDescriptorRanges = EngineUtil.Alloc<DescriptorRange>();
            param.DescriptorTable.PDescriptorRanges[0] = ranges[i];
            param.ShaderVisibility = ShaderVisibility.All; // TODO populate according to available shader composition types.
        }
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
                {
                    r.NumDescriptors = i - r.BaseShaderRegister;
                    r.RegisterSpace = 0; // TODO Populate - Requires reflection enhancements to support new HLSL register syntax: register(t0, space1);
                    r.OffsetInDescriptorsFromTableStart = 0;

                    ranges.Add(r);
                }

                // Start new range.
                ranges.Add(r);
                r = new DescriptorRange();
                r.BaseShaderRegister = i;
                r.RangeType = type;
            }

            last = i;
        }

        // Add last range to the list
        if (r.NumDescriptors > 0)
        {
            r.NumDescriptors = i - r.BaseShaderRegister;
            r.RegisterSpace = 0; // TODO Populate - Requires reflection enhancements to support new HLSL register syntax: register(t0, space1);
            r.OffsetInDescriptorsFromTableStart = 0;
            ranges.Add(r);
        }
    }
}
