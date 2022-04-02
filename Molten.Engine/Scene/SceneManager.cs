using Molten.Input;

namespace Molten
{
    internal class SceneManager : IDisposable
    {
        public event SceneInputEventHandler<MouseButton> OnObjectFocused;
        public event SceneInputEventHandler<MouseButton> OnObjectUnfocused;

        List<Scene> _scenes;
        List<SceneClickTracker> _trackers;

        internal SceneManager()
        {
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

        internal void SetFocus(IPointerReceiver acceptor)
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

        internal void HandleInput(MouseDevice mouse, TouchDevice touch, KeyboardDevice kb, GamepadDevice gamepad, Timing timing)
        {
            if (mouse != null)
                HandleMouseInput(mouse, timing);

            if (touch != null)
            {
                if (mouse != null && mouse.IsConnected && mouse.IsEnabled)
                {
                    // Make sure we're not emulating touch input using the same mouse device as above, if available.
                    if (touch is MouseTouchEmulatorDevice mted && mted.Mouse != mouse)
                        HandleTouchInput(touch, timing);
                }
                else
                {
                    HandleTouchInput(touch, timing);
                }
            }

            for (int i = _scenes.Count - 1; i >= 0; i--)
            {
                Scene scene = _scenes[i];
                for(int j = scene.Layers.Count - 1; j >= 0; j--)
                {
                    SceneLayer layer = scene.Layers[j];
                    for (int k = layer.InputHandlers.Count - 1; k >= 0; k--)
                        layer.InputHandlers[k].HandleInput(mouse, touch, kb, gamepad, timing);
                }
            }
        }

        private void HandleTouchInput(TouchDevice touch ,Timing time)
        {
            // TODO implement SceneTouchTracker class
            // TODO do touch-specific input handling and call relevant IInputAcceptor methods.
        }

        private void HandleMouseInput(MouseDevice mouse, Timing time)
        {
            Vector2F cursorPos = (Vector2F)mouse.Position;

            for (int i = _scenes.Count - 1; i >= 0; i--)
            {
                IPointerReceiver newHover = _scenes[i].PickObject(cursorPos);

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

                // Invoke hover event if possible
                if (Hovered != null)
                {
                    Hovered.CursorHover(cursorPos);

                    // Handle scroll wheel event
                    if (mouse.ScrollWheel.Delta != 0)
                        Hovered.CursorWheelScroll(mouse.ScrollWheel);
                }
            }

            // Update all button trackers
            for (int j = 0; j < _trackers.Count; j++)
                _trackers[j].Update(this, mouse, time);
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
        public IPointerReceiver Focused { get; private set; }

        /// <summary>Gets the component that the pointer is currently hovering over.</summary>
        public IPointerReceiver Hovered { get; private set; } = null;
    }
}
