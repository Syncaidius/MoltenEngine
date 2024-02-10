﻿using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX12.Pipeline;

internal class PipelineStateBuilderDX12
{
    GraphicsPipelineStateDesc _desc;
    ShaderPassDX12 _pass;

    /// <summary>
    /// Begins construction of a new pipeline state object (PSO) for the provided shader pass.
    /// </summary>
    /// <param name="pass"></param>
    /// <exception cref="Exception"></exception>
    internal void Begin(ShaderPassDX12 pass)
    {
        if (_pass != null)
            throw new Exception("Cannot call Begin() again before End() is called");

        _pass = pass;
        _desc = new GraphicsPipelineStateDesc()
        {
            Flags = PipelineStateFlags.None,
            RasterizerState = _pass.RasterizerState.Desc,
            BlendState = _pass.BlendState.Description.Desc,
            SampleMask = _pass.BlendState.Description.BlendSampleMask,
            DepthStencilState = _pass.DepthState.Description.Desc,
            VS = _pass.GetBytecode(ShaderType.Vertex),
            GS = _pass.GetBytecode(ShaderType.Geometry),
            DS = _pass.GetBytecode(ShaderType.Domain),
            HS = _pass.GetBytecode(ShaderType.Hull),
            PS = _pass.GetBytecode(ShaderType.Pixel),

            PRootSignature = null,
            NodeMask = 0,               // TODO Set this to the node mask of the device.
            CachedPSO = default,        // TODO Implement PSO caching
            DSVFormat = Format.FormatUnknown,
            IBStripCutValue = IndexBufferStripCutValue.ValueDisabled,
            StreamOutput = default,     // TODO Implement stream output
            SampleDesc = default,       // TODO Implement multisampling
        };

        // Populate render target formats if a pixel shader is present in the pass.
        ShaderComposition ps = _pass[ShaderType.Pixel];
        if (ps != null)
        {
            _desc.NumRenderTargets = (uint)ps.OutputLayout.Metadata.Length;
            unsafe
            {
                for (int i = 0; i < _desc.NumRenderTargets; i++)
                    _desc.RTVFormats[i] = (Format)pass.FormatLayout.RawFormats[i];
            }
        }
    }

    internal unsafe void SetCacheBlob(CachedPipelineState cachedState)
    {
        if(cachedState.PCachedBlob == null)
            throw new Exception("The provided cached state does not contain a valid blob pointer.");

        _desc.CachedPSO = cachedState;
    }

    internal unsafe PipelineStateDX12 End()
    {
        if(_pass == null)
            throw new Exception("Begin() must be called before End() is called.");

        DeviceDX12 device = _pass.Device as DeviceDX12;

        Guid guid = ID3D12PipelineState.Guid;
        void* ptr = null;
        HResult hr = device.Ptr->CreateGraphicsPipelineState(_desc, &guid, &ptr);
        if (!device.Log.CheckResult(hr, () => "Failed to create pipeline state object (PSO)"))
            return null;

        PipelineStateDX12 state = new PipelineStateDX12(device, (ID3D12PipelineState*)ptr);

        // Check cache for a matching state.
        device.Cache.Check(ref state);

        return state;
    }
}