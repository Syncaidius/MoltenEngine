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
    public class SceneParentingTest : TestGame
    {
        public override string Description => "A simple test of the scene object parenting system";

        Scene _scene;
        //Matrix _mView;
        //Matrix _mProjection;
        SceneObject _parent;
        SceneObject _child;
        List<Matrix> _positions;
        Random _rng;
        Camera _cam;
        SceneObject _player;

        public SceneParentingTest(EngineSettings settings = null) : base("Scene Parenting", settings)
        {
        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _rng = new Random();
            _positions = new List<Matrix>();
            _scene = new Scene("Test", engine);
            _scene.OutputCamera = _cam;
            SpawnPlayer();

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

            _parent = SpawnTestCube(mesh);
            _child = SpawnTestCube(mesh);

            _child.Transform.LocalScale = new Vector3(0.5f);

            _parent.Transform.LocalPosition = new Vector3(0, 0, 4);
            _parent.Children.Add(_child);

            Window.PresentClearColor = new Color(20,20,20,255);
        }

        private void SpawnPlayer()
        {
            _player = Engine.CreateObject();
            SceneCameraComponent cam = _player.AddComponent<SceneCameraComponent>();
            cam.OutputSurface = Window;
            cam.OutputDepthSurface = WindowDepthSurface;
            _scene.AddObject(_player);
            _scene.OutputCamera = cam;
        }

        private SceneObject SpawnTestCube(IMesh mesh)
        {
            SceneObject obj = Engine.CreateObject();
            MeshComponent meshCom = obj.AddComponent<MeshComponent>();
            meshCom.Mesh = mesh;
            _positions.Add(Matrix.CreateTranslation(new Vector3()
            {
                X = -4 + (float)(_rng.NextDouble() * 8),
                Y = -1 + (float)(_rng.NextDouble() * 2),
                Z = 3 + (float)(_rng.NextDouble() * 4)
            }));

            _scene.AddObject(obj);
            return obj;
        }

        private void Window_OnClose(IWindowSurface surface)
        {
            Exit();
        }

        protected override void OnUpdate(Timing time)
        {
            var rotateTime = (float)time.TotalTime.TotalSeconds;

            _parent.Transform.LocalRotationY += 0.5f;
            if (_parent.Transform.LocalRotationY >= 360)
                _parent.Transform.LocalRotationY -= 360;

            _child.Transform.LocalRotationX += 1f;
            if (_child.Transform.LocalRotationX >= 360)
                _child.Transform.LocalRotationX -= 360;

            _parent.Transform.LocalPosition = new Vector3(0, 1, 0);
            _child.Transform.LocalPosition = new Vector3(-3, 0, 0);

            // Mouse input - Messy for now - We're just testing input
            _player.Transform.LocalRotationX += Mouse.Moved.Y;
            _player.Transform.LocalRotationY += Mouse.Moved.X;
            Mouse.CenterInWindow();

            // Keyboard input - Again messy code for now
            Vector3 moveDelta = Vector3.Zero;
            float rotSpeed = 0.25f;
            float speed = 1.0f;

            if (Keyboard.IsPressed(Key.W)) moveDelta += _player.Transform.Global.Backward * rotSpeed;
            if (Keyboard.IsPressed(Key.S)) moveDelta += _player.Transform.Global.Forward * rotSpeed;
            if (Keyboard.IsPressed(Key.A)) moveDelta += _player.Transform.Global.Left * rotSpeed;
            if (Keyboard.IsPressed(Key.D)) moveDelta += _player.Transform.Global.Right * rotSpeed;

            _player.Transform.LocalPosition += moveDelta * time.Delta * speed;
        }
    }
}
