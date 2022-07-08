using Molten.Graphics;

namespace Molten.Samples
{
    public class TwoDTextureTest : SampleGame
    {
        public override string Description => "A simple test for 2D texture loading and usage.";

        public TwoDTextureTest() : base("2D Texture Test") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);    

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTexture.mfx");
            cr.Load<ITexture2D>("png_test.png");
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
            IMaterial mat = cr.Get<IMaterial>("BasicTexture.mfx");

            if (mat == null)
            {
                Exit();
                return;
            }

            // Manually construct a 2D texture array from the 3 textures we requested earlier
            ITexture texture = cr.Get<ITexture2D>("png_test.png");
            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;
        }
    }
}
