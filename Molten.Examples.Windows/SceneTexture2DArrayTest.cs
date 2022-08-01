using Molten.Graphics;

namespace Molten.Samples
{
    public class SceneTexture2DArrayTest : SampleGame
    {
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;

        public override string Description => "A simple test of texture arrays via a material shared between two parented objects.";

        public SceneTexture2DArrayTest() : base("2D Texture Array") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTextureArray2D.mfx");
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
                Exit();
                return;
            }

            IMaterial mat = _hMaterial.Get<IMaterial>();
            ITexture2D tex = _hTexture.Get<ITexture2D>();
            mat.SetDefaultResource(tex, 0);
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
