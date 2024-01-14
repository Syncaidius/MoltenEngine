using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

/// <summary>A helper class that safely wraps InputLayout.</summary>
internal unsafe class PipelineInputLayoutDX12 : GraphicsObject<DeviceDX12>
{
    InputLayoutDesc _desc;
    ulong[] _expectedFormatIDs;

    internal PipelineInputLayoutDX12(DeviceDX12 device,
        GraphicsStateValueGroup<GraphicsBuffer> vbSlots,
        ID3D10Blob* vertexBytecode,
        ShaderIOLayout io) :
        base(device)
    {
        _desc = new InputLayoutDesc();
        IsValid = true;
        _expectedFormatIDs = new ulong[vbSlots.Length];
        List<InputElementDesc> elements = new List<InputElementDesc>();
        ShaderIOLayout format = null;

        // Store the EOID of each expected vertext format.
        for (int i = 0; i < vbSlots.Length; i++)
        {
            if (vbSlots.BoundValues[i] == null)
                continue;

            format = vbSlots.BoundValues[i].VertexLayout;

            /* Check if the current vertex segment's format matches 
               the part of the shader's input structure that it's meant to represent. */
            int startID = elements.Count;
            if (!io.IsCompatible(format, startID))
            {
                IsValid = false;
                break;
            }

            // Collate vertex format elements into layout and set the correct input slot for each element.
            elements.AddRange((format as ShaderIOLayoutDX12).VertexElements);

            for (int eID = startID; eID < elements.Count; eID++)
            {
                InputElementDesc e = elements[eID];
                e.InputSlot = (uint)i; // Vertex buffer input slot.
                elements[eID] = e;

                IsInstanced = IsInstanced || e.InputSlotClass == InputClassification.PerInstanceData;
            }

            _expectedFormatIDs[i] = format.EOID;
        }

        // Check if there are actually any elements. If not, use the default placeholder vertex type.
        if (elements.Count == 0)
        {
            ShaderIOLayout nullFormat = device.VertexCache.Get<VertexWithID>();
            elements.Add((nullFormat as ShaderIOLayoutDX12).VertexElements[0]);
            IsNullBuffer = true;
        }

        _desc = new InputLayoutDesc()
        {
            PInputElementDescs = EngineUtil.AllocArray<InputElementDesc>((nuint)elements.Count),
            NumElements = (uint)elements.Count,
        };

        // Copy final element list into the unsafe array of elements.
        Span<InputElementDesc> elementSpan = new Span<InputElementDesc>(_desc.PInputElementDescs, elements.Count);
        elements.CopyTo(elementSpan);
    }

    public bool IsMatch(Logger log, GraphicsStateValueGroup<GraphicsBuffer> grp)
    {
        for (int i = 0; i < grp.Length; i++)
        {
            BufferDX12 seg = grp.BoundValues[i] as BufferDX12;

            // If null vertex buffer, check if shader actually need one to be present.
            if (seg == null)
            {
                // if shader's buffer hash is null for this slot, it's allowed to be null, otherwise no match.
                if (_expectedFormatIDs[i] == 0)
                    continue;
                else
                    return false;
            }

            // Prevent vertex buffer segments with no format from crashing the application.
            if (seg.VertexLayout == null)
            {
                log.Warning($"Missing format for bound vertex segment {seg.Name} in slot {i}. Skipping validation. This may cause a false input layout match.");
                continue;
            }

            if (seg.VertexLayout.EOID != _expectedFormatIDs[i])
                return false;
        }

        return true;
    }

    protected override void OnGraphicsRelease()
    {
        EngineUtil.Free(ref _desc.PInputElementDescs);
    }

    /// <summary>Gets whether or not the input layout is valid.</summary>
    internal bool IsValid { get; }

    /// <summary>Gets whether or not the vertex input layout is designed for use with instanced draw calls.</summary>
    public bool IsInstanced { get; }

    /// <summary>
    /// Gets whether the current <see cref="PipelineInputLayoutDX12"/> can represent a null vertex buffer. e.g. Contains SV_VertexID as the only vertex element.
    /// </summary>
    public bool IsNullBuffer { get; }

    internal ref InputLayoutDesc Desc => ref _desc;
}
