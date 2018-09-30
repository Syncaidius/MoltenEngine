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
            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicColor.sbm");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexColor>(36);
            _mesh.SetVertices(SampleVertexData.ColoredCube);

            for (int i = 0; i < 10000; i++)
                SpawnRandomTestCube(_mesh, 70);
        }

        private void Cr_OnCompleted(ContentRequest cr)
        {
            IMaterial mat = cr.Get<IMaterial>(0);

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
