using Molten.Graphics;

namespace Molten.Samples
{
    public class OneDTextureTest : SampleSceneGame
    {
        public override string Description => "A simple test for 1D texture loading and usage.";

        public OneDTextureTest() : base("1D Texture Test") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);    

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTexture1D.mfx");
            cr.Load<ITexture>("1d_1.png");
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
            ITexture texture = cr.Get<ITexture>(1);

            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;
        }
    }
}
