using System.Runtime.InteropServices;

namespace Molten.Graphics.DX11;

/// <summary>A placeholder vertex type for times when no vertex input is required.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct VertexWithID : IVertexType
{
    [VertexElement(VertexElementType.UInt, VertexElementUsage.VertexID, 0)]
    /// <summary>Gets or sets the position as a Vector4</summary>
    public uint Id;
}
