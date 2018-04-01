using Molten.Graphics;
using Molten.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class SceneIndexedStressTest : SampleSceneGame
    {
        public override string Description => "A simple scene test using colored cubes with indexed meshes.";
        List<SceneObject> _objects;

        public SceneIndexedStressTest(EngineSettings settings = null) : base("Scene Stress (Indexed)", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            _objects = new List<SceneObject>();

            string fn = "assets/BasicColor.sbm";
            string source = "";
            using (FileStream stream = new FileStream(fn, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                    source = reader.ReadToEnd();
            }

            ShaderCompileResult shaders = engine.Renderer.Resources.CreateShaders(source, fn);
            IMaterial material = shaders["material", 0] as IMaterial;

            IIndexedMesh<VertexColor> mesh = Engine.Renderer.Resources.CreateCustomIndexedMesh<VertexColor>(24, 36);
            if (material == null)
            {
                Exit();
                return;
            }

            VertexColor[] vertices = new VertexColor[]{
                new VertexColor(new Vector3F(-1,-1,-1), Color.Red), //front
                new VertexColor(new Vector3F(-1,1,-1), Color.Red),
                new VertexColor(new Vector3F(1,1,-1), Color.Red),
                new VertexColor(new Vector3F(1,-1,-1), Color.Red),

                new VertexColor(new Vector3F(-1,-1,1), Color.Green), //back
                new VertexColor(new Vector3F(1,1,1), Color.Green),
                new VertexColor(new Vector3F(-1,1,1), Color.Green),
                new VertexColor(new Vector3F(1,-1,1), Color.Green),

                new VertexColor(new Vector3F(-1,1,-1), Color.Blue), //top
                new VertexColor(new Vector3F(-1,1,1), Color.Blue),
                new VertexColor(new Vector3F(1,1,1), Color.Blue),
                new VertexColor(new Vector3F(1,1,-1), Color.Blue),

                new VertexColor(new Vector3F(-1,-1,-1), Color.Yellow), //bottom
                new VertexColor(new Vector3F(1,-1,1), Color.Yellow),
                new VertexColor(new Vector3F(-1,-1,1), Color.Yellow),
                new VertexColor(new Vector3F(1,-1,-1), Color.Yellow),

                new VertexColor(new Vector3F(-1,-1,-1), Color.Purple), //left
                new VertexColor(new Vector3F(-1,-1,1), Color.Purple),
                new VertexColor(new Vector3F(-1,1,1), Color.Purple),
                new VertexColor(new Vector3F(-1,1,-1), Color.Purple),

                new VertexColor(new Vector3F(1,-1,-1), Color.White), //right
                new VertexColor(new Vector3F(1,1,1), Color.White),
                new VertexColor(new Vector3F(1,-1,1), Color.White),
                new VertexColor(new Vector3F(1,1,-1), Color.White),
            };

            int[] indices = new int[]{
                0, 1, 2, 0, 2, 3,
                4, 5, 6, 4, 7, 5,
                8, 9, 10, 8, 10, 11,
                12, 13, 14, 12, 15, 13,
                16,17,18, 16, 18, 19,
                20, 21, 22, 20, 23, 21,
            };

            mesh.Material = material;
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices);
            for (int i = 0; i < 6000; i++)
                SpawnRandomTestCube(material, mesh, 70);

            Window.PresentClearColor = new Color(20, 20, 20, 255);
        }

        private void SpawnRandomTestCube(IMaterial material, IMesh mesh, int spawnRadius)
        {
            SceneObject obj = CreateObject();
            MeshComponent meshCom = obj.AddComponent<MeshComponent>();
            meshCom.Mesh = mesh;

            int maxRange = spawnRadius * 2;
            obj.Transform.LocalPosition = new Vector3F()
            {
                X = -spawnRadius + (float)(Rng.NextDouble() * maxRange),
                Y = -spawnRadius + (float)(Rng.NextDouble() * maxRange),
                Z = spawnRadius + (float)(Rng.NextDouble() * maxRange)
            };

            _objects.Add(obj);
            SampleScene.AddObject(obj);
        }

        private void Window_OnClose(IWindowSurface surface)
        {
            Exit();
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
