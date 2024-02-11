namespace Molten.Graphics;

[AttributeUsage(AttributeTargets.Field)]
public class VertexElementAttribute : Attribute
{
    public VertexElementAttribute(VertexElementType type, VertexElementUsage usage, uint semanticIndex,
        VertexInputType classification = VertexInputType.PerVertexData, uint instanceStepRate = 0, string customSemantic = null)
    {
        Type = type;
        Usage = usage;
        SemanticIndex = semanticIndex;
        Classification = classification;
        InstanceStepRate = instanceStepRate;

        if (usage != VertexElementUsage.Custom)
            customSemantic = null;

        CustomSemantic = customSemantic;
        ComponentCount = type.GetComponentCount();
    }

    /// <summary>Gets the element type.</summary>
    public VertexElementType Type { get; }

    /// <summary>Gets the vertex element usage.</summary>
    public VertexElementUsage Usage { get; }

    /// <summary>Gets the custom semantic name given to the element, if any.</summary>
    public string CustomSemantic { get; }

    /// <summary>Gets the semantic slot of the element (e.g. usage as a position with slot 0 would create SV_POSITION0 in hlsl).</summary>
    public uint SemanticIndex { get; }

    /// <summary>Gets the data classification of the element.</summary>
    public VertexInputType Classification { get; }

    /// <summary>
    /// Gets the instance step rate of the current vertex element.
    /// </summary>
    public uint InstanceStepRate { get; }

    /// <summary>
    /// Gets the number of components in the element.
    /// </summary>
    public uint ComponentCount { get; }
}
