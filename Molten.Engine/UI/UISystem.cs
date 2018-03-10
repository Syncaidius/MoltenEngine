using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Molten.Graphics;
using Molten.Graphics.Textures;
using Molten.IO;

namespace Molten.UI
{
    public class UISystem : EngineObject
    {
        const int PROXY_BIAS = 5000000;

        UITooltip _tooltip;
        UIContainer _screen;
        IInputManager _input;
        UIWindowManager _windowManager;

        bool _inputEnabled = true;

        List<UIClickTracker> _trackers;
        Dictionary<MouseButton, UIClickTracker> _trackerByButton;
        UIComponent _focused;
        UIComponent _hoverComponent = null;

        double _tooltipTimer;
        float _tooltipDelay = 500;
        float _dragThreshold = 10.0f;

        IWindowSurface _surface;
        MouseHandler _mouse;
        KeyboardHandler _keyboard;
        Scene _scene;
        Engine _engine;

        public event UIComponentEventHandler<MouseButton> OnFocus;
        public event UIComponentEventHandler<MouseButton> OnUnfocus;

        internal UISystem(Scene scene, Engine engine)
        {
            _scene = scene;
            _engine = engine;
            _input = _engine.Input;
            _windowManager = new UIWindowManager(this);
            _trackers = new List<UIClickTracker>();
            _trackerByButton = new Dictionary<MouseButton, UIClickTracker>();

            AddTracker(MouseButton.Left);
            AddTracker(MouseButton.Right);
            AddTracker(MouseButton.Middle);

            // Setup screen component
            _screen = new UIContainer(engine)
            {
                LocalBounds = new Rectangle()
                {
                    Width = 1,
                    Height = 1,
                },
                Name = "Screen",
                UI = this,
            };            

            _tooltip = new UITooltip(engine);
        }

        internal void SetSurface(IWindowSurface newSurface)
        {
            if (newSurface == _surface)
                return;

            if(_surface != null)
                _surface.OnPostResize -= Surface_OnPostResize;

            _screen.LocalBounds = new Rectangle()
            {
                Width = newSurface.Width,
                Height = newSurface.Height,
            };

            newSurface.OnPostResize += Surface_OnPostResize;
            _surface = newSurface;
        }

        void Surface_OnPostResize(ITexture surface)
        {
            _screen.LocalBounds = new Rectangle()
            {
                X = 0,
                Y = 0,
                Width = _surface.Width,
                Height = _surface.Height,
            };
        }

        /// <summary>Removes all active components from the UI system.</summary>
        public void Clear()
        {
            _screen.Children.Clear();
        }

        public void SetFocus(UIComponent component)
        {
            if (_focused != component && component != null)
            {
                component.InvokeFocus();

                OnFocus?.Invoke(new UIEventData<MouseButton>()
                {
                    Component = component,
                    Position = component.GlobalBounds.TopLeft,
                });
            }

            _focused = component;
        }

        public void Unfocus()
        {
            if (_focused != null)
            {
                _focused.InvokeUnfocus();

                OnUnfocus?.Invoke(new UIEventData<MouseButton>()
                {
                    Component = _focused,
                    Position = _focused.GlobalBounds.TopLeft,
                });
            }
        
            _focused = null;
        }

        /// <summary>Adds a root component to UI system.</summary>
        /// <param name="component">The component or parent UI to add to the system.</param>
        /// <returns>True if successful.</returns>
        public void AddUI(UIComponent component)
        {
            UIComponent matchingChild = null;

            if (component.Name == _screen.Name && component != _screen)
            {
                UIComponent child = null;

                // Take all of it's children and add them individuaally
                for (int i = 0; i < component.Children.Count; i++)
                {
                    child = component.Children[i];

                    matchingChild = _screen.HasChild(child.Name);
                    if (matchingChild == null)
                        _screen.AddChild(child);
                }
            }
            else {
                // Check if a child of the same name exists.
                matchingChild = _screen.HasChild(component.Name);
                if (matchingChild == null)
                    _screen.AddChild(component);
            }
        }

