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
    public class SceneTextureArrayTest : TestGame
    {
        public override string Description => "A simple test of texture arrays via a shared material between two parented objects.";

        Scene _scene;
        SceneObject _parent;
        SceneObject _child;
        List<Matrix> _positions;
        Random _rng;
        SceneObject _player;
        SpriteText _txtInstructions;
        Vector2 _txtInstructionSize;
        IMesh<CubeArrayVertex> _mesh;

        public SceneTextureArrayTest(EngineSettings settings = null) : base("2D Texture Array", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _rng = new Random();
            _positions = new List<Matrix>();
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

            _mesh = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
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

            _mesh.SetVertices(verts);

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<IMaterial>("BasicTextureArray2D.sbm");
            cr.Load<TextureData>("128_1.dds");
            cr.Load<TextureData>("128_2.dds");
            cr.Load<TextureData>("128_3.dds");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

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

            // Manually construct a 2D texture array from the 3 textures we requested earlier
            TextureData texData = content.Get<TextureData>(cr.RequestedFiles[1]);
            ITexture2D texture = Engine.Renderer.Resources.CreateTexture2D(new Texture2DProperties()
            {
                Width = texData.Width,
                Height = texData.Height,
                MipMapLevels = texData.MipMapCount,
                ArraySize = 3,
                Flags = texData.Flags,
                Format = texData.Format,
            });
            texture.SetData(texData, 0, 0, texData.MipMapCount, 1, 0, 0);

            texData = content.Get<TextureData>(cr.RequestedFiles[2]);
            texture.SetData(texData, 0, 0, texData.MipMapCount, 1, 0, 1);

            texData = content.Get<TextureData>(cr.RequestedFiles[3]);
            texture.SetData(texData, 0, 0, texData.MipMapCount, 1, 0, 2);

            mat.SetDefaultResource(texture, 0);
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

            // Keyboard input - Again messy code for now
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
