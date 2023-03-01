namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="Material"/> headers.</summary>
    internal class ShaderHSParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Hull;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslElement foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string entryPoint))
            {
                switch (foundation)
                {
                    case Material material:
                        material.DefaultHSEntryPoint = entryPoint;
                        break;

                    case MaterialPass pass:
                        pass.HS.EntryPoint = entryPoint;
                        break;
                }
            }
            else
            {
                context.AddError("Hull shader <entry> tag is missing or empty.");
            }
        }
    }
}
