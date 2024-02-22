using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

/// <summary>A helper class that safely wraps InputLayout.</summary>
internal unsafe class PipelineInputLayoutDX11 : GraphicsObject<DeviceDX11>
{
    struct FormatBinding
    {
        public ulong FormatEOID;
        public uint SlotID;
    }

    ID3D11InputLayout* _native;
    FormatBinding[] _expectedBindings;
    ShaderIOLayout _sourceLayout;

    internal PipelineInputLayoutDX11(DeviceDX11 device,
        GraphicsStateValueGroup<GraphicsBuffer> vbSlots, 
        ShaderPassDX11 pass) : 
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

            ShaderIOLayoutDX11 inputLayout = vbSlots.BoundValues[i].VertexLayout as ShaderIOLayoutDX11;

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
            ShaderIOLayoutDX11 nullFormat = device.LayoutCache.GetVertexLayout<VertexWithID>() as ShaderIOLayoutDX11;
            elements.Add(nullFormat.VertexElements[0]);
            expected.Add(new FormatBinding()
            {
                 FormatEOID = nullFormat.EOID,
                 SlotID = 0,
            });
            IsNullBuffer = true;
        }

        _expectedBindings = expected.ToArray();
        InputElementDesc[] finalElements = elements.ToArray();

        // Attempt creation of input layout.
        if (IsValid)
        {
            ID3D10Blob* byteCode = (ID3D10Blob*)pass.InputByteCode;
            void* ptrByteCode = byteCode->GetBufferPointer();
            nuint numBytes = byteCode->GetBufferSize();

            fixed (InputElementDesc* ptrElements = &finalElements[0])
                device.Ptr->CreateInputLayout(ptrElements, (uint)finalElements.Length, ptrByteCode, numBytes, ref _native);

            if (_native == null)
            {
                device.Log.Error("Failed to create new vertex input layout");
                device.ProcessDebugLayerMessages();
            }
        }
        else
        {
            device.Log.Warning($"Vertex formats do not match the input layout of shader:");
            for (int i = 0; i < vbSlots.Length; i++)
            {
                if (vbSlots.BoundValues[i] == null)
                    continue;

                ShaderIOLayout inputLayout = vbSlots.BoundValues[i].VertexLayout;

                device.Log.Warning("Format - Buffer slot " + i + ": ");
                for (int f = 0; f < inputLayout.Metadata.Length; f++)
                    device.Log.Warning($"\t[{f}]{inputLayout.Metadata[f].Name} -- index: {inputLayout.Metadata[f].SemanticIndex} -- slot: {i}");
            }

            // List final input structure.
            device.Log.Warning("Shader Input Structure: ");
            for (int i = 0; i < finalElements.Length; i++)
            {
                int slotID = (int)finalElements[i].InputSlot;
                ShaderIOLayout inputLayout = vbSlots.BoundValues[slotID].VertexLayout;
                device.Log.Warning($"\t[{i}]{inputLayout.Metadata[i].Name} -- index: {finalElements[i].SemanticIndex} -- slot: {slotID}");
            }
        }
    }

    public bool IsMatch(Logger log, GraphicsStateValueGroup<GraphicsBuffer> grp)
    {
        int lastIndex = _expectedBindings.Length - 1;

        for(int i = 0; i < _expectedBindings.Length; i++)
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
        NativeUtil.ReleasePtr(ref _native);
    }

    public static implicit operator ID3D11InputLayout*(PipelineInputLayoutDX11 resource)
    {
        return resource.NativePtr;
    }

    /// <summary>Gets whether or not the input layout is valid.</summary>
    internal bool IsValid { get; }

    /// <summary>Gets whether or not the vertex input layout is designed for use with instanced draw calls.</summary>
    public bool IsInstanced { get; }

    /// <summary>
    /// Gets whether the current <see cref="PipelineInputLayoutDX11"/> can represent a null vertex buffer. e.g. Contains SV_VertexID as the only vertex element.
    /// </summary>
    public bool IsNullBuffer { get; }

    internal ref ID3D11InputLayout* NativePtr => ref _native;
}
