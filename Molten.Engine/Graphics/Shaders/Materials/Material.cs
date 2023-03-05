
namespace Molten.Graphics
{
    public class Material : HlslShader<MaterialPass>
    { 
        public Material(GraphicsDevice device, string filename) : base(device, filename) { }

        public ObjectMaterialProperties Object { get; set; }

        public LightMaterialProperties Light { get; set; }

        public SceneMaterialProperties Scene { get; set; }

        public GBufferTextureProperties Textures { get; set; }

        public SpriteBatchMaterialProperties SpriteBatch { get; set; }
    }
}
