using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class MultiWindowGame : SampleGame
    {
        public override string Description => "A simple multi-window test.";

        List<IWindowSurface> _extraWindows;

        public MultiWindowGame(EngineSettings settings = null) : base("Multi-Window", settings)
        {
        }

        protected override void OnInitialize(Engine engine)
        {
            _extraWindows = new List<IWindowSurface>();
            Random rng = new Random();
            for (int i = 0; i < 5; i++)
            {
                IWindowSurface window = engine.Renderer.Resources.CreateFormSurface($"Window #{i}");
                engine.Renderer.OutputSurfaces.Add(window);
                window.PresentClearColor = new Color()
                {
                    R = (byte)rng.Next(0, 255),
                    G = (byte)rng.Next(0, 255),
                    B = (byte)rng.Next(0, 255),
                };

                _extraWindows.Add(window);
                window.Mode = WindowMode.Windowed;
                window.OnClose += Window_OnClose;
                window.Show();
            }

            base.OnInitialize(engine);
        }

        protected override void OnContentRequested(ContentRequest cr) { }

        protected override void OnContentLoaded(ContentManager content, ContentRequest cr) { }

        private void Window_OnClose(IWindowSurface surface)
        {
            Exit();
        }

        protected override void OnUpdate(Timing time)
        {
            Window.PresentClearColor = Color.Red;

            base.OnUpdate(time);
        }
    }
}
