using Molten.Input;
using Molten.UI;
using System;
using System.Collections.Generic;

namespace Molten
{
    internal class SceneManager : IDisposable
    {
        public event SceneInputEventHandler<MouseButton> OnFocus;
        public event SceneInputEventHandler<MouseButton> OnUnfocus;

        List<Scene> _scenes = new List<Scene>();
        List<SceneClickTracker> _trackers;
        Dictionary<MouseButton, SceneClickTracker> _trackerByButton;
        UITooltip _tooltip;
        double _tooltipTimer;
        UISettings _settings;

        internal SceneManager(UISettings settings)
        {
            _settings = settings;
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

        internal void SetFocus(ICursorAcceptor acceptor)
        {
            if (Focused != acceptor && acceptor != null)
            {
                acceptor.InvokeCursorFocus();

                OnFocus?.Invoke(new SceneEventData<MouseButton>()
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
                Focused.InvokeCursorUnfocus();

                OnUnfocus?.Invoke(new SceneEventData<MouseButton>()
                {
                    Object = Focused,
                    InputValue = MouseButton.None,
                });
            }

            Focused = null;
        }

        internal void HandleInput(IMouseDevice mouse, Timing time)
        {
            Vector2F cursorPos = mouse.Position;
            Vector2F cursorDelta = mouse.Delta;
            for (int i = _scenes.Count - 1; i >= 0; i--)
            {
                ICursorAcceptor newHover = _scenes[i].PickObject(cursorPos);

                if (newHover == null)
                {
                    // Trigger leave on previous hover component.
                    if (Hovered != null)
                    {
                        Hovered.InvokeCursorLeave(cursorPos);

                        // Set tooltip.
                        _tooltip.Text.Text = "";
                    }

                    // Set new-current as null.
                    Hovered = null;
                }
                else
                {
                    if (Hovered != newHover)
                    {
                        //trigger leave on old hover component.
                        if (Hovered != null)
                            Hovered.InvokeCursorLeave(cursorPos);

                        //set new hover component and trigger it's enter event
                        Hovered = newHover;
                        Hovered.InvokeCursorEnter(cursorPos);

                        // Set tooltip.
                        _tooltipTimer = 0;
                        _tooltip.Text.Text = Hovered.Tooltip;
                    }
                }

                // Update all button trackers
                for (int j = 0; j < _trackers.Count; j++)
                    _trackers[j].Update(this, mouse, time);

                // Invoke hover event if possible
                if (Hovered != null)
                {
                    Hovered.InvokeCursorHover(cursorPos);

                    // Update tooltip status
                    if (_tooltipTimer < _settings.TooltipDelay)
                    {
                        _tooltip.IsVisible = false;
                        _tooltipTimer += time.ElapsedTime.TotalMilliseconds;
                    }
                    else
                    {
                        _tooltip.IsVisible = true;
                    }

                    _tooltip.Position = cursorPos + new Vector2F(16); // TODO Offset this position based on font/cursor size, not a hard-coded value.

                    // Handle scroll wheel event
                    if (mouse.WheelDelta != 0)
                        Hovered.InvokeCursorWheelScroll(mouse.WheelPosition, mouse.WheelDelta);
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
        public ICursorAcceptor Focused { get; private set; }

        /// <summary>Gets the component that the pointer is currently hovering over.</summary>
        public ICursorAcceptor Hovered { get; private set; } = null;
    }
}
