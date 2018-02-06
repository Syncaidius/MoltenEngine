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
    public class SceneStressTest : TestGame
    {
        public override string Description => "A simple scene test using spinning, colored cubes";

        Scene _scene;
        List<SceneObject> _objects;
        Random _rng;
        Camera3D _cam;
        Vector3 _camPos;
        Vector3 _camRotation;
        SpriteText _txtInstructions;

        public SceneStressTest(EngineSettings settings = null) : base("Scene Stress", settings)
        {
        }

        protected override void OnInitialize(Engine engine)
        {
            Window.OnPostResize += Window_OnPostResize;
            _cam = new Camera3D()
            {
                MaximumDrawDistance = 1000,
                OutputSurface = Window,
                OutputDepthSurface = WindowDepthSurface,
                View = Matrix.Identity,
            };

            _rng = new Random();
            _objects = new List<SceneObject>();
            _scene = new Scene("Test", engine);
            _scene.OutputCamera = _cam;

            string text = "[W][A][S][D] to move. Mouse to rotate";
            Vector2 size = ISpriteFont.MeasureString()
            _txtInstructions = new SpriteText()
            {
                Text = text,
                Font = TestFont,
                Position = new Vector2(Window.Width / 2, 0),
            };

            _scene.AddSprite(_txtInstructions);

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
                SpawnTestCube(material, mesh, 20);

            Window.PresentClearColor = new Color(20, 20, 20, 255);
            base.OnInitialize(engine);
        }

        private void Window_OnPostResize(ITexture texture)
        {
            throw new NotImplementedException();
        }

        private void SpawnTestCube(IMaterial material, IMesh mesh, int spawnRadius)
        {
            SceneObject obj = Engine.CreateObject();
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

            // Mouse input - Messy for now - We're just testing input
            _camRotation.X += Mouse.Moved.Y;
            _camRotation.Y += Mouse.Moved.X;
            Mouse.CenterInWindow();

            // Keyboard input - Again messy code for now
            Matrix camTransform = Matrix.Invert(_cam.View);
            Vector3 moveDelta = Vector3.Zero;
            if (Keyboard.IsPressed(Key.W)) moveDelta += camTransform.Backward;
            if (Keyboard.IsPressed(Key.S)) moveDelta += camTransform.Forward;
            if (Keyboard.IsPressed(Key.A)) moveDelta += camTransform.Left;
            if (Keyboard.IsPressed(Key.D)) moveDelta += camTransform.Right;

            _camPos += moveDelta * time.Delta;
            _cam.SetView(_camPos, _camRotation, Vector3.Up, Vector3.ForwardLH);
        }
    }
}
