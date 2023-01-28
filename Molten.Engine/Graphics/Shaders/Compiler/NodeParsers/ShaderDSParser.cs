namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ShaderDSParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Domain;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            switch (foundation)
            {
                case Material material:
                    material.DefaultDSEntryPoint = node.Value;
                    break;

                case MaterialPass pass:
                    pass.DS.EntryPoint = node.Value;
                    break;
            }
        }
    }
}
