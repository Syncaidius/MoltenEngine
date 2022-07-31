using Molten.Graphics;

namespace Molten.Samples
{
    public class SceneTexture1DArrayTest : SampleGame
    {
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture1;
        ContentLoadHandle _hTexture2;
        ContentLoadHandle _hTexture3;

        public override string Description => "A sample of 1D texture arrays via a material shared between two parented objects.";

        public SceneTexture1DArrayTest() : base("1D Texture Array") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTextureArray1D.mfx");
            _hTexture1 = loader.Load<TextureData>("assets/1d_1.png");
            _hTexture2 = loader.Load<TextureData>("assets/1d_2.png");
            _hTexture3 = loader.Load<TextureData>("assets/1d_3.png");
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Exit();
                return;
            }

            // Manually construct a 2D texture array from the 3 textures we requested earlier
            IMaterial mat = _hMaterial.Get<IMaterial>();
            TextureData texData = _hTexture1.Get<TextureData>();

            ITexture texture = Engine.Renderer.Resources.CreateTexture1D(new Texture1DProperties()
            {
                Width = texData.Width,
                MipMapLevels = texData.MipMapLevels,
                ArraySize = 3,
                Flags = texData.Flags,
                Format = texData.Format,
            });
            texture.SetData(texData, 0, 0, texData.MipMapLevels, 1, 0, 0);

            texData = _hTexture2.Get<TextureData>();
            texture.SetData(texData, 0, 0, texData.MipMapLevels, 1, 0, 1);

            texData = _hTexture3.Get<TextureData>();
            texture.SetData(texData, 0, 0, texData.MipMapLevels, 1, 0, 2);

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
