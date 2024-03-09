using Molten.Cache;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX12;

internal class PipelineStateBuilderDX12
{
    struct CacheKey : IEquatable<CacheKey>
    {
        public ShaderPassDX12 Pass;

        public PipelineInputLayoutDX12 InputLayout;

        public bool IsValid => Pass != null && InputLayout != null;

        public CacheKey(ShaderPassDX12 pass, PipelineInputLayoutDX12 layout)
        {
            Pass = pass;
            InputLayout = layout;
        }

        public override bool Equals(object obj)
        {
            if(obj is CacheKey key)
                return Equals(key);
            else
                return false;
        }

        public bool Equals(CacheKey other)
        {
            return IsValid == other.IsValid 
                && Pass.Equals(other.Pass) 
                && InputLayout.Equals(other.InputLayout);
        }
    }

    KeyedObjectCache<CacheKey, PipelineStateDX12> _cache = new();
    static readonly Dictionary<D3DRootSignatureVersion, RootSignaturePopulatorDX12> _rootPopulators = new()
    {
        [D3DRootSignatureVersion.Version10] = new RootSigPopulator1_0(),
        [D3DRootSignatureVersion.Version11] = new RootSigPopulator1_1(),
    };

    D3DRootSignatureVersion _rootSignatureVersion;
    RootSignaturePopulatorDX12 _rootSigPopulator;

    internal PipelineStateBuilderDX12(DeviceDX12 device)
    {
        _rootSignatureVersion = device.CapabilitiesDX12.RootSignatureVersion;
        if (!_rootPopulators.TryGetValue(_rootSignatureVersion, out _rootSigPopulator))
            throw new NotSupportedException($"Unsupported root signature version: {_rootSignatureVersion}.");
    }

    internal unsafe PipelineStateDX12 Build(
        ShaderPassDX12 pass, 
        PipelineInputLayoutDX12 layout, 
        IndexBufferStripCutValue indexStripCutValue = IndexBufferStripCutValue.ValueDisabled,
        CachedPipelineState? cachedState = null)
    {
        CacheKey cacheKey = new(pass, layout);
        DeviceDX12 device = pass.Device as DeviceDX12;
        PipelineStateDX12 result = null;

        // Return the cached pipeline state.
        if (_cache.Check(ref cacheKey, ref result))
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
            VS = pass.GetBytecode(ShaderStageType.Vertex),
            GS = pass.GetBytecode(ShaderStageType.Geometry),
            DS = pass.GetBytecode(ShaderStageType.Domain),
            HS = pass.GetBytecode(ShaderStageType.Hull),
            PS = pass.GetBytecode(ShaderStageType.Pixel),
            PrimitiveTopologyType = pass.GeometryPrimitive.ToApiToplogyType(),
            NodeMask = 0,               // TODO Set this to the node mask of the device.
            IBStripCutValue = indexStripCutValue,
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
        ShaderPassStage ps = pass[ShaderStageType.Pixel];
        if (ps != null)
        {
            desc.NumRenderTargets = (uint)ps.OutputLayout.Metadata.Length;

            for (int i = 0; i < desc.NumRenderTargets; i++)
                desc.RTVFormats[i] = (Format)pass.FormatLayout.RawFormats[i];
        }
        else // ... If no pixel shader is present, but a geometry shader is, populate the stream output format.
        {
            ShaderPassStage gs = pass[ShaderStageType.Geometry];
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
                        OutputSlot = 0,     // TODO populate - 0 to 3 only.
                    };
                }

                // TODO populate this properly.
                desc.StreamOutput = new StreamOutputDesc()
                {
                    RasterizedStream = pass.RasterizedStreamOutput,
                    NumEntries = (byte)numEntries, 
                    NumStrides = 0,         // TODO Populate this.
                    PBufferStrides = null,  // TODO Populate this.
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

        RootSignatureDX12 rootSig = BuildRootSignature(pass, ref desc, device);
        desc.PRootSignature = rootSig;

        Guid guid = ID3D12PipelineState.Guid;
        void* ptr = null;
        HResult hr = device.Handle->CreateGraphicsPipelineState(desc, &guid, &ptr);
        if (!device.Log.CheckResult(hr, () => "Failed to create pipeline state object (PSO)"))
            return null;

        result = new PipelineStateDX12(device, (ID3D12PipelineState*)ptr);

        // Add the new pipeline state to the cache.
        _cache.Add(ref cacheKey, ref result);

        // Free all GS stream output semantic name pointers, if any.
        for(int i = 0; i < desc.StreamOutput.NumEntries; i++)
        {
            SODeclarationEntry* entry = &desc.StreamOutput.PSODeclaration[i];
            SilkMarshal.Free((IntPtr)entry->SemanticName);
        }

        return result;
    }

    private unsafe RootSignatureDX12 BuildRootSignature(ShaderPassDX12 pass, ref readonly GraphicsPipelineStateDesc psoDesc, DeviceDX12 device)
    {
        // TODO Check root signature cache for existing root signature.

        VersionedRootSignatureDesc sigDesc = new(_rootSignatureVersion);
        _rootSigPopulator.Populate(ref sigDesc, in psoDesc, pass);

        // Serialize the root signature.
        ID3D10Blob* signature = null;
        ID3D10Blob* error = null;

        HResult hr = device.Renderer.Api.SerializeVersionedRootSignature(&sigDesc, &signature, &error);
        if (!device.Log.CheckResult(hr, () => "Failed to serialize root signature"))
            hr.Throw();

        // TODO Read the error blob and log it if it contains any errors.

        // TODO Implement root signature caching - Store the serialized signature blob in cache file.

        // Create the root signature.
        Guid guid = ID3D12RootSignature.Guid;
        void* ptr = null;
        hr = device.Handle->CreateRootSignature(0, signature->GetBufferPointer(), signature->GetBufferSize(), &guid, &ptr);
        if (!device.Log.CheckResult(hr, () => "Failed to create root signature"))
            hr.Throw();

        RootSignatureDX12 result = new RootSignatureDX12(device, (ID3D12RootSignature*)ptr);
        _rootSigPopulator.Free(ref sigDesc);

        return result;
    }
}
