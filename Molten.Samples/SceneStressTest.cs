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
    public class SceneStressTest : TestGame
    {
        public override string Description => "A simple scene test using colored cubes with";

        Scene _scene;
        List<SceneObject> _objects;
        SceneObject _player;
        Random _rng;
        SpriteText _txtInstructions;
        Vector2 _txtInstructionSize;
        IMesh<VertexColor> _mesh;

        public SceneStressTest(EngineSettings settings = null) : base("Scene Stress", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            Window.OnPostResize += Window_OnPostResize;
            
            _rng = new Random();
            _objects = new List<SceneObject>();
            _scene = CreateScene("Test");
            SpawnPlayer();

            string text = "[W][A][S][D] to move. Mouse to rotate";
            _txtInstructionSize = TestFont.MeasureString(text);
            _txtInstructions = new SpriteText()
            {
                Text = text,
                Font = TestFont,
                Color = Color.White,
            };

            _scene.AddSprite(_txtInstructions);
            UpdateInstructions();


            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<IMaterial>("BasicColor.sbm");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexColor>(36);
            VertexColor[] vertices = new VertexColor[]{
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

            _mesh.SetVertices(vertices);
            Window.PresentClearColor = new Color(20, 20, 20, 255);
            for (int i = 0; i < 6000; i++)
                SpawnTestCube(_mesh, 70);
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

        private void UpdateInstructions()
        {
            if (_txtInstructions == null)
                return;

            _txtInstructions.Position = new Vector2()
            {
                X = Window.Width / 2 + (-_txtInstructionSize.X / 2),
                Y = 3,
            };
        }

        private void Window_OnPostResize(ITexture texture)
        {
            UpdateInstructions();
        }

        private void SpawnPlayer()
        {
            _player = CreateObject(new Vector3(0,0,-5));
            SceneCameraComponent cam = _player.AddComponent<SceneCameraComponent>();
            cam.OutputSurface = Window;
            cam.OutputDepthSurface = WindowDepthSurface;
            _scene.AddObject(_player);
            _scene.OutputCamera = cam;
        }

        private void SpawnTestCube(IMesh mesh, int spawnRadius)
        {
            SceneObject obj = CreateObject();
            MeshComponent meshCom = obj.AddComponent<MeshComponent>();
            meshCom.Mesh = mesh;

            int maxRange = spawnRadius * 2;
            obj.Transform.LocalPosition = new Vector3()
            {
                X = -spawnRadius + (float)(_rng.NextDouble() * maxRange),
                Y = -spawnRadius + (float)(_rng.NextDouble() * maxRange),
                Z = spawnRadius + (float)(_rng.NextDouble() * maxRange)
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
            var rotateAngle = 1.2f * time.Delta;

            foreach(SceneObject obj in _objects)
            {
                obj.Transform.LocalRotationX += rotateAngle;
                obj.Transform.LocalRotationY += rotateAngle;
                obj.Transform.LocalRotationZ += rotateAngle * 0.7f * time.Delta;
            }

            // Keyboard input - Again messy code for now
            Vector3 moveDelta = Vector3.Zero;
            float rotSpeed = 0.25f;
            float speed = 1.0f;

            // Mouse input - Messy for now - We're just testing input
            _player.Transform.LocalRotationX -= Mouse.Moved.Y * rotSpeed;
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
