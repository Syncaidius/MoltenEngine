using Molten.Graphics;

namespace Molten.Samples
{
    public class SceneTexture2DArrayTest : SampleGame
    {
        public override string Description => "A simple test of texture arrays via a material shared between two parented objects.";

        public SceneTexture2DArrayTest() : base("2D Texture Array") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTextureArray2D.mfx");
            cr.Load<ITexture2D>("128.dds", new TextureParameters()
            {
                ArraySize = 3,
            });
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

            ITexture2D texture = cr.Get<ITexture2D>(1);
            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;
        }
    }
}
