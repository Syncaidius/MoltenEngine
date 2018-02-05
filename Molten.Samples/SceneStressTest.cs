using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class SceneStressTest : TestGame
    {
        public override string Description => "A simple scene test using spinning, colored cubes";

        Scene _scene;
        List<SceneObject> _objects;
        Random _rng;
        Camera _cam;

        public SceneStressTest(EngineSettings settings = null) : base("Scene Stress", settings)
        {
        }

        protected override void OnInitialize(Engine engine)
        {
            _cam = new Camera3D()
            {
                OutputSurface = Window,
                OutputDepthSurface = WindowDepthSurface,
            };

            _rng = new Random();
            _objects = new List<SceneObject>();
            _scene = new Scene("Test", engine);
            _scene.OutputCamera = _cam;

            string fn = "assets/BasicColor.sbm";
            string source = "";
            using (FileStream stream = new FileStream(fn, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                    source = reader.ReadToEnd();
            }

            ShaderParseResult shaders = engine.Renderer.Resources.CreateShaders(source, fn);
            IMaterial material = shaders["material", 0] as IMaterial;

            IMesh<VertexColor> mesh = Engine.Renderer.Resources.CreateMesh<VertexColor>(36);
            if (material == null)
            {
                Exit();
                return;
            }

            VertexColor[] verts = new VertexColor[]{
                           new VertexColor(new Vector3(-1,-1,-1), Color.Red), //front
                           new VertexColor(new Vector3(-1,1,-1), Color.Red),
                           new VertexColor(new Vector3(1,1,-1), Color.Red),
                           new VertexColor(new Vector3(-1,-1,-1), Color.Red),
                           new VertexColor(new Vector3(1,1,-1), Color.Red),
                           new VertexColor(new Vector3(1,-1,-1), Color.Red),

                           new VertexColor(new Vector3(-1,-1,1), Color.Blue), //back
                           new VertexColor(new Vector3(1,1,1), Color.Blue),
                           new VertexColor(new Vector3(-1,1,1), Color.Blue),
                           new VertexColor(new Vector3(-1,-1,1),Color.Blue),
                           new VertexColor(new Vector3(1,-1,1), Color.Blue),
                           new VertexColor(new Vector3(1,1,1), Color.Blue),

                           new VertexColor(new Vector3(-1,1,-1), Color.Yellow), //top
                           new VertexColor(new Vector3(-1,1,1), Color.Yellow),
                           new VertexColor(new Vector3(1,1,1), Color.Yellow),
                           new VertexColor(new Vector3(-1,1,-1), Color.Yellow),
                           new VertexColor(new Vector3(1,1,1), Color.Yellow),
                           new VertexColor(new Vector3(1,1,-1), Color.Yellow),

                           new VertexColor(new Vector3(-1,-1,-1), Color.Purple), //bottom
                           new VertexColor(new Vector3(1,-1,1), Color.Purple),
                           new VertexColor(new Vector3(-1,-1,1), Color.Purple),
                           new VertexColor(new Vector3(-1,-1,-1), Color.Purple),
                           new VertexColor(new Vector3(1,-1,-1), Color.Purple),
                           new VertexColor(new Vector3(1,-1,1), Color.Purple),

                           new VertexColor(new Vector3(-1,-1,-1), Color.Green), //left
                           new VertexColor(new Vector3(-1,-1,1), Color.Green),
                           new VertexColor(new Vector3(-1,1,1), Color.Green),
                           new VertexColor(new Vector3(-1,-1,-1), Color.Green),
                           new VertexColor(new Vector3(-1,1,1), Color.Green),
                           new VertexColor(new Vector3(-1,1,-1), Color.Green),

                           new VertexColor(new Vector3(1,-1,-1), Color.White), //right
                           new VertexColor(new Vector3(1,1,1), Color.White),
                           new VertexColor(new Vector3(1,-1,1), Color.White),
                           new VertexColor(new Vector3(1,-1,-1), Color.White),
                           new VertexColor(new Vector3(1,1,-1), Color.White),
                           new VertexColor(new Vector3(1,1,1), Color.White),
                        };

            mesh.Material = material;
            mesh.SetVertices(verts);
            for (int i = 0; i < 6000; i++)
                SpawnTestCube(material, mesh);

            Window.PresentClearColor = new Color(20,20,20,255);
            base.OnInitialize(engine);
        }

        private void SpawnTestCube(IMaterial material, IMesh mesh)
        {
            SceneObject obj = Engine.CreateObject();
            MeshComponent meshCom = obj.AddComponent<MeshComponent>();
            meshCom.Mesh = mesh;

            obj.Transform.LocalPosition = new Vector3()
            {
                X = -4 + (float)(_rng.NextDouble() * 8),
                Y = -1 + (float)(_rng.NextDouble() * 2),
                Z = 3 + (float)(_rng.NextDouble() * 30)
            };

            _objects.Add(obj);
            _scene.AddObject(obj);
        }

        private void Window_OnClose(IWindowSurface surface)
        {
            Exit();
        }

        protected override void OnUpdate(Timing time)
        {
            var rotateAngle = 1.2f * time.DeltaTime;

            foreach(SceneObject obj in _objects)
            {
                obj.Transform.LocalRotationX += rotateAngle;
                obj.Transform.LocalRotationY += rotateAngle;
                obj.Transform.LocalRotationZ += rotateAngle * 0.7f;
            }
        }
    }
}
