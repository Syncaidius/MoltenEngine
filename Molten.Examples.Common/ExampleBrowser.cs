using Molten.Audio;
using Molten.Collections;
using Molten.Graphics;
using Molten.Input;
using Molten.Net;
using Molten.UI;
using System.Diagnostics;

namespace Molten.Examples
{
    public class ExampleBrowser<R, I, A> : Foundation
        where R : RenderService, new()
        where I : InputService, new ()
        where A : AudioService, new()
    {
        bool _baseContentLoaded;
        Scene _scene;
        CameraComponent _cam2D;
        ContentLoadBatch _loader;

        UILabel _txtDebug;
        UILabel _txtMovement;
        UILabel _txtGamepad;
        UIListView _lstExamples;
        UIButton _btnCloseAll;
        UIButton _btnStart;
        UICheckBox _chkNativeWindow;

        ThreadedList<MoltenExample> _activeExamples;

        public ExampleBrowser(string title) : base(title)
        {
            _activeExamples = new ThreadedList<MoltenExample>();
        }

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

            _scene = new Scene("ExampleBrowser", Engine);
            _scene.BackgroundColor = new Color(0x333333);
            SpriteLayer = _scene.AddLayer("sprite", true);
            UILayer = _scene.AddLayer("ui", true);
            UILayer.BringToFront();
            UI = UILayer.AddObjectWithComponent<UIManagerComponent>();

            // Use the same camera for both the sprite and UI scenes.
            _cam2D = _scene.AddObjectWithComponent<CameraComponent>(UILayer);
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
                SampleFont = font;
                Engine.Renderer.Overlay.Font = SampleFont;
            });

            _loader.Deserialize<UITheme>("assets/test_theme.json", (theme, isReload) =>
            {
                UI.Root.Theme = theme;
            });

            _loader.OnCompleted += OnBaseContentLoaded;
            _loader.Dispatch();
        }

        private void OnBaseContentLoaded(ContentLoadBatch content)
        {
            SampleSpriteRenderComponent com = UILayer.AddObjectWithComponent<SampleSpriteRenderComponent>();
            com.RenderCallback = OnDrawSprites;
            com.DepthWriteOverride = GraphicsDepthWritePermission.Disabled;
            BuildUI(UI);
            DetectExamples();
            _baseContentLoaded = true;
        }

        private void DetectExamples()
        {
            List<Type> eTypes = ReflectionHelper.FindType<MoltenExample>();
            foreach(Type t in eTypes)
            {
                UIExampleListItem item = _lstExamples.Children.Add<UIExampleListItem>(new Rectangle(0, 0, 10, 25));
                item.ExampleType = t;
                item.Text = t.Name;
            }
        }

        private void BuildUI(UIManagerComponent ui)
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

            UIPanel cp = UI.Children.Add<UIPanel>(new Rectangle(0, 0, 300, 900));
            UILabel lblExamples = UI.Children.Add<UILabel>(new Rectangle(5, 5, 300, 20));
            lblExamples.Text = "Available examples:";

            _lstExamples = cp.Children.Add<UIListView>(new Rectangle(5, 30, 285, 600));
            _btnCloseAll = UI.Children.Add<UIButton>(new Rectangle(25, 650, 130, 25));
            _btnCloseAll.Text = "Close All";

            _btnStart = UI.Children.Add<UIButton>(new Rectangle(165, 650, 100, 25));
            _btnStart.Text = "Start";

            _chkNativeWindow = UI.Children.Add<UICheckBox>(new Rectangle(5, 690, 200, 25));
            _chkNativeWindow.Text = "Open in Native Window";

            ui.Children.Add(_txtDebug);
            ui.Children.Add(_txtMovement);
            ui.Children.Add(_txtGamepad);

            UpdateGamepadUI();
            Gamepad.OnConnectionStatusChanged += Gamepad_OnConnectionStatusChanged;

            UpdateUIlayout(ui);
        }

        private void Gamepad_OnConnectionStatusChanged(InputDevice device, bool isConnected)
        {
            UpdateGamepadUI();
        }

        private void Window_OnPostResize(ITexture texture)
        {
            if (_baseContentLoaded)
                UpdateUIlayout(UI);
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

        protected virtual void UpdateUIlayout(UIManagerComponent ui)
        {
            int xCenter = (int)(Window.Width / 2);
            _txtDebug.LocalBounds = new Rectangle(xCenter, 5, 0, 0);
            _txtGamepad.LocalBounds = new Rectangle(xCenter, (int)Window.Height - 20, 0, 0);
            _txtMovement.LocalBounds = new Rectangle(xCenter, _txtGamepad.LocalBounds.Y - 20, 0, 0);
        }

        /// <summary>
        /// Called when the <see cref="SampleBrowser"/> should update and handle gamepad input. <see cref="SampleBrowser"/> provides default handling.
        /// </summary> 
        /// <param name="time"></param>
        private void OnGamepadInput(Timing time)
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

            OnGamepadInput(time);
        }

        protected virtual void OnDrawSprites(SpriteBatcher sb)
        {
            if (SampleFont == null)
                return;

        }

        public SpriteFont SampleFont { get; private set; }

        /// <summary>Gets a random number generator. Used for various samples.</summary>
        public Random Rng { get; private set; } = new Random();

        /// <summary>
        /// Gets the sample's UI scene layer.
        /// </summary>
        public SceneLayer UILayer { get; private set; }

        /// <summary>
        /// Gets the sample's sprite scene layer.
        /// </summary>
        public SceneLayer SpriteLayer { get; private set; }

        public UIManagerComponent UI { get; private set; }

        protected IMesh TestMesh { get; private set; }
    }
}
