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
        ISpriteFont _testFont;
        int _nextRenderPage;

        public SampleGame(string title, EngineSettings settings = null) : base(title, settings)
        {

        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            Window.OnClose += Window_OnClose;

            engine.Renderer.SetDebugOverlayPage(true, 0);
            _formDepthSurface = Engine.Renderer.Resources.CreateDepthSurface(Window.Width, Window.Height);
            Window.OnPostResize += Window_OnPostResize;

            _testFont = engine.Renderer.Resources.CreateFont("arial", 14);
        }

        private void Window_OnPostResize(ITexture texture)
        {
            _formDepthSurface.Resize(Window.Width, Window.Height);
        }

        private void Window_OnClose(IWindowSurface surface)
        {
            Exit();
        }

        protected override void OnUpdate(Timing time)
        {
            if(Keyboard.IsTapped(Key.F1))
                _nextRenderPage = Engine.Renderer.SetDebugOverlayPage(true, _nextRenderPage);
        }

        public abstract string Description { get; }

        /// <summary>Gets the <see cref="IDepthSurface"/> which is meant to be used when rendering to the game's main window.
        /// Whenever the main game window is resized, this depth surface is too.</summary>
        public IDepthSurface WindowDepthSurface => _formDepthSurface;

        public ISpriteFont TestFont => _testFont;
    }
}
