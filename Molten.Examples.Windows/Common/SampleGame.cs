using Molten.Graphics;
using Molten.Input;
using Molten.UI;
using System.Diagnostics;
using System.Windows.Forms;

namespace Molten.Samples
{
    public abstract class SampleGame : Foundation
    {
        TextFont _sampleFont;
        bool _baseContentLoaded;
        ControlSampleForm _form;
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

        public SampleGame(string title) : base(title) { }

        protected override void OnStart(EngineSettings settings)
        {
            settings.AddService<RendererDX11>();
            settings.AddService<WinInputService>();
        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            if (Window != null)
            {
                Window.OnHandleChanged += Window_OnHandleChanged;
                Window.OnPostResize += Window_OnPostResize;
            }

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

            UI.Camera = _cam2D;

            Settings.Input.PointerSensitivity.Value = 0.75f;
            Settings.Input.PointerSensitivity.Apply();

            if (engine.Input != null && engine.Input.State == EngineServiceState.Ready)
                Engine.Input.Camera = _cam2D;

            _loader = Engine.Content.GetLoadBatch();
            _loader.Load<TextFont>("assets/BroshK.ttf", (font, isReload) =>
            {
                _sampleFont = font;
                Engine.Renderer.Overlay.Font = _sampleFont;
            });
            OnLoadContent(_loader);
            _loader.OnCompleted += ContentBatch_OnCompleted;
            _loader.Dispatch();

            SpawnPlayer();
            TestMesh = GetTestCubeMesh();
            SpawnParentChild(TestMesh, Vector3F.Zero, out _parent, out _child);
        }

        private void Gamepad_OnConnectionStatusChanged(InputDevice device, bool isConnected)
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

        private void Window_OnHandleChanged(INativeSurface surface)
        {
            if (Settings.UseGuiControl && _form == null)
            {
                // Create form and find placeholder panel.
                _form = new ControlSampleForm();
                Control[] placeholderPanels = _form.Controls.Find("surfacePlaceholder", true);
                if (placeholderPanels.Length > 0)
                {
                    surface.ParentHandle = placeholderPanels[0].Handle;

                    // TODO remove winforms sliders once engine has own GUI system.
                    _form.SliderRed.ValueChanged += SliderRed_ValueChanged;
                    _form.SliderGreen.ValueChanged += SliderGreen_ValueChanged;
                    _form.SliderBlue.ValueChanged += SliderBlue_ValueChanged;
                    _form.Show();
                }
            }
        }

        private void SliderRed_ValueChanged(object sender, EventArgs e)
        {
            ChangePresentColor(0, (byte)((sender as TrackBar).Value));
        }

        private void SliderGreen_ValueChanged(object sender, EventArgs e)
        {
            ChangePresentColor(1, (byte)((sender as TrackBar).Value));
        }

        private void SliderBlue_ValueChanged(object sender, EventArgs e)
        {
            ChangePresentColor(2, (byte)((sender as TrackBar).Value));
        }

        private void ChangePresentColor(int channelIndex, byte val)
        {
            Color color = MainScene.BackgroundColor;
            color[channelIndex] = val;
            MainScene.BackgroundColor = color;
        }

        protected abstract void OnLoadContent(ContentLoadBatch loader);

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

            ui.Children.Add(_txtDebug);
            ui.Children.Add(_txtMovement);
            ui.Children.Add(_txtGamepad);

            UpdateGamepadUI();
            Gamepad.OnConnectionStatusChanged += Gamepad_OnConnectionStatusChanged;

            OnBuildUI(ui);
            UpdateUIlayout(ui);
        }

        protected virtual void OnBuildUI(UIManagerComponent ui) { }

        protected virtual void UpdateUIlayout(UIManagerComponent ui)
        {
            int xCenter = (int)(Window.Width / 2);
            _txtDebug.LocalBounds = new Rectangle(xCenter, 5, 0, 0);
            _txtGamepad.LocalBounds = new Rectangle(xCenter, (int)Window.Height - 20, 0, 0);
            _txtMovement.LocalBounds = new Rectangle(xCenter, _txtGamepad.LocalBounds.Y - 20, 0, 0);
        }

        private void ContentBatch_OnCompleted(ContentLoadBatch content)
        {
            SampleSpriteRenderComponent com = _uiLayer.AddObjectWithComponent<SampleSpriteRenderComponent>();
            com.RenderCallback = OnDrawSprites;
            com.DepthWriteOverride = GraphicsDepthWritePermission.Disabled;
            BuildUI(UI);
            _baseContentLoaded = true;
        }

        /// <summary>
        /// Called when the <see cref="SampleGame"/> should update and handle gamepad input. <see cref="SampleGame"/> provides default handling.
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

            /*if (Gamepad.IsConnected)
            {
                text = "Gamepad [LEFT STICK] or [D-PAD] to move -- [RIGHT STICK] to aim";
                tSize = SampleFont.MeasureString(text);
                pos.X = (Window.Width / 2) - (tSize.X / 2);
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
                pos.X = (Window.Width / 2) - (tSize.X / 2);
                pos.Y -= tSize.Y + 5;
                sb.DrawString(SampleFont, text, pos, Color.White);
            }*/
        }

        public abstract string Description { get; }

        public TextFont SampleFont => _sampleFont;

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
