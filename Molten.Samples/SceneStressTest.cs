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
    public class SceneStressTest : SampleSceneGame
    {
        public override string Description => "A simple scene test using colored cubes with";

        List<SceneObject> _objects;
        IMesh<VertexColor> _mesh;

        public SceneStressTest(EngineSettings settings = null) : base("Scene Stress", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _objects = new List<SceneObject>();

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<IMaterial>("BasicColor.sbm");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexColor>(36);
            VertexColor[] vertices = new VertexColor[]{
                        new VertexColor(new Vector3F(-1,-1,-1), Color.Red), //front
                        new VertexColor(new Vector3F(-1,1,-1), Color.Red),
                        new VertexColor(new Vector3F(1,1,-1), Color.Red),
                        new VertexColor(new Vector3F(-1,-1,-1), Color.Red),
                        new VertexColor(new Vector3F(1,1,-1), Color.Red),
                        new VertexColor(new Vector3F(1,-1,-1), Color.Red),

                        new VertexColor(new Vector3F(-1,-1,1), Color.Blue), //back
                        new VertexColor(new Vector3F(1,1,1), Color.Blue),
                        new VertexColor(new Vector3F(-1,1,1), Color.Blue),
                        new VertexColor(new Vector3F(-1,-1,1),Color.Blue),
                        new VertexColor(new Vector3F(1,-1,1), Color.Blue),
                        new VertexColor(new Vector3F(1,1,1), Color.Blue),

                        new VertexColor(new Vector3F(-1,1,-1), Color.Yellow), //top
                        new VertexColor(new Vector3F(-1,1,1), Color.Yellow),
                        new VertexColor(new Vector3F(1,1,1), Color.Yellow),
                        new VertexColor(new Vector3F(-1,1,-1), Color.Yellow),
                        new VertexColor(new Vector3F(1,1,1), Color.Yellow),
                        new VertexColor(new Vector3F(1,1,-1), Color.Yellow),

                        new VertexColor(new Vector3F(-1,-1,-1), Color.Purple), //bottom
                        new VertexColor(new Vector3F(1,-1,1), Color.Purple),
                        new VertexColor(new Vector3F(-1,-1,1), Color.Purple),
                        new VertexColor(new Vector3F(-1,-1,-1), Color.Purple),
                        new VertexColor(new Vector3F(1,-1,-1), Color.Purple),
                        new VertexColor(new Vector3F(1,-1,1), Color.Purple),

                        new VertexColor(new Vector3F(-1,-1,-1), Color.Green), //left
                        new VertexColor(new Vector3F(-1,-1,1), Color.Green),
                        new VertexColor(new Vector3F(-1,1,1), Color.Green),
                        new VertexColor(new Vector3F(-1,-1,-1), Color.Green),
                        new VertexColor(new Vector3F(-1,1,1), Color.Green),
                        new VertexColor(new Vector3F(-1,1,-1), Color.Green),

                        new VertexColor(new Vector3F(1,-1,-1), Color.White), //right
                        new VertexColor(new Vector3F(1,1,1), Color.White),
                        new VertexColor(new Vector3F(1,-1,1), Color.White),
                        new VertexColor(new Vector3F(1,-1,-1), Color.White),
                        new VertexColor(new Vector3F(1,1,-1), Color.White),
                        new VertexColor(new Vector3F(1,1,1), Color.White),
                    };

            _mesh.SetVertices(vertices);
            Window.PresentClearColor = new Color(20, 20, 20, 255);
            for (int i = 0; i < 6000; i++)
                SpawnRandomTestCube(_mesh, 70);
        }

        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            IMaterial mat = content.Get<IMaterial>(cr.RequestedFiles[0]);

            if (mat == null)
            {
                Exit();
                return;
            }

            _mesh.Material = mat;
        }

        private void SpawnRandomTestCube(IMesh mesh, int spawnRadius)
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
