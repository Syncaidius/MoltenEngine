using Molten.Graphics;

namespace Molten.Samples
{
    public class SceneTexture1DArrayTest : SampleSceneGame
    {
        public override string Description => "A sample of 1D texture arrays via a material shared between two parented objects.";

        public SceneTexture1DArrayTest() : base("1D Texture Array") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);    

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTextureArray1D.mfx");
            cr.Load<TextureData>("1d_1.png");
            cr.Load<TextureData>("1d_2.png");
            cr.Load<TextureData>("1d_3.png");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();
        }

        protected override IMesh GetTestCubeMesh()
        {
            IMesh<CubeArrayVertex> cube = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            cube.SetVertices(SampleVertexData.TextureArrayCubeVertices);
            return cube;
        }

        private void Cr_OnCompleted(ContentRequest cr)
        {
            IMaterial mat = cr.Get<IMaterial>(0);

            if (mat == null)
            {
                Exit();
                return;
            }

            // Manually construct a 2D texture array from the 3 textures we requested earlier
            TextureData texData = cr.Get<TextureData>(1);
            ITexture texture = Engine.Renderer.Resources.CreateTexture1D(new Texture1DProperties()
            {
                Width = texData.Width,
                MipMapLevels = texData.MipMapLevels,
                ArraySize = 3,
                Flags = texData.Flags,
                Format = texData.Format,
            });
            texture.SetData(texData, 0, 0, texData.MipMapLevels, 1, 0, 0);

            texData = cr.Get<TextureData>(2);
            texture.SetData(texData, 0, 0, texData.MipMapLevels, 1, 0, 1);

            texData = cr.Get<TextureData>(3);
            texture.SetData(texData, 0, 0, texData.MipMapLevels, 1, 0, 2);

            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;
        }
    }
}
