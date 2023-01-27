using System.Xml;

namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="Material"/> headers.</summary>
    internal class ShaderHSParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Hull;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            switch (foundation)
            {
                case Material material:
                    material.DefaultHSEntryPoint = node.Value;
                    break;

                case MaterialPass pass:
                    pass.HS.EntryPoint = node.Value;
                    break;
            }
        }
    }
}
