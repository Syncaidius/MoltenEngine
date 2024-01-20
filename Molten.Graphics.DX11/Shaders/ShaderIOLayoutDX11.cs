using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

public unsafe class ShaderIOLayoutDX11 : ShaderIOLayout
{
    internal InputElementDesc[] VertexElements { get; private set; }

    public ShaderIOLayoutDX11() { }

    protected override void InitializeAsVertexLayout(int numVertexElements)
    {
        VertexElements = new InputElementDesc[numVertexElements];
    }

    protected override void BuildVertexElement(VertexElementAttribute att, uint index, uint byteOffset)
    {
        VertexElements[index] = new InputElementDesc()
        {
            SemanticName = (byte*)SilkMarshal.StringToPtr(Metadata[index].Name),
            SemanticIndex = att.SemanticIndex,
            AlignedByteOffset = byteOffset,
            InstanceDataStepRate = att.InstanceStepRate,
            InputSlotClass = att.Classification.ToApi(),
            Format = att.Type.ToGraphicsFormat().ToApi()
        };
    }

    public override bool Equals(object obj)
    {
        if (obj is ShaderIOLayoutDX11 layout)
            return Equals(layout);

        return false;
    }

    public bool Equals(ShaderIOLayoutDX11 other)
    {
        // If we're comparing the object to itself, return true.
        if (EOID == other.EOID)
            return true;

        if (VertexElements == null || other.VertexElements == null)
        {
            if (VertexElements.Length != other.VertexElements.Length)
                return false;

            for (int i = 0; i < VertexElements.Length; i++)
            {
                ref InputElementDesc element = ref VertexElements[i];
                ref InputElementDesc otherElement = ref other.VertexElements[i];

                if (Metadata[i].Name != other.Metadata[i].Name)
                    return false;

                if (element.SemanticIndex != otherElement.SemanticIndex
                || element.InputSlot != otherElement.InputSlot
                || element.InstanceDataStepRate != otherElement.InstanceDataStepRate
                || element.AlignedByteOffset != otherElement.AlignedByteOffset
                || element.InputSlotClass != otherElement.InputSlotClass
                || element.Format != otherElement.Format)
                    return false;
            }

            return true;
        }

        return base.Equals(other);
    }

    protected override void OnDispose(bool immediate)
    {
            // Dispose of element string pointers, since they were statically-allocated by Silk.NET
        if (VertexElements != null)
        {
            for (uint i = 0; i < VertexElements.Length; i++)
                SilkMarshal.Free((nint)VertexElements[i].SemanticName);
        }
    }
}
