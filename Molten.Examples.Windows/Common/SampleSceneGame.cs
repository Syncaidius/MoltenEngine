using Molten.Graphics;
using Molten.Input;

namespace Molten.Samples
{
    public abstract class SampleSceneGame : SampleGame
    {
        SceneObject _player;

        public SampleSceneGame(string title) : base(title) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            _player = CreateObject();
            SceneCamera = _player.Components.Add<CameraComponent>();
            SceneCamera.LayerMask = SceneLayerMask.Layer1 | SceneLayerMask.Layer2;
            SceneCamera.OutputSurface = Window;
            SceneCamera.MaxDrawDistance = 300;
            //SceneCamera.MultiSampleLevel = AntiAliasLevel.X8;
            MainScene.AddObject(_player);
        }

        protected SceneObject SpawnTestCube(IMesh mesh)
        {
            return SpawnTestCube(mesh, Vector3F.Zero);
        }

        protected SceneObject SpawnTestCube(IMesh mesh, Vector3F pos)
        {
            SceneObject obj = CreateObject(pos, MainScene);
            MeshComponent meshCom = obj.Components.Add<MeshComponent>();
            meshCom.RenderedObject = mesh;
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

            if (Keyboard.IsTapped(KeyCode.Escape))
                Exit();

            // Keyboard input - Again messy code for now
            float rotSpeed = 0.25f;
            float moveSpeed = 0.5f;

            Vector2F axisDelta = Vector2F.Zero;

            if (AcceptPlayerInput && Mouse.IsDown(MouseButton.Left))
                axisDelta = new Vector2F(Mouse.Delta.Y, Mouse.Delta.X) * rotSpeed;

            // Gamepad movement
            axisDelta += new Vector2F(Gamepad.RightStick.Y, Gamepad.RightStick.X) * rotSpeed * 2f;

            _player.Transform.LocalRotationX += axisDelta.X;
            _player.Transform.LocalRotationY += axisDelta.Y;

            // Handle forward, backward, left and right movement. 
            // For now, we'll just add the keyboard and gamepad values together. In a real game, this isn't a good idea!
            Vector3F moveDelta = UpdateKeyboardMovement(time, moveSpeed);
            moveDelta += UpdateGamepadMovement(time, moveSpeed);

            _player.Transform.LocalPosition += moveDelta * time.Delta * moveSpeed;
        }

        private Vector3F UpdateKeyboardMovement(Timing time, float speed)
        {
            Vector3F moveDelta = Vector3F.Zero;
            if (Keyboard.IsDown(KeyCode.W)) moveDelta += _player.Transform.Global.Backward * speed;
            if (Keyboard.IsDown(KeyCode.S)) moveDelta += _player.Transform.Global.Forward * speed;
            if (Keyboard.IsDown(KeyCode.A)) moveDelta += _player.Transform.Global.Left * speed;
            if (Keyboard.IsDown(KeyCode.D)) moveDelta += _player.Transform.Global.Right * speed;


            return moveDelta;
        }

        private Vector3F UpdateGamepadMovement(Timing time, float speed)
        {
            Vector3F moveDelta = Vector3F.Zero;
            if (Gamepad.IsDown(GamepadButton.DPadUp)) moveDelta += _player.Transform.Global.Backward * speed;
            if (Gamepad.IsDown(GamepadButton.DPadDown)) moveDelta += _player.Transform.Global.Forward * speed;
            if (Gamepad.IsDown(GamepadButton.DPadLeft)) moveDelta += _player.Transform.Global.Left * speed;
            if (Gamepad.IsDown(GamepadButton.DPadRight)) moveDelta += _player.Transform.Global.Right * speed;

            moveDelta += new Vector3F(Gamepad.LeftStick.X, 0, Gamepad.LeftStick.Y) * speed * 2;
            return moveDelta;
        }

        protected override void OnHudDraw(SpriteBatcher sb)
        {
            base.OnHudDraw(sb);

            if (SampleFont == null)
                return;

            string text = "[W][A][S][D] to move -- [ESC] close -- [LMB] and [MOUSE] to rotate";
            Vector2F tSize = SampleFont.MeasureString(text);
            Vector2F pos = new Vector2F()
            {
                X = Window.Width / 2 + (-tSize.X / 2),
                Y = Window.Height - tSize.Y - 20,
            };

            sb.DrawString(SampleFont, text, pos, Color.White);

            // Gamepad instructions
            text = "OR";
            tSize = SampleFont.MeasureString(text);
            pos.X = Window.Width / 2 + (-tSize.X / 2);
            pos.Y -= tSize.Y + 5;
            sb.DrawString(SampleFont, text, pos, Color.White);

            if (Gamepad.IsConnected)
            {
                text = "Gamepad [LEFT STICK] or [D-PAD] to move -- [RIGHT STICK] to aim";
                tSize = SampleFont.MeasureString(text);
                pos.X = Window.Width / 2 + (-tSize.X / 2);
                pos.Y -= tSize.Y + 5;
                sb.DrawString(SampleFont, text, pos, Color.White);

                // Stats
                pos.X = 5;
                pos.Y = 300; sb.DrawString(SampleFont, $"Left stick: {Gamepad.LeftStick.X},{Gamepad.LeftStick.Y}", pos, Color.White);
                pos.Y += 20; sb.DrawString(SampleFont, $"Right stick: {Gamepad.RightStick.X},{Gamepad.RightStick.Y}", pos, Color.White);
                pos.Y += 20; sb.DrawString(SampleFont, $"Left Trigger: {Gamepad.LeftTrigger.Value}", pos, Color.White);
                pos.Y += 20; sb.DrawString(SampleFont, $"Right Trigger: {Gamepad.RightTrigger.Value}", pos, Color.White);
            }
            else
            {
                text = "Connect a gamepad / controller";
                tSize = SampleFont.MeasureString(text);
                pos.X = Window.Width / 2 + (-tSize.X / 2);
                pos.Y -= tSize.Y + 5;
                sb.DrawString(SampleFont, text, pos, Color.White);
            }
        }

        public SceneObject Player => _player;

        public bool AcceptPlayerInput { get; set; } = true;

        public CameraComponent SceneCamera { get; set; }
    }
}
