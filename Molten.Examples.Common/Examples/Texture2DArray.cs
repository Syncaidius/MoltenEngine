using Molten.Graphics;

namespace Molten.Examples
{
    [Example("Texture Arrays - 2D", "Demonstrates how 2D texture arrays are used")]
    public class Texture2DArray : MoltenExample
    {
        ContentLoadHandle _hShader;
        ContentLoadHandle _hTexture;

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            base.OnLoadContent(loader);

            _hShader = loader.Load<HlslShader>("assets/BasicTextureArray2D.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/128.dds", parameters: new TextureParameters()
            {
                PartCount = 3,
            });
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hShader.HasAsset())
            {
                Close();
                return;
            }

            HlslShader shader = _hShader.Get<HlslShader>();
            ITexture2D texture = _hTexture.Get<ITexture2D>();

            shader.SetDefaultResource(texture, 0);
            TestMesh.Shader = shader;
        }

        protected override Mesh GetTestCubeMesh()
        {
            return Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(SampleVertexData.TextureArrayCubeVertices);
        }
    }
}
