using Molten.Graphics;
using Molten.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public abstract class SampleGame : MoltenGame
    {
        SpriteFont _testFont;
        bool _baseContentLoaded;
        ISceneDebugOverlay _mainOverlay;
        Camera2D _uiCamera;

        public SampleGame(string title, EngineSettings settings = null) : base(title, settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            Window.OnClose += Window_OnClose;

            SpriteScene = CreateScene("Sprite", SceneRenderFlags.Render2D);
            UIScene = CreateScene("UI", SceneRenderFlags.Render2D);

            // Use the same camera for both the sprite and UI scenes.
            UIScene.OutputCamera = SpriteScene.OutputCamera = new Camera2D();

            DebugOverlay = UIScene.DebugOverlay;
            UIScene.AddSprite(DebugOverlay);

            Window.PresentClearColor = new Color(20, 20, 20, 255);

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<SpriteFont>("BroshK.ttf;size=24");
            OnContentRequested(cr);
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();
        }

        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            _testFont = content.Get<SpriteFont>(cr[0]);
            DebugOverlay.Font = _testFont;

            OnContentLoaded(content, cr);
            _baseContentLoaded = true;
        }

        private void Window_OnClose(IWindowSurface surface)
        {
            Exit();
        }

        protected abstract void OnContentRequested(ContentRequest cr);

        protected abstract void OnContentLoaded(ContentManager content, ContentRequest cr);

        protected override void OnUpdate(Timing time)
        {
            // Don't update until the base content is loaded.
            if (!_baseContentLoaded)
                return;

            // Cycle through debug overlay pages.
            if(Keyboard.IsTapped(Key.F1) && _testFont != null)
                DebugOverlay.NextPage();

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

        public abstract string Description { get; }

        public SpriteFont SampleFont => _testFont;

        /// <summary>Gets a random number generator. Used for various samples.</summary>
        public Random Rng { get; private set; } = new Random();

        /// <summary>
        /// Gets the sample's UI scene.
        /// </summary>
        public Scene UIScene { get; private set; }

        /// <summary>
        /// Gets the sample's sprite scene. This is rendered before <see cref="UIScene"/>.
        /// </summary>
        public Scene SpriteScene { get; private set; }

        /// <summary>
        /// Gets or sets the sample's main debug overlay.
        /// </summary>
        public ISceneDebugOverlay DebugOverlay
        {
            get => _mainOverlay;
            set
            {
                if(_mainOverlay != value)
                {
                    if (_mainOverlay != null)
                        UIScene.RemoveSprite(_mainOverlay);

                    if (value != null)
                        UIScene.AddSprite(value);

                    _mainOverlay = value;
                }
            }
        }
    }
}
