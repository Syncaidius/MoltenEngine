using Molten.Graphics;

namespace Molten.Samples
{
    public class TwoDTextureTest : SampleSceneGame
    {
        public override string Description => "A simple test for 2D texture loading and usage.";

        SceneObject _parent;
        SceneObject _child;
        IMesh<CubeArrayVertex> _mesh;

        public TwoDTextureTest() : base("2D Texture Test") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);    

            _mesh = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            _mesh.SetVertices(SampleVertexData.TextureArrayCubeVertices);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTexture.mfx");
            cr.Load<ITexture2D>("png_test.png");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
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
            _mesh.Material = mat;
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);
            base.OnUpdate(time);
        }
    }
}