        /// <summary>Removes a container component from the UI system.</summary>
        /// <param name="childName">The name of the container to remove.</param>
        /// <returns>True if the component was successfully removed, false if it wasn't found.</returns>
        public void RemoveUI(string childName)
        {
            UIComponent matchingChild = null;
            
            // What is this? :o
            if (childName == _screen.Name)
            {
                for (int i = _screen.Children.Count - 1; i >= 0; i--)
                    _screen.RemoveChild(_screen.Children[i]);
            }
            else
            {

                matchingChild = _screen.HasChild(childName);

                if (matchingChild != null)
                    _screen.RemoveChild(matchingChild);
            }
        }

        public bool RemoveUI(UIComponent container)
        {
            return _screen.RemoveChild(container);
        }

        private void AddTracker(MouseButton button)
        {
            UIClickTracker tracker = new UIClickTracker(button);
            _trackers.Add(tracker);
            _trackerByButton.Add(button, tracker);
        }

        private void SetDragThreshold(float threshold)
        {
            _dragThreshold = threshold;
            for (int i = 0; i < _trackers.Count; i++)
                _trackers[i].DragThreshold = threshold;
        }

        private UIComponent HandlePressStarted(Vector2F inputPos)
        {
            UIComponent result = _screen.GetComponent(inputPos);

            return result;
        }

        internal void Update(Timing time)
        {
            Vector2F mousePos = _mouse.Position;
            Vector2F mouseMove = _mouse.Moved;

            _screen.Update(time);

            UIComponent newHover = HandlePressStarted(mousePos);
            if (newHover == null)
            {
                //trigger leave on previous hover component.
                if (_hoverComponent != null)
                {
                    _hoverComponent.InvokeLeave(mousePos);

                    // Set tooltip.
                    _tooltip.Text.Text = "";
                }

                //set new-current as null.
                _hoverComponent = null;
            }
            else
            {
                if (_hoverComponent != newHover)
                {
                    //trigger leave on old hover component.
                    if (_hoverComponent != null)
                        _hoverComponent.InvokeLeave(mousePos);

                    //set new hover component and trigger it's enter event
                    _hoverComponent = newHover;
                    _hoverComponent.InvokeEnter(mousePos);

                    // Set tooltip.
                    _tooltipTimer = 0;
                    _tooltip.Text.Text = _hoverComponent.Tooltip;
                }
            }

            // Update all button trackers
            if (_inputEnabled)
            {
                for (int i = 0; i < _trackers.Count; i++)
                    _trackers[i].Update(this, time);
            }

            // Invoke hover event if possible
            if (_hoverComponent != null)
            {
                _hoverComponent.InvokeHover(mousePos);

                // Update tooltip status
                if (_tooltipTimer < _tooltipDelay)
                {
                    _tooltip.IsVisible = false;
                    _tooltipTimer += time.ElapsedTime.TotalMilliseconds;
                }
                else
                {
                    _tooltip.IsVisible = true;
                }

                _tooltip.Position = mousePos + new Vector2F(16);

                // Handle scroll wheel event
                if (_mouse.WheelDelta != 0)
                    _hoverComponent.InvokeScrollWheel(_mouse.WheelDelta);
            }
        }

        /// <summary>Gets the root UI component which represents the screen.</summary>
        public UIContainer Screen { get { return _screen; } }

        /// <summary>Gets the component which is currently focused.</summary>
        public UIComponent Focused
        {
            get { return _focused; }
        }

        /// <summary>Gets the component that the pointer is currently hovering over.</summary>
        public UIComponent Hovered { get { return _hoverComponent; } }

        /// <summary>Gets the window manager bound to the UI system.</summary>
        public UIWindowManager WindowManager { get { return _windowManager; } }

        public MouseHandler Mouse => _mouse;

        public KeyboardHandler Keyboard => _keyboard;

        /// <summary>Gets or sets the number of pixels the mouse must be dragged before it 
        /// begins triggering drag events. Resets when the left mouse button is released.</summary>
        public float DragThreshold
        {
            get { return _dragThreshold; }
            set
            {
                _dragThreshold = value;
                SetDragThreshold(value);
            }
        }

        /// <summary>Gets or sets the delay before a tooltip is shown when the mouse is kept stationary.</summary>
        public float TooltipDelay
        {
            get { return _tooltipDelay; }
            set { _tooltipDelay = value; }
        }

        /// <summary>Gets or sets whether or not the UI system is accepting input.</summary>
        public bool InputEnabled
        {
            get { return _inputEnabled; }
            set { _inputEnabled = value; }
        }
    }
}