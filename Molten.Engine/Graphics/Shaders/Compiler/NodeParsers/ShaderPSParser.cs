namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ShaderPSParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Pixel;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslElement foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string entryPoint))
            {
                switch (foundation)
                {
                    case Material material:
                        material.DefaultPSEntryPoint = entryPoint;
                        break;

                    case MaterialPass pass:
                        pass.PS.EntryPoint = entryPoint;
                        break;
                }
            }
            else
            {
                context.AddError("Pixel/Fragment shader <entry> tag is missing or empty.");
            }
        }
    }
}
