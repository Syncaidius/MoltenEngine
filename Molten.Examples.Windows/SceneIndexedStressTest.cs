using Molten.Graphics;
using System.IO;

namespace Molten.Samples
{
    public class SceneIndexedStressTest : SampleGame
    {
        public override string Description => "A simple scene test using colored cubes with indexed meshes.";
        List<SceneObject> _objects;

        public SceneIndexedStressTest() : base("Scene Stress (Indexed)") { }

        protected override void OnLoadContent(ContentLoadBatch loader) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            _objects = new List<SceneObject>();

            string fn = "assets/BasicColor.mfx";
            string source = "";
            using (FileStream stream = new FileStream(fn, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                    source = reader.ReadToEnd();
            }

            ShaderCompileResult shaders = engine.Renderer.Resources.CompileShaders(ref source, fn);
            IMaterial material = shaders[ShaderClassType.Material, 0] as IMaterial;

            IIndexedMesh<VertexColor> mesh = Engine.Renderer.Resources.CreateIndexedMesh<VertexColor>(24, 36);
            if (material == null)
            {
                Exit();
                return;
            }

            mesh.Material = material;
            mesh.SetVertices(SampleVertexData.IndexedTexturedCubeVertices);
            mesh.SetIndices(SampleVertexData.TexturedCubeIndices);
            for (int i = 0; i < 10000; i++)
                SpawnRandomTestCube(material, mesh, 70);
        }

        private void SpawnRandomTestCube(IMaterial material, IMesh mesh, int spawnRadius)
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
