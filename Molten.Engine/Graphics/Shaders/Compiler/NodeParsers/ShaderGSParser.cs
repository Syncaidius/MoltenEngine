namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="Material"/> headers.</summary>
    internal class ShaderGSParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Geometry;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslElement foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string entryPoint))
            {
                switch (foundation)
                {
                    case Material material:
                        material.DefaultGSEntryPoint = entryPoint;
                        break;

                    case MaterialPass pass:
                        pass.GS.EntryPoint = entryPoint;
                        break;
                }
            }
            else
            {
                context.AddError("Geometry shader <entry> tag is missing or empty.");
            }
        }
    }
}
