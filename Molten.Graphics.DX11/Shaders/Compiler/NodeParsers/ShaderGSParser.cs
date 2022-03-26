using System.Xml;

namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="Material"/> headers.</summary>
    internal class ShaderGSParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Geometry;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, ShaderHeaderNode node)
        {
            switch (foundation)
            {
                case Material material:
                    material.DefaultGSEntryPoint = node.Value;
                    break;

                case MaterialPass pass:
                    pass.GS.EntryPoint = node.Value;
                    break;
            }
        }
    }
}
