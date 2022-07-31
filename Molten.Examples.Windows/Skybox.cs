using Molten.Graphics;

namespace Molten.Samples
{
    public class SkyboxSample : SampleGame
    {
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;

        public override string Description => "A skybox demonstration.";

        public SkyboxSample() : base("Skybox") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/dds_dxt5.dds");

            loader.Load<ITextureCube>("assets/cubemap.dds", 
                (tex, isReload) => MainScene.SkyboxTeture = tex);

            loader.OnCompleted += Loader_OnCompleted;
        }

        protected override IMesh GetTestCubeMesh()
        {
            IMesh<CubeArrayVertex> cube = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            cube.SetVertices(SampleVertexData.TextureArrayCubeVertices);
            return cube;
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
    }
}
