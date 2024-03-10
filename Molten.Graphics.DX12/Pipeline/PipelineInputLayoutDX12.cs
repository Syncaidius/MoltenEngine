using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

/// <summary>A helper class that safely wraps InputLayout.</summary>
internal unsafe class PipelineInputLayoutDX12 : GraphicsObject<DeviceDX12>
{
    struct FormatBinding
    {
        public ulong FormatEOID;
        public uint SlotID;
    }

    InputLayoutDesc _desc;
    FormatBinding[] _expectedBindings;
    ShaderIOLayout _sourceLayout;

    internal PipelineInputLayoutDX12(DeviceDX12 device,
        GpuStateValueGroup<GraphicsBuffer> vbSlots,
        ShaderPassDX12 pass) :
        base(device)
    {
        IsValid = true;
        ShaderPassStage vs = pass[ShaderStageType.Vertex];
        _sourceLayout = vs.InputLayout;
        List<FormatBinding> expected = new List<FormatBinding>();
        List<InputElementDesc> elements = new List<InputElementDesc>();

        // Store the EOID of each expected vertext format.
        for (int i = 0; i < vbSlots.Length; i++)
        {
            if (vbSlots.BoundValues[i] == null)
                continue;

            ShaderIOLayoutDX12 inputLayout = vbSlots.BoundValues[i].VertexLayout as ShaderIOLayoutDX12;

            /* Check if the current vertex segment's format matches 
               the part of the shader's input structure that it's meant to represent. */
            int startID = elements.Count;
            if (!_sourceLayout.IsCompatible(inputLayout, startID))
            {
                IsValid = false;
                break;
            }

            // Collate vertex format elements into layout and set the correct input slot for each element.
            elements.AddRange(inputLayout.VertexElements);

            for (int eID = startID; eID < elements.Count; eID++)
            {
                InputElementDesc e = elements[eID];
                e.InputSlot = (uint)i; // Vertex buffer input slot.
                elements[eID] = e;

                IsInstanced = IsInstanced || e.InputSlotClass == InputClassification.PerInstanceData;
            }

            expected.Add(new FormatBinding()
            {
                FormatEOID = inputLayout.EOID,
                SlotID = (uint)i,
            });
        }

        // Check if there are actually any elements. If not, use the default placeholder vertex type.
        if (elements.Count == 0)
        {
            ShaderIOLayoutDX12 nullFormat = device.LayoutCache.GetVertexLayout<VertexWithID>() as ShaderIOLayoutDX12;
            elements.Add(nullFormat.VertexElements[0]);
            expected.Add(new FormatBinding()
            {
                FormatEOID = nullFormat.EOID,
                SlotID = 0,
            });
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

    public bool IsMatch(GpuStateValueGroup<GraphicsBuffer> grp)
    {
        int lastIndex = _expectedBindings.Length - 1;

        for (int i = 0; i < _expectedBindings.Length; i++)
        {
            int iNext = i < lastIndex ? i + 1 : i;
            ref FormatBinding binding = ref _expectedBindings[i];
            ref FormatBinding nextBinding = ref _expectedBindings[iNext];

            int slotID = (int)binding.SlotID;
            GraphicsBuffer buffer = grp.BoundValues[(int)binding.SlotID];

            // If the expected slot is null, then the there is no match.
            if (!IsNullBuffer)
            {
                if (buffer == null)
                    return false;

                // If the expected vertex layout ID doesn't match, then there is no match.
                if (buffer.VertexLayout == null || buffer.VertexLayout.EOID != binding.FormatEOID)
                    return false;
            }
            else
            {
                // If we're expecting a null buffer, then the current slot should be null.
                if (buffer != null)
                    return false;
            }

            // All buffers between the current and next slot should be null
            int nextSlotID = (int)nextBinding.SlotID;
            for (int s = slotID + 1; s < nextSlotID; s++)
            {
                if (grp.BoundValues[s] != null)
                    return false;
            }
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
