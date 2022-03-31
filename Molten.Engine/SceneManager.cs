using Molten.Input;

namespace Molten
{
    internal class SceneManager : IDisposable
    {
        public event SceneInputEventHandler<MouseButton> OnObjectFocused;
        public event SceneInputEventHandler<MouseButton> OnObjectUnfocused;

        List<Scene> _scenes;
        List<SceneClickTracker> _trackers;
        UISettings _settings;

        internal SceneManager(UISettings settings)
        {
            _settings = settings;
            _trackers = new List<SceneClickTracker>();
            _scenes = new List<Scene>();

            MouseButton[] buttons = ReflectionHelper.GetEnumValues<MouseButton>();
            foreach(MouseButton b in buttons)
            {
                if (b == MouseButton.None)
                    continue;

                _trackers.Add(new SceneClickTracker(b));
            }
        }

        internal void Add(Scene scene)
        {
            _scenes.Add(scene);
        }

        internal void Remove(Scene scene)
        {
            _scenes.Remove(scene);
        }

        public void Dispose()
        {
            // Dispose of scenes
            for (int i = 0; i < _scenes.Count; i++)
                _scenes[i].Dispose();

            _scenes.Clear();
        }

        internal void SetFocus(IInputAcceptor acceptor)
        {
            if (Focused != acceptor && acceptor != null)
            {
                acceptor.CursorFocus();

                OnObjectFocused?.Invoke(new SceneInputData<MouseButton>()
                {
                    Object = acceptor,
                    InputValue = MouseButton.None,
                });
            }

            Focused = acceptor;
        }

        internal void Unfocus()
        {
            if (Focused != null)
            {
                Focused.CursorUnfocus();

                OnObjectUnfocused?.Invoke(new SceneInputData<MouseButton>()
                {
                    Object = Focused,
                    InputValue = MouseButton.None,
                });
            }

            Focused = null;
        }

        internal void HandleInput(TouchDevice touch, Timing time)
        {
            // TODO implement SceneTouchTracker class
            // TODO do touch-specific input handling and call relevant IInputAcceptor methods.
        }

        internal void HandleInput(MouseDevice mouse, Timing time)
        {
            Vector2F cursorPos = (Vector2F)mouse.Position;
            Vector2F cursorDelta = (Vector2F)mouse.Delta;

            for (int i = _scenes.Count - 1; i >= 0; i--)
            {
                IInputAcceptor newHover = _scenes[i].PickObject(cursorPos);

                if (newHover == null)
                {
                    // Trigger leave on previous hover component.
                    Hovered?.CursorLeave(cursorPos);
                    Hovered = null;
                }
                else
                {
                    if (Hovered != newHover)
                    {
                        //trigger leave on old hover component.
                        Hovered?.CursorLeave(cursorPos);

                        //set new hover component and trigger it's enter event
                        Hovered = newHover;
                        Hovered.CursorEnter(cursorPos);
                    }
                }

                // Update all button trackers
                for (int j = 0; j < _trackers.Count; j++)
                    _trackers[j].Update(this, mouse, time);

                // Invoke hover event if possible
                if (Hovered != null)
                {
                    Hovered.CursorHover(cursorPos);

                    // Handle scroll wheel event
                    if (mouse.ScrollWheel.Delta != 0)
                        Hovered.CursorWheelScroll(mouse.ScrollWheel);
                }
            }
        }

        internal void Update(Timing time)
        {
            // Run through all the scenes and update if enabled.
            foreach (Scene scene in _scenes)
            {
                if (scene.IsEnabled)
                    scene.Update(time);
            }
        }

        /// <summary>Gets the component which is currently focused.</summary>
        public IInputAcceptor Focused { get; private set; }

        /// <summary>Gets the component that the pointer is currently hovering over.</summary>
        public IInputAcceptor Hovered { get; private set; } = null;
    }
}
