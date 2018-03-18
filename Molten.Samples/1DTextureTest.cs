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
    public class OneDTextureTest : SampleGame
    {
        public override string Description => "A simple 1D texture test, to check that it all works.";

        Scene _scene;
        SceneObject _parent;
        SceneObject _child;
        List<Matrix4F> _positions;
        Random _rng;
        SceneObject _player;
        IMesh<VertexTexture> _mesh;

        public OneDTextureTest(EngineSettings settings = null) : base("1D Texture", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _rng = new Random();
            _positions = new List<Matrix4F>();
            _scene = CreateScene("Test");
            SpawnPlayer();

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<ITexture>("1d_1.png;mipmaps=true");
            cr.Load<IMaterial>("BasicTexture1D.sbm");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);
            VertexTexture[] verts = new VertexTexture[]{
                new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)), //front
                new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(0,0)),
                new VertexTexture(new Vector3F(1,1,-1), new Vector2F(1,0)),
                new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)),
                new VertexTexture(new Vector3F(1,1,-1), new Vector2F(1, 0)),
                new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(1,1)),

                new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(1,0)), //back
                new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),
                new VertexTexture(new Vector3F(-1,1,1), new Vector2F(1,1)),
                new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(1,0)),
                new VertexTexture(new Vector3F(1,-1,1), new Vector2F(0, 0)),
                new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),

                new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(0,1)), //top
                new VertexTexture(new Vector3F(-1,1,1), new Vector2F(0,0)),
                new VertexTexture(new Vector3F(1,1,1), new Vector2F(1,0)),
                new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(0,1)),
                new VertexTexture(new Vector3F(1,1,1), new Vector2F(1, 0)),
                new VertexTexture(new Vector3F(1,1,-1), new Vector2F(1,1)),

                new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(1,0)), //bottom
                new VertexTexture(new Vector3F(1,-1,1), new Vector2F(0,1)),
                new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(1,1)),
                new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(1,0)),
                new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(0, 0)),
                new VertexTexture(new Vector3F(1,-1,1), new Vector2F(0,1)),

                new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)), //left
                new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(0,0)),
                new VertexTexture(new Vector3F(-1,1,1), new Vector2F(1,0)),
                new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)),
                new VertexTexture(new Vector3F(-1,1,1), new Vector2F(1, 0)),
                new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(1,1)),

                new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(1,0)), //right
                new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),
                new VertexTexture(new Vector3F(1,-1,1), new Vector2F(1,1)),
                new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(1,0)),
                new VertexTexture(new Vector3F(1,1,-1), new Vector2F(0, 0)),
                new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),
            };

            _mesh.SetVertices(verts);

            _parent = SpawnTestCube(_mesh);
            _child = SpawnTestCube(_mesh);

            _child.Transform.LocalScale = new Vector3F(0.5f);

            _parent.Transform.LocalPosition = new Vector3F(0, 0, 4);
            _parent.Children.Add(_child);

            Window.PresentClearColor = new Color(20, 20, 20, 255);
        }

        protected override void OnContentRequested(ContentRequest cr) { }

        protected override void OnContentLoaded(ContentManager content, ContentRequest cr) { }

        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            ITexture tex = content.Get<ITexture>(cr.RequestedFiles[0]);
            IMaterial mat = content.Get<IMaterial>(cr.RequestedFiles[1]);

            if (mat == null)
            {
                Exit();
                return;
            }

            mat.SetDefaultResource(tex, 0);
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
            _positions.Add(Matrix4F.CreateTranslation(new Vector3F()
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

            _parent.Transform.LocalPosition = new Vector3F(0, 1, 0);
            _child.Transform.LocalPosition = new Vector3F(-3, 0, 0);

            Vector3F moveDelta = Vector3F.Zero;
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
