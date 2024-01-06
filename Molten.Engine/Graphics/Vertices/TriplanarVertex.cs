using System.Runtime.InteropServices;

namespace Molten.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
/// <summary>A vertex type containing position, color, normal and a texture array index. Does not include any UV coordinates. 
/// Designed for use with triplanar texturing.</summary>
public struct TriplanarVertex : IVertexType
{
    [VertexElement(VertexElementType.Vector4, VertexElementUsage.Position, 0)]
    /// <summary>Gets or sets the position as a Vector4</summary>
    Vector4F Position4;

    [VertexElement(VertexElementType.Color, VertexElementUsage.Color, 0)]
    public Color Color;

    [VertexElement(VertexElementType.Vector3, VertexElementUsage.Normal, 0)]
    public Vector3F Normal;

    [VertexElement(VertexElementType.Vector3, VertexElementUsage.Tangent, 0)]
    public Vector3F Tangent;

    [VertexElement(VertexElementType.Vector3, VertexElementUsage.Binormal, 0)]
    public Vector3F BiNormal;

    [VertexElement(VertexElementType.Float, VertexElementUsage.TextureCoordinate, 0)]
    public float ArrayIndex;

    public TriplanarVertex(Vector4F position, Color color, Vector3F normal, float arrayIndex)
    {
        this.Position4 = position;
        this.Color = color;
        this.Normal = normal;
        this.Tangent = new Vector3F();
        this.BiNormal = new Vector3F();
        this.ArrayIndex = arrayIndex;
    }

    public TriplanarVertex(Vector3F position, Color color, Vector3F normal, float arrayIndex)
    {
        this.Position4 = new Vector4F(position, 1);
        this.Color = color;
        this.Normal = normal;
        this.Tangent = new Vector3F();
        this.BiNormal = new Vector3F();
        this.ArrayIndex = arrayIndex;
    }

    public TriplanarVertex(Vector3F position, Vector3F normal, float arrayIndex)
    {
        this.Position4 = new Vector4F(position, 1);
        this.Color = new Color(255, 255, 255, 255);
        this.Normal = normal;
        this.Tangent = new Vector3F();
        this.BiNormal = new Vector3F();
        this.ArrayIndex = arrayIndex;
    }

    public override string ToString()
    {
        return string.Format("{Position:{0} Color:{1} Normal: {2} Tan: {3} BiN: {4} Idx: {5}}", new object[] { this.Position4,
            this.Color, this.Normal, this.Tangent,
            this.BiNormal, this.ArrayIndex });
    }
}
