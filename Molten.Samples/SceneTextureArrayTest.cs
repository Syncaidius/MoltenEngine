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
    public class SceneTextureArrayTest : TestGame
    {
        public override string Description => "A simple test of texture arrays via a shared material between two parented objects.";

        Scene _scene;
        SceneObject _parent;
        SceneObject _child;
        List<Matrix> _positions;
        Random _rng;
        Camera _cam;
        SceneObject _player;

        public SceneTextureArrayTest(EngineSettings settings = null) : base("Texture Arrays", settings)
        {

        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            // Default texture
            TextureData texData;
            string fn = "assets/128_1.dds";
            using (FileStream stream = new FileStream(fn, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    TextureDataLoader texLoader = new TextureDataLoader();
                    texData = texLoader.Read(Log, engine, reader, fn);
                }
            }

            ITexture2D texDefault = engine.Renderer.Resources.CreateTexture2D(new Texture2DProperties()
            {
                Width = texData.Width,
                Height = texData.Height,
                MipMapLevels = texData.MipMapCount,
                ArraySize = 3,
                Flags = texData.Flags,
                Format = texData.Format,
            });

            texDefault.SetData(texData, 0,0, texData.MipMapCount, 1, 0, 0);

            fn = "assets/128_2.dds";
            using (FileStream stream = new FileStream(fn, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    TextureDataLoader texLoader = new TextureDataLoader();
                    texData = texLoader.Read(Log, engine, reader, fn);
                }
            }

            texDefault.SetData(texData, 0, 0, texData.MipMapCount, 1, 0, 1);

            fn = "assets/128_3.dds";
            using (FileStream stream = new FileStream(fn, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    TextureDataLoader texLoader = new TextureDataLoader();
                    texData = texLoader.Read(Log, engine, reader, fn);
                }
            }

            texDefault.SetData(texData, 0, 0, texData.MipMapCount, 1, 0, 2);

            _rng = new Random();
            _positions = new List<Matrix>();
            _scene = new Scene("Test", engine);
            _scene.OutputCamera = _cam;

            fn = "assets/BasicTextureArray2D.sbm";
            string source = "";
            using (FileStream stream = new FileStream(fn, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                    source = reader.ReadToEnd();
            }

            ShaderParseResult shaders = engine.Renderer.Resources.CreateShaders(source, fn);
            IMaterial material = shaders["material", 0] as IMaterial;
            
            if (material == null)
            {
                Exit();
                return;
            }

            IMesh<CubeArrayVertex> mesh = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);

            material.SetDefaultResource(texDefault, 0);

            CubeArrayVertex[] verts = new CubeArrayVertex[]{
               new CubeArrayVertex(new Vector3(-1,-1,-1), new Vector3(0,1,0)), //front
               new CubeArrayVertex(new Vector3(-1,1,-1), new Vector3(0,0,0)),
               new CubeArrayVertex(new Vector3(1,1,-1), new Vector3(1,0,0)),
               new CubeArrayVertex(new Vector3(-1,-1,-1), new Vector3(0,1,0)),
               new CubeArrayVertex(new Vector3(1,1,-1), new Vector3(1, 0,0)),
               new CubeArrayVertex(new Vector3(1,-1,-1), new Vector3(1,1,0)),

               new CubeArrayVertex(new Vector3(-1,-1,1), new Vector3(1,0,1)), //back
               new CubeArrayVertex(new Vector3(1,1,1), new Vector3(0,1,1)),
               new CubeArrayVertex(new Vector3(-1,1,1), new Vector3(1,1,1)),
               new CubeArrayVertex(new Vector3(-1,-1,1), new Vector3(1,0,1)),
               new CubeArrayVertex(new Vector3(1,-1,1), new Vector3(0, 0,1)),
               new CubeArrayVertex(new Vector3(1,1,1), new Vector3(0,1,1)),

               new CubeArrayVertex(new Vector3(-1,1,-1), new Vector3(0,1,2)), //top
               new CubeArrayVertex(new Vector3(-1,1,1), new Vector3(0,0,2)),
               new CubeArrayVertex(new Vector3(1,1,1), new Vector3(1,0,2)),
               new CubeArrayVertex(new Vector3(-1,1,-1), new Vector3(0,1,2)),
               new CubeArrayVertex(new Vector3(1,1,1), new Vector3(1, 0,2)),
               new CubeArrayVertex(new Vector3(1,1,-1), new Vector3(1,1,2)),

               new CubeArrayVertex(new Vector3(-1,-1,-1), new Vector3(1,0,0)), //bottom
               new CubeArrayVertex(new Vector3(1,-1,1), new Vector3(0,1,0)),
               new CubeArrayVertex(new Vector3(-1,-1,1), new Vector3(1,1,0)),
               new CubeArrayVertex(new Vector3(-1,-1,-1), new Vector3(1,0,0)),
               new CubeArrayVertex(new Vector3(1,-1,-1), new Vector3(0, 0,0)),
               new CubeArrayVertex(new Vector3(1,-1,1), new Vector3(0,1,0)),

               new CubeArrayVertex(new Vector3(-1,-1,-1), new Vector3(0,1,1)), //left
               new CubeArrayVertex(new Vector3(-1,-1,1), new Vector3(0,0,1)),
               new CubeArrayVertex(new Vector3(-1,1,1), new Vector3(1,0,1)),
               new CubeArrayVertex(new Vector3(-1,-1,-1), new Vector3(0,1,1)),
               new CubeArrayVertex(new Vector3(-1,1,1), new Vector3(1, 0,1)),
               new CubeArrayVertex(new Vector3(-1,1,-1), new Vector3(1,1,1)),

               new CubeArrayVertex(new Vector3(1,-1,-1), new Vector3(1,0,2)), //right
               new CubeArrayVertex(new Vector3(1,1,1), new Vector3(0,1,2)),
               new CubeArrayVertex(new Vector3(1,-1,1), new Vector3(1,1,2)),
               new CubeArrayVertex(new Vector3(1,-1,-1), new Vector3(1,0,2)),
               new CubeArrayVertex(new Vector3(1,1,-1), new Vector3(0, 0,2)),
               new CubeArrayVertex(new Vector3(1,1,1), new Vector3(0,1,2)),
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
