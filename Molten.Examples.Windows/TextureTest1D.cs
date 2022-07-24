using Molten.Graphics;

namespace Molten.Samples
{
    public class TextureTest1D : SampleGame
    {
        ContentLoadHandle<IMaterial> _hMaterial;
        ContentLoadHandle<ITexture> _hTexture;

        public override string Description => "A simple test for 1D texture loading and usage.";

        public TextureTest1D() : base("1D Texture Test") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture1D.mfx");
            _hTexture = loader.Load<ITexture>("assets/1d_1.png");
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (_hMaterial.HasAsset())
            {
                Exit();
                return;
            }

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
