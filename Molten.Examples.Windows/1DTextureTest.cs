using Molten.Graphics;

namespace Molten.Samples
{
    public class OneDTextureTest : SampleGame
    {
        ContentLoadHandle<IMaterial> _hMaterial;
        ContentLoadHandle<ITexture2D> _hTexture;

        public override string Description => "A simple test for 1D texture loading and usage.";

        public OneDTextureTest() : base("1D Texture Test") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture1D.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/1d_1.png");
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (_hMaterial.HasAsset())
            {
                Exit();
                return;
            }

            // Manually construct a 2D texture array from the 3 textures we requested earlier
            IMaterial mat = _hMaterial.Get();
            ITexture texture = _hTexture.Get();

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
