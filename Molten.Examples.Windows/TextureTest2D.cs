using Molten.Graphics;

namespace Molten.Samples
{
    public class TextureTest2D : SampleGame
    {
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;

        public override string Description => "A simple test for 2D texture loading and usage.";

        public TextureTest2D() : base("2D Texture Test") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/png_test.png");
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Exit();
                return;
            }

            IMaterial mat = _hMaterial.Get<IMaterial>();
            ITexture2D texture = _hTexture.Get<ITexture2D>();
            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;
        }

        protected override IMesh GetTestCubeMesh()
        {
            IMesh<CubeArrayVertex> cube = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            cube.SetVertices(SampleVertexData.TextureArrayCubeVertices);
            return cube;
        }
    }
}
