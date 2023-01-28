namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ShaderVSParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Vertex;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            switch (foundation)
            {
                case Material material:
                    material.DefaultVSEntryPoint = node.Value;
                    break;

                case MaterialPass pass:
                    pass.VS.EntryPoint = node.Value;
                    break;
            }
        }
    }
}
