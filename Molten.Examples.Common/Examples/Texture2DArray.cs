using Molten.Graphics;

namespace Molten.Examples
{
    [Example("Texture Arrays - 2D", "Demonstrates how 2D texture arrays are used")]
    public class Texture2DArray : MoltenExample
    {
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            base.OnLoadContent(loader);

            _hMaterial = loader.Load<Material>("assets/BasicTextureArray2D.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/128.dds", parameters: new TextureParameters()
            {
                PartCount = 3,
            });
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Close();
                return;
            }

            Material mat = _hMaterial.Get<Material>();
            ITexture2D texture = _hTexture.Get<ITexture2D>();

            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;
        }

        protected override Mesh GetTestCubeMesh()
        {
            Mesh<CubeArrayVertex> cube = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            cube.SetVertices(SampleVertexData.TextureArrayCubeVertices);
            return cube;
        }
    }
}
