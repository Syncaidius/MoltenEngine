using Molten.Graphics;
using Molten.Input;

namespace Molten.Samples
{
    public abstract class SampleSceneGame : SampleGame
    {
        SceneObject _player;
        SampleCameraController _camController;

        public SampleSceneGame(string title) : base(title) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            _player = CreateObject();
            _player.Transform.LocalPosition = new Vector3F(0, 0, -10);
            SceneCamera = _player.Components.Add<CameraComponent>();
            _camController = _player.Components.Add<SampleCameraController>();
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

        public CameraComponent SceneCamera { get; set; }

        public SampleCameraController CameraController => _camController;
    }
}
