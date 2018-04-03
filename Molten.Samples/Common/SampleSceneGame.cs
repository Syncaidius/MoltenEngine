using Molten.Graphics;
using Molten.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public abstract class SampleSceneGame : SampleGame
    {
        Scene _scene;
        SceneObject _player;
        SpriteText _txtInstructions;
        Vector2F _txtInstructionSize;

        public SampleSceneGame(string title, EngineSettings settings) : base(title, settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            Window.OnPostResize += Window_OnPostResize;
            _scene = CreateScene("Test");
            _scene.SendToBack();
            SpawnPlayer();
        }

        protected override void OnContentLoaded(ContentManager content, ContentRequest cr)
        {
            string text = "[W][A][S][D] to move -- [ESC] Close -- Move mouse to rotate";
            _txtInstructionSize = TestFont.MeasureString(text);
            _txtInstructions = new SpriteText()
            {
                Text = text,
                Font = TestFont,
                Color = Color.White,
            };
            UpdateInstructions();
            UIScene.AddSprite(_txtInstructions);
        }

        protected override void OnContentRequested(ContentRequest cr) { }

        private void UpdateInstructions()
        {
            if (_txtInstructions == null)
                return;

            _txtInstructions.Position = new Vector2F()
            {
                X = Window.Width / 2 + (-_txtInstructionSize.X / 2),
                Y = Window.Height - _txtInstructionSize.Y,
            };
        }

        private void Window_OnPostResize(ITexture texture)
        {
            UpdateInstructions();
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

        protected SceneObject SpawnTestCube(IMesh mesh)
        {
            return SpawnTestCube(mesh, Vector3F.Zero);
        }

        protected SceneObject SpawnTestCube(IMesh mesh, Vector3F pos)
        {
            SceneObject obj = CreateObject(pos);
            MeshComponent meshCom = obj.AddComponent<MeshComponent>();
            meshCom.Mesh = mesh;
            _scene.AddObject(obj);
            return obj;
        }

        protected void SpawnParentChild(IMesh mesh, Vector3F origin, out SceneObject parent, out SceneObject child)
        {
            parent = SpawnTestCube(mesh);
            child = SpawnTestCube(mesh);

            child.Transform.LocalScale = new Vector3F(0.5f);
            child.Transform.LocalPosition = new Vector3F(0, 0, 4);
            parent.Transform.LocalPosition = origin;
            parent.Children.Add(child);
        }

        protected void RotateParentChild(SceneObject parent, SceneObject child, Timing time)
        {
            var rotateTime = (float)time.TotalTime.TotalSeconds;

            parent.Transform.LocalRotationY += 0.5f;
            if (parent.Transform.LocalRotationY >= 360)
                parent.Transform.LocalRotationY -= 360;

            child.Transform.LocalRotationX += 1f;
            if (child.Transform.LocalRotationX >= 360)
                child.Transform.LocalRotationX -= 360;

            parent.Transform.LocalPosition = new Vector3F(0, 1, 0);
            child.Transform.LocalPosition = new Vector3F(-3, 0, 0);
        }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            if (Keyboard.IsTapped(Key.Escape))
                Exit();

            // Keyboard input - Again messy code for now
            Vector3F moveDelta = Vector3F.Zero;
            float rotSpeed = 0.25f;
            float speed = 1.0f;

            if (AcceptPlayerInput)
            {
                // Mouse input - Messy for now - We're just testing input
                _player.Transform.LocalRotationX += Mouse.Moved.Y * rotSpeed;
                _player.Transform.LocalRotationY += Mouse.Moved.X * rotSpeed;
                Mouse.CenterInWindow();

                if (Keyboard.IsPressed(Key.W)) moveDelta += _player.Transform.Global.Backward * speed;
                if (Keyboard.IsPressed(Key.S)) moveDelta += _player.Transform.Global.Forward * speed;
                if (Keyboard.IsPressed(Key.A)) moveDelta += _player.Transform.Global.Left * speed;
                if (Keyboard.IsPressed(Key.D)) moveDelta += _player.Transform.Global.Right * speed;

                _player.Transform.LocalPosition += moveDelta * time.Delta * speed;
            }
        }

        public Scene SampleScene => _scene;

        public SceneObject Player => _player;

        public bool AcceptPlayerInput { get; set; } = true;
    }
}
