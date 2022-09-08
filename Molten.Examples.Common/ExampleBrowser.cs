using Molten.Audio;
using Molten.Graphics;
using Molten.Input;
using Molten.Net;
using Molten.UI;
using System.Diagnostics;

namespace Molten.Samples
{
    public class ExampleBrowser<R, I, A> : Foundation
        where R : RenderService, new()
        where I : InputService, new ()
        where A : AudioService, new()
    {
        SpriteFont _sampleFont;
        bool _baseContentLoaded;
        SceneLayer _spriteLayer;
        SceneLayer _uiLayer;
        CameraComponent _cam2D;

        ContentLoadBatch _loader;

        SceneObject _player;
        SampleCameraController _camController;
        SceneObject _parent;
        SceneObject _child;

        UILabel _txtDebug;
        UILabel _txtMovement;
        UILabel _txtGamepad;
        UIListView _lstExamples;

        public ExampleBrowser(string title) : base(title) { }

        protected override void OnStart(EngineSettings settings)
        {
            settings.AddService<R>();
            settings.AddService<I>();
            settings.AddService<A>();
        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            if (Window != null)
                Window.OnPostResize += Window_OnPostResize;

            MainScene = CreateScene("Main");
            _spriteLayer = MainScene.AddLayer("sprite", true);
            _uiLayer = MainScene.AddLayer("ui", true);
            _uiLayer.BringToFront();
            UI = _uiLayer.AddObjectWithComponent<UIManagerComponent>();

            // Use the same camera for both the sprite and UI scenes.
            _cam2D = MainScene.AddObjectWithComponent<CameraComponent>(_uiLayer);
            _cam2D.Mode = RenderCameraMode.Orthographic;
            _cam2D.OrderDepth = 1;
            _cam2D.MaxDrawDistance = 1.0f;
            _cam2D.OutputSurface = Window;
            _cam2D.LayerMask = SceneLayerMask.Layer0;

            UI.Root.IsScrollingEnabled = false;
            UI.Camera = _cam2D;

            Settings.Input.PointerSensitivity.Value = 0.75f;
            Settings.Input.PointerSensitivity.Apply();

            if (engine.Input != null && engine.Input.State == EngineServiceState.Ready)
                Engine.Input.Camera = _cam2D;

            _loader = Engine.Content.GetLoadBatch();
            _loader.LoadFont("assets/BroshK.ttf", (font, isReload) =>
            {
                _sampleFont = font;
                Engine.Renderer.Overlay.Font = _sampleFont;
            });

            _loader.OnCompleted += OnBaseContentLoaded;
            _loader.Dispatch();

            SpawnPlayer();
            TestMesh = GetTestCubeMesh();
            SpawnParentChild(TestMesh, Vector3F.Zero, out _parent, out _child);
        }

        private void Gamepad_OnConnectionStatusChanged(Input.InputDevice device, bool isConnected)
        {
            UpdateGamepadUI();
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


        protected virtual IMesh GetTestCubeMesh()
        {
            IMesh<VertexTexture> cube = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);
            cube.SetVertices(SampleVertexData.TexturedCube);
            return cube;
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

        private void Window_OnPostResize(ITexture texture)
        {
            if (_baseContentLoaded)
                UpdateUIlayout(UI);
        }

        private void ChangePresentColor(int channelIndex, byte val)
        {
            Color color = MainScene.BackgroundColor;
            color[channelIndex] = val;
            MainScene.BackgroundColor = color;
        }

        private void UpdateGamepadUI()
        {
            if (Gamepad.IsConnected)
            {
                _txtGamepad.Text = "Gamepad [LEFT STICK] or [D-PAD] to move -- [RIGHT STICK] to aim";
            }
            else
            {
                _txtGamepad.Text = "Connect a gamepad / controller";
            }
        }

        protected void BuildUI(UIManagerComponent ui)
        {
            _txtDebug = new UILabel()
            {
                Text = "[F1] debug overlay",
                HorizontalAlign = UIHorizonalAlignment.Center,
            };

            _txtMovement = new UILabel()
            {
                Text = "[W][A][S][D] to move -- [ESC] close -- [LMB] and [MOUSE] to rotate",
                HorizontalAlign = UIHorizonalAlignment.Center,
            };

            _txtGamepad = new UILabel()
            {
                HorizontalAlign = UIHorizonalAlignment.Center,
            };

            _lstExamples = UI.Children.Add<UIListView>(new Rectangle(0, 0, 300, 600));

            ui.Children.Add(_txtDebug);
            ui.Children.Add(_txtMovement);
            ui.Children.Add(_txtGamepad);

            UpdateGamepadUI();
            Gamepad.OnConnectionStatusChanged += Gamepad_OnConnectionStatusChanged;

            UpdateUIlayout(ui);
        }

        protected virtual void UpdateUIlayout(UIManagerComponent ui)
        {
            int xCenter = (int)(Window.Width / 2);
            _txtDebug.LocalBounds = new Rectangle(xCenter, 5, 0, 0);
            _txtGamepad.LocalBounds = new Rectangle(xCenter, (int)Window.Height - 20, 0, 0);
            _txtMovement.LocalBounds = new Rectangle(xCenter, _txtGamepad.LocalBounds.Y - 20, 0, 0);
        }

        private void OnBaseContentLoaded(ContentLoadBatch content)
        {
            SampleSpriteRenderComponent com = _uiLayer.AddObjectWithComponent<SampleSpriteRenderComponent>();
            com.RenderCallback = OnDrawSprites;
            com.DepthWriteOverride = GraphicsDepthWritePermission.Disabled;
            BuildUI(UI);
            _baseContentLoaded = true;
        }

        /// <summary>
        /// Called when the <see cref="SampleBrowser"/> should update and handle gamepad input. <see cref="SampleBrowser"/> provides default handling.
        /// </summary> 
        /// <param name="time"></param>
        protected virtual void OnGamepadInput(Timing time)
        {
            // Apply left and right vibration equal to left and right trigger values 
            Gamepad.VibrationLeft.Value = Gamepad.LeftTrigger.Value;
            Gamepad.VibrationRight.Value = Gamepad.RightTrigger.Value;
        }

        protected override void OnUpdate(Timing time)
        {
            // Don't update until the base content is loaded.
            if (!_baseContentLoaded)
                return;

            // Cycle through window modes.
            if (Engine.Renderer == null || Engine.Renderer.State != EngineServiceState.Running)
                return;

            if (Engine.Input != null && Engine.Input.State == EngineServiceState.Running)
            {
                if (Keyboard.IsTapped(KeyCode.F2))
                {
                    switch (Window.Mode)
                    {
                        case WindowMode.Borderless: Window.Mode = WindowMode.Windowed; break;
                        case WindowMode.Windowed: Window.Mode = WindowMode.Borderless; break;
                    }
                }

                // Toggle overlay.
                if (Keyboard.IsTapped(KeyCode.F1))
                {
                    if (_cam2D.HasFlags(RenderCameraFlags.ShowOverlay))
                    {
                        int cur = Engine.Renderer.Overlay.Current;

                        // Remove overlay flag if we've hit the last overlay.
                        if (!Engine.Renderer.Overlay.Next())
                        {
                            _cam2D.Flags &= ~RenderCameraFlags.ShowOverlay;
                            Engine.Renderer.Overlay.Current = 0;
                        }
                    }
                    else
                    {
                        Engine.Renderer.Overlay.Current = 0;
                        _cam2D.Flags |= RenderCameraFlags.ShowOverlay;
                    }
                    Debug.WriteLine($"Overlay toggled with F1 -- Frame ID: {time.FrameID}");
                }
            }

            if (Keyboard.IsTapped(KeyCode.Escape))
                Exit();

            RotateParentChild(_parent, _child, time);
            OnGamepadInput(time);
        }

        protected virtual void OnDrawSprites(SpriteBatcher sb)
        {
            if (SampleFont == null)
                return;

        }

        public SpriteFont SampleFont => _sampleFont;

        /// <summary>Gets a random number generator. Used for various samples.</summary>
        public Random Rng { get; private set; } = new Random();

        /// <summary>
        /// Gets the sample's UI scene layer.
        /// </summary>
        public SceneLayer UILayer => _uiLayer;

        /// <summary>
        /// Gets the sample's sprite scene layer.
        /// </summary>
        public SceneLayer SpriteLayer => _spriteLayer;

        public UIManagerComponent UI { get; private set; }

        /// <summary>
        /// Gets the sample's sprite scene. This is rendered before <see cref="UIScene"/>.
        /// </summary>
        public Scene MainScene { get; private set; }

        protected IMesh TestMesh { get; private set; }

        public SceneObject Player => _player;

        public CameraComponent SceneCamera { get; set; }

        public SampleCameraController CameraController => _camController;
    }
}
