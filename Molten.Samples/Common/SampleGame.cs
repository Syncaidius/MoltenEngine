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
        IDepthSurface _formDepthSurface;
        SpriteFont _testFont;
        bool _baseContentLoaded;
        ISceneDebugOverlay _mainOverlay;

        public SampleGame(string title, EngineSettings settings = null) : base(title, settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            Window.OnClose += Window_OnClose;

            SpriteScene = CreateScene("Sprite", SceneRenderFlags.Render2D);
            UIScene = CreateScene("UI", SceneRenderFlags.Render2D);
            DebugOverlay = UIScene.DebugOverlay;
            UIScene.AddSprite(DebugOverlay);

            _formDepthSurface = Engine.Renderer.Resources.CreateDepthSurface(Window.Width, Window.Height);
            Window.OnPostResize += Window_OnPostResize;
            Window.PresentClearColor = new Color(20, 20, 20, 255);

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<SpriteFont>("BroshK.ttf;size=24");
            OnContentRequested(cr);
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();
        }

        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            _testFont = content.Get<SpriteFont>(cr.RequestedFiles[0]);
            DebugOverlay.Font = _testFont;

            OnContentLoaded(content, cr);
            _baseContentLoaded = true;
        }

        private void Window_OnPostResize(ITexture texture)
        {
            _formDepthSurface.Resize(Window.Width, Window.Height);
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

            if(Keyboard.IsTapped(Key.F1) && _testFont != null)
                DebugOverlay.NextPage();
        }

        public abstract string Description { get; }

        /// <summary>Gets the <see cref="IDepthSurface"/> which is meant to be used when rendering to the game's main window.
        /// Whenever the main game window is resized, this depth surface is too.</summary>
        public IDepthSurface WindowDepthSurface => _formDepthSurface;

        public SpriteFont TestFont => _testFont;

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
