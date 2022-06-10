using Molten.Collections;
using Molten.Input;

namespace Molten
{
    internal class SceneManager : IDisposable
    {
        public event SceneInputEventHandler<PointerButton> OnObjectFocused;
        public event SceneInputEventHandler<PointerButton> OnObjectUnfocused;

        List<Scene> _scenes;
        List<SceneClickTracker> _trackers;
        ThreadedQueue<SceneChange> _pendingChanges;

        internal SceneManager()
        {
            _trackers = new List<SceneClickTracker>();
            _scenes = new List<Scene>();
            _pendingChanges = new ThreadedQueue<SceneChange>();

            PointerButton[] buttons = ReflectionHelper.GetEnumValues<PointerButton>();
            foreach(PointerButton b in buttons)
            {
                if (b == PointerButton.None)
                    continue;

                _trackers.Add(new SceneClickTracker(b));
            }
        }

        internal void QueueChange(Scene scene, SceneChange change)
        {
            change.Scene = scene;
            _pendingChanges.Enqueue(change);
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
                acceptor.PointerFocus();

                OnObjectFocused?.Invoke(new SceneInputData<PointerButton>()
                {
                    Object = acceptor,
                    InputValue = PointerButton.None,
                });
            }

            Focused = acceptor;
        }

        internal void Unfocus()
        {
            if (Focused != null)
            {
                Focused.PointerUnfocus();

                OnObjectUnfocused?.Invoke(new SceneInputData<PointerButton>()
                {
                    Object = Focused,
                    InputValue = PointerButton.None,
                });
            }

            Focused = null;
        }

        internal void HandleInput(MouseDevice mouse, TouchDevice touch, KeyboardDevice kb, GamepadDevice gamepad, Timing timing)
        {
            if (mouse != null)
                HandlePointerInput(mouse, timing);

            if (touch != null)
                HandlePointerInput(touch, timing);

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

        private void HandlePointerInput(PointingDevice pDevice, Timing time)
        {
            Vector2F cursorPos = pDevice.Position;

            if (pDevice is MouseDevice mouse)
            {
                for (int i = _scenes.Count - 1; i >= 0; i--)
                {
                    IPointerReceiver newHover = _scenes[i].PickObject(cursorPos);

                    if (newHover == null)
                    {
                        // Trigger leave on previous hover component.
                        Hovered?.PointerLeave(cursorPos);
                        Hovered = null;
                    }
                    else
                    {
                        if (Hovered != newHover)
                        {
                            //trigger leave on old hover component.
                            Hovered?.PointerLeave(cursorPos);

                            //set new hover component and trigger it's enter event
                            Hovered = newHover;
                            Hovered.PointerEnter(cursorPos);
                        }
                    }

                    // Invoke hover event if possible
                    if (Hovered != null)
                    {
                        Hovered.PointerHover(cursorPos);

                        // Handle scroll wheel event
                        if (mouse.ScrollWheel.Delta != 0)
                            Hovered.PointerScroll(mouse.ScrollWheel);
                    }
                }
            }

            // Update all button trackers
            for (int j = 0; j < _trackers.Count; j++)
                _trackers[j].Update(this, pDevice, time);
        }

        internal void Update(Timing time)
        {
            // TODO implement scene-management-wide queue
            while (_pendingChanges.TryDequeue(out SceneChange change))
                change.Process();

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
