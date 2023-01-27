using System.Xml;

namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ShaderPSParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Pixel;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            switch (foundation)
            {
                case Material material:
                    material.DefaultPSEntryPoint = node.Value;
                    break;

                case MaterialPass pass:
                    pass.PS.EntryPoint = node.Value;
                    break;
            }
        }
    }
}
