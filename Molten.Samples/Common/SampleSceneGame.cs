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
            DebugOverlay = _scene.DebugOverlay;
            _scene.SendToBack();
            SpawnPlayer();
        }

        protected override void OnContentLoaded(ContentRequest cr)
        {
            string text = "[W][A][S][D] to move -- [ESC] Close -- [LMB] and [MOUSE] to rotate";
            _txtInstructionSize = SampleFont.MeasureString(text);
            _txtInstructions = new SpriteText()
            {
                Text = text,
                Font = SampleFont,
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
                Y = Window.Height - _txtInstructionSize.Y - 20,
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
            cam.MaximumDrawDistance = 300;
            cam.OutputSurface = Window;
            _scene.AddObject(_player);
            _scene.OutputCamera = cam;
        }

        protected SceneObject SpawnTestCube(IMesh mesh)
        {
            return SpawnTestCube(mesh, Vector3F.Zero);
        }

        protected SceneObject SpawnTestCube(IMesh mesh, Vector3F pos)
        {
            SceneObject obj = CreateObject(pos, _scene);
            MeshComponent meshCom = obj.AddComponent<MeshComponent>();
            meshCom.Mesh = mesh;
            return obj;
        }

        protected void SpawnParentChild(IMesh mesh, Vector3F origin, out SceneObject parent, out SceneObject child)
        {
            parent = SpawnTestCube(mesh);
            child = SpawnTestCube(mesh);

            child.Transform.LocalScale = new Vector3F(0.5f);
            child.Transform.LocalPosition = new Vector3F(0, 0, 2);
            parent.Transform.LocalPosition = origin;
            parent.Children.Add(child);
        }

        protected void RotateParentChild(SceneObject parent, SceneObject child, Timing time, float speed = 0.5f, float childSpeed = 1.0f)
        {
            var rotateTime = (float)time.TotalTime.TotalSeconds;

            parent.Transform.LocalRotationY += speed;
            if (parent.Transform.LocalRotationY >= 360)
                parent.Transform.LocalRotationY -= 360;

            child.Transform.LocalRotationX += childSpeed;
            if (child.Transform.LocalRotationX >= 360)
                child.Transform.LocalRotationX -= 360;
        }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            if (Keyboard.IsTapped(Key.Escape))
                Exit();

            // Keyboard input - Again messy code for now
            float rotSpeed = 0.25f;
            float moveSpeed = 0.5f;

            Vector2F axisDelta = Vector2F.Zero;

            if (AcceptPlayerInput && Mouse.IsPressed(MouseButton.Left))
            {
                axisDelta = new Vector2F()
                {
                    X = Mouse.Moved.Y * rotSpeed,
                    Y = Mouse.Moved.X * rotSpeed,
                };
                // Mouse input - Messy for now - We're just testing input
                _player.Transform.LocalRotationX += axisDelta.X;
                _player.Transform.LocalRotationY += axisDelta.Y;
            }

            // Gamepad movement


            // Handle forward, backward, left and right movement. 
            // For now, we'll just add the keyboard and gamepad values together. In a real game, this isn't a good idea!
            Vector3F moveDelta = UpdateKeyboardMovement(time, moveSpeed);
            moveDelta += UpdateGamepadMovement(time, moveSpeed);

            _player.Transform.LocalPosition += moveDelta * time.Delta * moveSpeed;
        }

        private Vector3F UpdateKeyboardMovement(Timing time, float speed)
        {
            Vector3F moveDelta = Vector3F.Zero;
            if (Keyboard.IsPressed(Key.W)) moveDelta += _player.Transform.Global.Backward * speed;
            if (Keyboard.IsPressed(Key.S)) moveDelta += _player.Transform.Global.Forward * speed;
            if (Keyboard.IsPressed(Key.A)) moveDelta += _player.Transform.Global.Left * speed;
            if (Keyboard.IsPressed(Key.D)) moveDelta += _player.Transform.Global.Right * speed;

            return moveDelta;
        }

        private Vector3F UpdateGamepadMovement(Timing time, float speed)
        {
            Vector3F moveDelta = Vector3F.Zero;
            if (Gamepad.IsPressed(GamepadButtonFlags.DPadUp)) moveDelta += _player.Transform.Global.Backward * speed;
            if (Gamepad.IsPressed(GamepadButtonFlags.DPadDown)) moveDelta += _player.Transform.Global.Forward * speed;
            if (Gamepad.IsPressed(GamepadButtonFlags.DPadLeft)) moveDelta += _player.Transform.Global.Left * speed;
            if (Gamepad.IsPressed(GamepadButtonFlags.DPadRight)) moveDelta += _player.Transform.Global.Right * speed;


            return moveDelta;
        }

        public Scene SampleScene => _scene;

        public SceneObject Player => _player;

        public bool AcceptPlayerInput { get; set; } = true;
    }
}
