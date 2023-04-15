using Molten.Collections;

namespace Molten.Graphics.Overlays
{
    public class OverlayProvider
    {
        ThreadedList<IRenderOverlay> _overlays = new ThreadedList<IRenderOverlay>();
        int _current = 0;

        internal OverlayProvider()
        {
            _overlays.Add(new RenderProfilerOverlay());
        }

        public void Render(Timing time, SpriteBatcher sb, RenderProfiler rendererProfiler, RenderProfiler sceneProfiler, RenderCamera camera)
        {
            if (Font == null)
                return;

            _overlays.For(0, (index, overlay) => overlay.OnRender(time, sb, Font, rendererProfiler, sceneProfiler, camera));
        }

        public void Add(IRenderOverlay overlay)
        {
            _overlays.Add(overlay);
        }

        public void Remove(IRenderOverlay overlay)
        {
            _overlays.Remove(overlay);
        }

        public string GetTitle(int index)
        {
            return _overlays[index].Title;
        }

        /// <summary>
        /// Moves to the previous overlay and returns true if successful. Returns false if there are no previous overlays.
        /// </summary>
        /// <returns></returns>
        public bool Previous()
        {
            if (_current - 1 < 0)
                return false;
            else
                _current--;

            return true;
        }

        /// <summary>
        /// Moves to the next overlay and returns true if successful. Returns false if there are no more overlays.
        /// </summary>
        /// <returns></returns>
        public bool Next()
        {
            if (_current + 1 == _overlays.Count)
                return false;
            else
                _current++;

            return true;
        }

        /// <summary>
        /// Gets the number of active debug overlays available for viewing.
        /// </summary>
        public int Count => _overlays.Count;

        /// <summary>
        /// Gets or sets the currently-active overlay index. When setting, the value will be constrained to a number between 0 and <see cref="Count"/>.
        /// </summary>
        public int Current
        {
            get => _current;
            set => _current = int.Clamp(value, 0, Math.Max(0, _overlays.Count - 1));
        }

        /// <summary>
        /// Gets or sets the font used when rendering overlay text.
        /// </summary>
        public SpriteFont Font { get; set; }
    }
}
