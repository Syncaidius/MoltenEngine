using Molten.Graphics;

namespace Molten.Examples
{
    [Example("Stress Test - Non-Indexed", "A stress test which spawns a large number of rotating cubes, which use an non-indexed mesh")]
    public class StressTest : MoltenExample
    {
        const int CUBE_COUNT = 10000;

        ContentLoadHandle _hMaterial;
        List<SceneObject> _objects;

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _objects = new List<SceneObject>();
            for (int i = 0; i < 10000; i++)
                SpawnRandomTestCube(TestMesh, 70);
        }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            base.OnLoadContent(loader);
            _hMaterial = loader.Load<Material>("assets/BasicColor.mfx");


            loader.OnCompleted += Loader_OnCompleted;
        }
        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Close();
                return;
            }

            TestMesh.Material = _hMaterial.Get<Material>();
        }

        private void SpawnRandomTestCube(Mesh mesh, int spawnRadius)
        {
            int maxRange = spawnRadius * 2;
            SceneObject obj = MainScene.CreateObject(new Vector3F()
            {
                X = -spawnRadius + (float)(Rng.NextDouble() * maxRange),
                Y = -spawnRadius + (float)(Rng.NextDouble() * maxRange),
                Z = spawnRadius + (float)(Rng.NextDouble() * maxRange)
            });
            MeshComponent meshCom = obj.Components.Add<MeshComponent>();
            meshCom.RenderedObject = mesh;

            _objects.Add(obj);
        }

        protected override void OnUpdate(Timing time)
        {
            var rotateAngle = 1.2f * time.Delta;
            foreach (SceneObject obj in _objects)
            {
                obj.Transform.LocalRotationX += rotateAngle;
                obj.Transform.LocalRotationY += rotateAngle;
                obj.Transform.LocalRotationZ += rotateAngle * 0.7f * time.Delta;
            }

            base.OnUpdate(time);
        }
    }
}
