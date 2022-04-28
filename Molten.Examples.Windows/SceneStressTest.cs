using Molten.Graphics;

namespace Molten.Samples
{
    public class SceneStressTest : SampleSceneGame
    {
        public override string Description => "A simple scene test using colored cubes with";

        List<SceneObject> _objects;

        public SceneStressTest() : base("Scene Stress") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _objects = new List<SceneObject>();
            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicColor.mfx");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            for (int i = 0; i < 10000; i++)
                SpawnRandomTestCube(TestMesh, 70);
        }

        protected override IMesh GetTestCubeMesh()
        {
            IMesh<VertexColor> cube = Engine.Renderer.Resources.CreateMesh<VertexColor>(36);
            cube.SetVertices(SampleVertexData.ColoredCube);
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

            TestMesh.Material = mat;
        }

        private void SpawnRandomTestCube(IMesh mesh, int spawnRadius)
        {
            SceneObject obj = CreateObject();
            MeshComponent meshCom = obj.Components.Add<MeshComponent>();
            meshCom.RenderedObject = mesh;

            int maxRange = spawnRadius * 2;
            obj.Transform.LocalPosition = new Vector3F()
            {
                X = -spawnRadius + (float)(Rng.NextDouble() * maxRange),
                Y = -spawnRadius + (float)(Rng.NextDouble() * maxRange),
                Z = spawnRadius + (float)(Rng.NextDouble() * maxRange)
            };

            _objects.Add(obj);
            MainScene.AddObject(obj);
        }

        protected override void OnUpdate(Timing time)
        {
            var rotateAngle = 1.2f * time.Delta;

            foreach(SceneObject obj in _objects)
            {
                obj.Transform.LocalRotationX += rotateAngle;
                obj.Transform.LocalRotationY += rotateAngle;
                obj.Transform.LocalRotationZ += rotateAngle * 0.7f * time.Delta;
            }

            base.OnUpdate(time);
        }
    }
}
