using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public abstract class TestGame : MoltenGame
    {
        IDepthSurface _formDepthSurface;

        public TestGame(string title, EngineSettings settings = null) : base($"{title} Test", settings)
        {

        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            Window.OnClose += Window_OnClose;

            engine.Renderer.SetDebugOverlayPage(true, 0);
            _formDepthSurface = Engine.Renderer.Resources.CreateDepthSurface(Window.Width, Window.Height);
            Window.OnPostResize += Window_OnPostResize;
        }

        private void Window_OnPostResize(ITexture texture)
        {
            _formDepthSurface.Resize(Window.Width, Window.Height);
        }

        private void Window_OnClose(IWindowSurface surface)
        {
            Exit();
        }

        public abstract string Description { get; }

        /// <summary>Gets the <see cref="IDepthSurface"/> which is meant to be used when rendering to the game's main window.
        /// Whenever the main game window is resized, this depth surface is too.</summary>
        public IDepthSurface WindowDepthSurface => _formDepthSurface;
    }
}
