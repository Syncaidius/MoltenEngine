using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class FontTest : TestGame
    {
        public override string Description => "A stress test of sprite rendering.";

        Scene _scene;
        SceneObject _parent;
        SceneObject _child;
        List<Matrix> _positions;
        Random _rng;
        SceneCameraComponent _cam;
        Camera2D _cam2D;
        ISpriteFont _font;
        List<ISprite> _sprites;
        IMesh<VertexTexture> _mesh;

        public FontTest(EngineSettings settings = null) : base("Font/Text", settings)
        {

        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            _cam = new SceneCameraComponent()
            {
                OutputSurface = Window,
                OutputDepthSurface = WindowDepthSurface,
            };

            _cam2D = new Camera2D()
            {
                OutputSurface = Window,
                OutputDepthSurface = WindowDepthSurface,
            };

            _sprites = new List<ISprite>();
            _rng = new Random();
            _positions = new List<Matrix>();
            _scene = CreateScene("Test");
            _scene.OutputCamera = _cam;
            _font = engine.Renderer.Resources.CreateFont("arial", 36);
            SetupSprites(_font);

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<ITexture2D>("png_test.png;mipmaps=true");
            cr.Load<IMaterial>("Basictexture.sbm");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);

            VertexTexture[] verts = new VertexTexture[]{
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)), //front
               new VertexTexture(new Vector3(-1,1,-1), new Vector2(0,0)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(1,0)),
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(1, 0)),
               new VertexTexture(new Vector3(1,-1,-1), new Vector2(1,1)),

               new VertexTexture(new Vector3(-1,-1,1), new Vector2(1,0)), //back
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),
               new VertexTexture(new Vector3(-1,1,1), new Vector2(1,1)),
               new VertexTexture(new Vector3(-1,-1,1), new Vector2(1,0)),
               new VertexTexture(new Vector3(1,-1,1), new Vector2(0, 0)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),

               new VertexTexture(new Vector3(-1,1,-1), new Vector2(0,1)), //top
               new VertexTexture(new Vector3(-1,1,1), new Vector2(0,0)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(1,0)),
               new VertexTexture(new Vector3(-1,1,-1), new Vector2(0,1)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(1, 0)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(1,1)),

               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(1,0)), //bottom
               new VertexTexture(new Vector3(1,-1,1), new Vector2(0,1)),
               new VertexTexture(new Vector3(-1,-1,1), new Vector2(1,1)),
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(1,0)),
               new VertexTexture(new Vector3(1,-1,-1), new Vector2(0, 0)),
               new VertexTexture(new Vector3(1,-1,1), new Vector2(0,1)),

               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)), //left
               new VertexTexture(new Vector3(-1,-1,1), new Vector2(0,0)),
               new VertexTexture(new Vector3(-1,1,1), new Vector2(1,0)),
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)),
               new VertexTexture(new Vector3(-1,1,1), new Vector2(1, 0)),
               new VertexTexture(new Vector3(-1,1,-1), new Vector2(1,1)),

               new VertexTexture(new Vector3(1,-1,-1), new Vector2(1,0)), //right
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),
               new VertexTexture(new Vector3(1,-1,1), new Vector2(1,1)),
               new VertexTexture(new Vector3(1,-1,-1), new Vector2(1,0)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(0, 0)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),
            };
            _mesh.SetVertices(verts);

            _parent = SpawnTestCube(_mesh);
            _child = SpawnTestCube(_mesh);

            _child.Transform.LocalScale = new Vector3(0.5f);

            _parent.Transform.LocalPosition = new Vector3(0, 0, 4);
            _parent.Children.Add(_child);

            Window.PresentClearColor = new Color(20,20,20,255);
        }

        private void SetupSprites(ISpriteFont font)
        {
            for(int i = 0; i < 1000; i++)
            {
                SpriteText s = new SpriteText()
                {
                    Position = new Vector2()
                    {
                        X = _rng.Next(0, 1720),
                        Y = _rng.Next(0, 880),
                    },

                    Color = new Color()
                    {
                        R = (byte)_rng.Next(0, 256),
                        G = (byte)_rng.Next(0, 256),
                        B = (byte)_rng.Next(0, 256),
                        A = 255,
                    },

                    Font = font,
                    Text = $"TEST {_rng.Next(6000000, int.MaxValue)}",
                };

                _sprites.Add(s);
                _scene.RenderData.AddSprite(s);
            }
        }

        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            if (cr.RequestedFiles.Count == 0)
                return;

            ITexture2D tex = content.Get<ITexture2D>(cr.RequestedFiles[0]);
            IMaterial mat = content.Get<IMaterial>(cr.RequestedFiles[1]);

            if (mat == null)
            {
                Exit();
                return;
            }

            mat.SetDefaultResource(tex, 0);
            _mesh.Material = mat;
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

            base.OnUpdate(time);
        }
    }
}
