using Molten.Graphics;
using Molten.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class SceneParentingTest : SampleGame
    {
        public override string Description => "A simple test of the scene object parenting system";

        Scene _scene;
        SceneObject _parent;
        SceneObject _child;
        List<Matrix> _positions;
        Random _rng;
        SceneObject _player;
        IMesh<VertexColor> _mesh;

        public SceneParentingTest(EngineSettings settings = null) : base("Scene Parenting", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _rng = new Random();
            _positions = new List<Matrix>();
            _scene = CreateScene("Test");
            SpawnPlayer();

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<IMaterial>("BasicColor.sbm");
            cr.OnCompleted += Cr_OnCompleted; ;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexColor>(36);
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

            _mesh.SetVertices(verts);

            _parent = SpawnTestCube(_mesh);
            _child = SpawnTestCube(_mesh);

            _child.Transform.LocalScale = new Vector3(0.5f);

            _parent.Transform.LocalPosition = new Vector3(0, 0, 4);
            _parent.Children.Add(_child);

            Window.PresentClearColor = new Color(20,20,20,255);
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

        private void SpawnPlayer()
        {
            _player = CreateObject();
            SceneCameraComponent cam = _player.AddComponent<SceneCameraComponent>();
            cam.OutputSurface = Window;
            cam.OutputDepthSurface = WindowDepthSurface;
            _scene.AddObject(_player);
            _scene.OutputCamera = cam;
        }

        private SceneObject SpawnTestCube(IMesh mesh)
        {
            SceneObject obj = CreateObject();
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

            Vector3 moveDelta = Vector3.Zero;
            float rotSpeed = 0.25f;
            float speed = 1.0f;

            // Mouse input - Messy for now - We're just testing input
            _player.Transform.LocalRotationX += Mouse.Moved.Y * rotSpeed;
            _player.Transform.LocalRotationY += Mouse.Moved.X * rotSpeed;
            Mouse.CenterInWindow();

            if (Keyboard.IsPressed(Key.W)) moveDelta += _player.Transform.Global.Backward * speed;
            if (Keyboard.IsPressed(Key.S)) moveDelta += _player.Transform.Global.Forward * speed;
            if (Keyboard.IsPressed(Key.A)) moveDelta += _player.Transform.Global.Left * speed;
            if (Keyboard.IsPressed(Key.D)) moveDelta += _player.Transform.Global.Right * speed;

            _player.Transform.LocalPosition += moveDelta * time.Delta * speed;

            base.OnUpdate(time);
        }
    }
}
