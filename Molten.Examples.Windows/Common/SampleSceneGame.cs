using Molten.Graphics;
using Molten.Input;

namespace Molten.Samples
{
    /// <summary>
    /// Provides a basic test scene with one rotating cube parented to a larger rotating cube.
    /// </summary>
    public abstract class SampleSceneGame : SampleGame
    {
        SceneObject _player;
        SampleCameraController _camController;
        SceneObject _parent;
        SceneObject _child;
        protected IMesh TestMesh { get; private set; }

        public SampleSceneGame(string title) : base(title) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            SpawnPlayer();

            TestMesh = GetTestCubeMesh();
            SpawnParentChild(TestMesh, Vector3F.Zero, out _parent, out _child);
        }

        protected virtual IMesh GetTestCubeMesh()
        {
            IMesh<VertexTexture> cube = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);
            cube.SetVertices(SampleVertexData.TexturedCube);
            return cube;
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

        private SceneObject SpawnTestCube(IMesh mesh, Vector3F pos)
        {
            SceneObject obj = CreateObject(pos, MainScene);
            MeshComponent meshCom = obj.Components.Add<MeshComponent>();
            meshCom.RenderedObject = mesh;
            return obj;
        }

        protected void SpawnParentChild(IMesh mesh, Vector3F origin, out SceneObject parent, out SceneObject child)
        {
            parent = SpawnTestCube(mesh, Vector3F.Zero);
            child = SpawnTestCube(mesh, Vector3F.Zero);

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

            RotateParentChild(_parent, _child, time);
            OnGamepadInput(time);
        }

        /// <summary>
        /// Called when the <see cref="SampleSceneGame"/> should update and handle gamepad input. <see cref="SampleSceneGame"/> provides default handling.
        /// </summary> 
        /// <param name="time"></param>
        protected virtual void OnGamepadInput(Timing time)
        {
            // Apply left and right vibration equal to left and right trigger values 
            Gamepad.VibrationLeft.Value = Gamepad.LeftTrigger.Value;
            Gamepad.VibrationRight.Value = Gamepad.RightTrigger.Value;
        }

        protected override void OnHudDraw(SpriteBatcher sb)
        {
            base.OnHudDraw(sb);

            if (SampleFont == null)
                return;

            string text = "[W][A][S][D] to move -- [ESC] close -- [LMB] and [MOUSE] to rotate";
            Vector2F tSize = SampleFont.MeasureString(text, 16);
            Vector2F pos = new Vector2F()
            {
                X = Window.Width / 2 + (-tSize.X / 2),
                Y = Window.Height - tSize.Y - 20,
            };

            sb.DrawString(SampleFont, 16, text, pos, Color.White);

            // Gamepad instructions
            text = "OR";
            tSize = SampleFont.MeasureString(text, 16);
            pos.X = Window.Width / 2 + (-tSize.X / 2);
            pos.Y -= tSize.Y + 5;
            sb.DrawString(SampleFont, 16, text, pos, Color.White);

            if (Gamepad.IsConnected)
            {
                text = "Gamepad [LEFT STICK] or [D-PAD] to move -- [RIGHT STICK] to aim";
                tSize = SampleFont.MeasureString(text, 16);
                pos.X = Window.Width / 2 + (-tSize.X / 2);
                pos.Y -= tSize.Y + 5;
                sb.DrawString(SampleFont, 16, text, pos, Color.White);

                // Stats
                pos.X = 5;
                pos.Y = 300; sb.DrawString(SampleFont, 16, $"Left stick: {Gamepad.LeftStick.X},{Gamepad.LeftStick.Y}", pos, Color.White);
                pos.Y += 20; sb.DrawString(SampleFont, 16, $"Right stick: {Gamepad.RightStick.X},{Gamepad.RightStick.Y}", pos, Color.White);
                pos.Y += 20; sb.DrawString(SampleFont, 16, $"Left Trigger: {Gamepad.LeftTrigger.Value}", pos, Color.White);
                pos.Y += 20; sb.DrawString(SampleFont, 16, $"Right Trigger: {Gamepad.RightTrigger.Value}", pos, Color.White);
            }
            else
            {
                text = "Connect a gamepad / controller";
                tSize = SampleFont.MeasureString(text, 16);
                pos.X = Window.Width / 2 + (-tSize.X / 2);
                pos.Y -= tSize.Y + 5;
                sb.DrawString(SampleFont, 16, text, pos, Color.White);
            }
        }

        public SceneObject Player => _player;

        public CameraComponent SceneCamera { get; set; }

        public SampleCameraController CameraController => _camController;
    }
}
