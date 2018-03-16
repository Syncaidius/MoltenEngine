using Molten.Graphics;
using Molten.IO;
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
        int _nextRenderPage;

        public SampleGame(string title, EngineSettings settings = null) : base(title, settings)
        {

        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            Window.OnClose += Window_OnClose;

            _formDepthSurface = Engine.Renderer.Resources.CreateDepthSurface(Window.Width, Window.Height);
            Window.OnPostResize += Window_OnPostResize;
            Window.PresentClearColor = new Color(20, 20, 20, 255);

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<SpriteFont>("broshk.ttf;size=14");
            OnContentRequested(cr);
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();
        }

        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            _testFont = content.Get<SpriteFont>(cr.RequestedFiles[0]);
            Engine.Renderer.SetDebugOverlayPage(_testFont, true, 0);
            OnContentLoaded(content, cr);
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
            if(Keyboard.IsTapped(Key.F1) && _testFont != null)
                _nextRenderPage = Engine.Renderer.SetDebugOverlayPage(_testFont, true, _nextRenderPage);
        }

        public abstract string Description { get; }

        /// <summary>Gets the <see cref="IDepthSurface"/> which is meant to be used when rendering to the game's main window.
        /// Whenever the main game window is resized, this depth surface is too.</summary>
        public IDepthSurface WindowDepthSurface => _formDepthSurface;

        public SpriteFont TestFont => _testFont;

        /// <summary>Gets a random number generator. Used for various samples.</summary>
        public Random Rng { get; private set; } = new Random();
    }
}
