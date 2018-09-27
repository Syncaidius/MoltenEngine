using Molten.Graphics;
using Molten.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Molten.Samples
{
    public abstract class SampleGame : Foundation
    {
        SpriteFont _sampleFont;
        bool _baseContentLoaded;
        RenderDebugOverlay _mainOverlay;
        ControlSampleForm _form;
        SpriteBatchContainer _sbContainer;
        SceneLayer _spriteLayer;
        SceneLayer _uiLayer;

        public SampleGame(string title, EngineSettings settings = null) : base(title, settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            Window.OnHandleChanged += Window_OnHandleChanged;

            MainScene = CreateScene("Main");
            _spriteLayer = MainScene.AddLayer("sprite", true);
            _uiLayer = MainScene.AddLayer("ui", true);
            _uiLayer.BringToFront();

            // Use the same camera for both the sprite and UI scenes.
            SceneObject obj = CreateObject(MainScene);            
            CameraComponent cam2D = obj.AddComponent<CameraComponent>();
            cam2D.Mode = RenderCameraMode.Orthographic;
            cam2D.OrderDepth = 1;
            cam2D.MaxDrawDistance = 1.0f;
            cam2D.OutputSurface = Window;
            cam2D.LayerMask = BitwiseHelper.Set(cam2D.LayerMask, 0);
            _uiLayer.AddObject(obj);

            DebugOverlay = MainScene.DebugOverlay;
            _uiLayer.AddObject(DebugOverlay);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<SpriteFont>("BroshK.ttf;size=24");
            OnContentRequested(cr);
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();
        }

        private void Window_OnHandleChanged(IWindowSurface surface)
        {
            if (Settings.UseGuiControl && _form == null)
            {
                // Create form and find placeholder panel.
                _form = new ControlSampleForm();
                Control[] panelsToReplace = _form.Controls.Find("panelToReplace", true);

                if (panelsToReplace.Length > 0)
                {
                    // Replace the placeholder panel with our game's control surface.
                    Control gameControl = Control.FromHandle(Window.Handle);
                    gameControl.Size = panelsToReplace[0].Size;
                    gameControl.Location = panelsToReplace[0].Location;
                    gameControl.Anchor = panelsToReplace[0].Anchor;

                    _form.SliderRed.ValueChanged += SliderRed_ValueChanged;
                    _form.SliderGreen.ValueChanged += SliderGreen_ValueChanged;
                    _form.SliderBlue.ValueChanged += SliderBlue_ValueChanged;

                    _form.Controls.Add(gameControl);
                    _form.Controls.Remove(panelsToReplace[0]);
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
            //Color color = Window.ClearColor;
            //color[channelIndex] = val;
            //Window.ClearColor = color;
        }

        private void Cr_OnCompleted(ContentRequest cr)
        {
            _sampleFont = cr.Get<SpriteFont>(0);
            DebugOverlay.Overlay.Font = _sampleFont;


            _sbContainer = new SpriteBatchContainer(OnHudDraw);
            _uiLayer.AddObject(_sbContainer);

            OnContentLoaded(cr);
            _baseContentLoaded = true;
        }

        protected virtual void OnContentRequested(ContentRequest cr) { }

        protected virtual void OnContentLoaded(ContentRequest cr) { }

        protected override void OnUpdate(Timing time)
        {
            // Don't update until the base content is loaded.
            if (!_baseContentLoaded)
                return;

            // Cycle through debug overlay pages.
            if(Keyboard.IsTapped(Key.F1) && _sampleFont != null)
                DebugOverlay.Overlay.NextPage();

            // Cycle through window modes.
            if (Keyboard.IsTapped(Key.F2))
            {
                switch (Window.Mode)
                {
                    case WindowMode.Borderless: Window.Mode = WindowMode.Windowed; break;
                    case WindowMode.Windowed: Window.Mode = WindowMode.Borderless; break;
                }
            }
        }

        protected virtual void OnHudDraw(SpriteBatch sb) { }

        public abstract string Description { get; }

        public SpriteFont SampleFont => _sampleFont;

        /// <summary>Gets a random number generator. Used for various samples.</summary>
        public Random Rng { get; private set; } = new Random();

        /// <summary>
        /// Gets the sample's UI scene layer.
        /// </summary>
        public SceneLayer UI => _uiLayer;

        /// <summary>
        /// Gets the sample's sprite scene layer.
        /// </summary>
        public SceneLayer SpriteLayer => _spriteLayer;

        /// <summary>
        /// Gets the sample's sprite scene. This is rendered before <see cref="UIScene"/>.
        /// </summary>
        public Scene MainScene { get; private set; }

        /// <summary>
        /// Gets or sets the sample's main debug overlay.
        /// </summary>
        public RenderDebugOverlay DebugOverlay
        {
            get => _mainOverlay;
            set
            {
                if(_mainOverlay != value)
                {
                    if (_mainOverlay != null)
                        _uiLayer.RemoveObject(_mainOverlay);

                    if (value != null)
                        _uiLayer.AddObject(value);

                    _mainOverlay = value;
                }
            }
        }
    }
}
