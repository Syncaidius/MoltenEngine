namespace Molten.Graphics
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VertexElementAttribute : Attribute
    {
        public VertexElementAttribute(VertexElementType type, VertexElementUsage usage, uint semanticIndex,
            VertexInputType classification = VertexInputType.PerVertexData, string customSemantic = null)
        {
            Type = type;
            Usage = usage;
            SemanticIndex = semanticIndex;
            Classification = classification;

            if (usage != VertexElementUsage.Custom)
                customSemantic = null;

            CustomSemantic = customSemantic;
        }

        /// <summary>The element type.</summary>
        public VertexElementType Type;

        /// <summary>Gets the vertex element usage.</summary>
        public VertexElementUsage Usage;

        /// <summary>The custom semantic name given to the element, if any.</summary>
        public string CustomSemantic;

        /// <summary>Gets or sets the semantic slot of the element (e.g. usage as a position with slot 0 would create SV_POSITION0 in hlsl).</summary>
        public uint SemanticIndex;

        /// <summary>Gets the data classification of the element.</summary>
        public VertexInputType Classification;
    }
}
