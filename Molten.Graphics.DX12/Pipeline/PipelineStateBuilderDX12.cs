using Molten.Cache;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using System.Runtime.InteropServices;

namespace Molten.Graphics.DX12.Pipeline;

internal class PipelineStateBuilderDX12
{
    public struct CacheInfo : IEquatable<CacheInfo>
    {
        public ShaderPassDX12 Pass;

        public PipelineInputLayoutDX12 InputLayout;

        /// <summary>
        /// The engine object ID of the pipeline state represented by the current <see cref="CacheInfo"/>. 
        /// <para>Not included in equality checks.</para>
        /// </summary>
        public ulong EOID;

       public bool Equals(CacheInfo other)
        {
           return Pass == other.Pass
               && InputLayout == other.InputLayout;
       }

        public override bool Equals(object obj)
        {
            if(obj is CacheInfo other)
                return Equals(other);

            return false;
        }
    }

    TypedObjectCache<CacheInfo> _cache = new();
    Dictionary<ulong, PipelineStateDX12> _states = new();

    internal unsafe PipelineStateDX12 Build(
        ShaderPassDX12 pass, 
        PipelineInputLayoutDX12 layout, 
        IndexBufferStripCutValue indexStripCutValue = IndexBufferStripCutValue.ValueDisabled,
        CachedPipelineState? cachedState = null)
    {
        CacheInfo cacheInfo = new()
        {
            Pass = pass,
            InputLayout = layout
        };

        // Return the cached pipeline state.
        if(_cache.Check(ref cacheInfo))
            return _states[cacheInfo.EOID];

        // Proceed to create new pipeline state.
        GraphicsPipelineStateDesc desc = new()
        {
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

            PRootSignature = null,
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
                        SemanticName = (byte*)SilkMarshal.StringToPtr(meta.Name, NativeStringEncoding.UTF8),
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

        // TODO implement a PSO-specific cache in this class to optimally check for duplicate PSOs
        //  - Create a struct named StateCacheData which contains:
        //      -- shader pass:
        //          -- This contains the blend, depth and raster states
        //          -- RTV formats
        //          -- DSV format
        //          -- Root signature - Generated from shader input layout(s)
        //          -- Sampler desc - Provided by shader
        //      -- input layout - Set via SetInputLayout()
        //      -- Stream output - Set via SetStreamOutput<T>(buffer) where T : IVertexType
        //      -- Multisample desc - ??? Could be provided by render surface or graphics settings, or SetMultisampleLevel(int sampleCount, int quality)

        // TODO Check our PSO cache for a matching state
        // TODO Implement a multi-level cache so we can bucket PSO objects by each individual property we need to check.
        //      For example: Dictionary<HlslPass, Dictionary<InputLayout, ...>>();

        DeviceDX12 device = pass.Device as DeviceDX12;
        Guid guid = ID3D12PipelineState.Guid;
        void* ptr = null;
        HResult hr = device.Ptr->CreateGraphicsPipelineState(desc, &guid, &ptr);
        if (!device.Log.CheckResult(hr, () => "Failed to create pipeline state object (PSO)"))
            return null;

        PipelineStateDX12 state = new PipelineStateDX12(device, (ID3D12PipelineState*)ptr);

        // Add the new pipeline state to the cache.
        cacheInfo.EOID = state.EOID;
        _cache.Add(ref cacheInfo);

        // Free all GS stream output semantic name pointers, if any.
        for(int i = 0; i < desc.StreamOutput.NumEntries; i++)
        {
            SODeclarationEntry* entry = &desc.StreamOutput.PSODeclaration[i];
            SilkMarshal.Free((IntPtr)entry->SemanticName);
        }

        return state;
    }
}
