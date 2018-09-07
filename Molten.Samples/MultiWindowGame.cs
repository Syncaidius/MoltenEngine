using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class MultiWindowGame : SampleGame
    {
        public override string Description => "A simple multi-window test.";

        List<IWindowSurface> _extraWindows;
        SceneObject _player;
        List<Scene> _scenes;
        List<SceneObject> _parents;
        List<SceneObject> _children;
        IMesh<VertexColor> _mesh;

        public MultiWindowGame(EngineSettings settings = null) : base("Multi-Window", settings)
        {
        }

        protected override void OnInitialize(Engine engine)
        {
            _scenes = new List<Scene>();
            _parents = new List<SceneObject>();
            _children = new List<SceneObject>();
            _extraWindows = new List<IWindowSurface>();
            Random rng = new Random();

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicColor.sbm");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexColor>(36);
            _mesh.SetVertices(SampleVertexData.ColoredCube);

            SpawnScene(Window);
            for (int i = 0; i < 5; i++)
            {
                string title = $"Window #{i}";
                IWindowSurface window = engine.Renderer.Resources.CreateFormSurface(title);
                engine.Renderer.OutputSurfaces.Add(window);

                SpawnScene(window);

                _extraWindows.Add(window);
                window.Mode = WindowMode.Windowed;
                window.OnClose += Window_OnClose;
                window.Visible = true;
            }

            base.OnInitialize(engine);
        }

        private void SpawnScene(IWindowSurface surface)
        {
            Scene scene = CreateScene("Test");
            scene.BackgroundColor = new Color()
            {
                R = (byte)Rng.Next(0,255),
                G = (byte)Rng.Next(0, 255),
                B = (byte)Rng.Next(0, 255),
                A = 255,
            };
            _player = CreateObject();
            _player.Transform.LocalPosition = new Vector3F(0, 0, -20);
            SceneCameraComponent cam = _player.AddComponent<SceneCameraComponent>();
            cam.MaxDrawDistance = 300;
            cam.OutputSurface = surface;
            scene.AddObject(_player);
            _scenes.Add(scene);

            SpawnParentChild(scene, _mesh, Vector3F.Zero);
        }

        protected SceneObject SpawnTestCube(Scene scene, IMesh mesh, Vector3F pos)
        {
            SceneObject obj = CreateObject(pos, scene);
            MeshComponent meshCom = obj.AddComponent<MeshComponent>();
            meshCom.Mesh = mesh;
            return obj;
        }

        protected void SpawnParentChild(Scene scene, IMesh mesh, Vector3F origin)
        {
            SceneObject parent = SpawnTestCube(scene, mesh, Vector3F.Zero);

            int childCount = Rng.Next(1, 10);
            for(int i = 0; i < childCount; i++)
            {
                SceneObject child = SpawnTestCube(scene, mesh, Vector3F.Zero);
                child.Transform.LocalScale = new Vector3F((1.0f / 100f) * Rng.Next(1, 101));
                child.Transform.LocalPosition = new Vector3F()
                {
                    X = Rng.Next(2, 10),
                    Y = 0,
                    Z = Rng.Next(2, 10)
                };

                child.Transform.LocalRotationZ = Rng.Next(0, 360);
                parent.Transform.LocalPosition = origin;
                parent.Children.Add(child);
                _children.Add(child);
            }

            _parents.Add(parent);
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

        protected override void OnContentRequested(ContentRequest cr) { }

        protected override void OnContentLoaded(ContentRequest cr) { }

        private void Window_OnClose(IWindowSurface surface)
        {
            Exit();
        }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);
            float rotateTime = (float)time.TotalTime.TotalSeconds;
            float parentSpeed = 0.5f;
            float childSpeed = 0.8f;

            foreach (SceneObject parent in _parents)
            {
                parent.Transform.LocalRotationY += parentSpeed;
                if (parent.Transform.LocalRotationY >= 360)
                    parent.Transform.LocalRotationY -= 360;
            }

            foreach (SceneObject child in _children)
            {
                child.Transform.LocalRotationY += childSpeed;
                if (child.Transform.LocalRotationY >= 360)
                    child.Transform.LocalRotationY -= 360;
            }
        }
    }
}
