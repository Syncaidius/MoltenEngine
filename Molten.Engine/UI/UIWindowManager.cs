using SharpDX;
using Molten.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UIWindowManager
    {
        public enum MinimizeDirection
        {
            Horizontal = 0,

            Vertical = 1,
        }

        const int MINIMIZE_SPACING = 5;

        UISystem _ui;
        List<UIWindow> _windows;
        List<UIWindow> _minimized;
        List<UIWindow> _closed;

        IntVector2 _minimizeLocation;
        MinimizeDirection _direction;

        internal UIWindowManager(UISystem ui)
        {
            _ui = ui;
            _windows = new List<UIWindow>();
            _minimized = new List<UIWindow>();
            _closed = new List<UIWindow>();
        }

        internal void RegisterWindow(UIWindow window)
        {
            _windows.Add(window);
        }

        internal void UnregisterWindow(UIWindow window)
        {
            _windows.Remove(window);
        }

        /// <summary>Refreshes the layout of minimized windows.</summary>
        private void Refresh()
        {
            switch (_direction)
            {
                case UIWindowManager.MinimizeDirection.Horizontal:
                    int xPos = _minimizeLocation.X;

                    foreach (UIWindow window in _minimized)
                    {
                        Vector2 titleSize = window.Title.GetSize();
                        int height = (int)titleSize.Y + 4;
                        int width = (int)titleSize.X + 4 + UIWindow.ICON_SIZE + UIWindow.ICON_SPACING + 10;

                        window.LocalBounds = new Rectangle()
                        {
                            X = xPos,
                            Y = _minimizeLocation.Y,
                            Width = width,
                            Height = height,
                        };

                        xPos += width + MINIMIZE_SPACING;
                    }
                    break;
                case UIWindowManager.MinimizeDirection.Vertical:
                    int yPos = _minimizeLocation.Y;

                    foreach (UIWindow window in _minimized)
                    {
                        Vector2 titleSize = window.Title.GetSize();
                        int height = (int)titleSize.Y + 4;
                        int width = (int)titleSize.X + 4 + UIWindow.ICON_SIZE + UIWindow.ICON_SPACING + 10;

                        window.LocalBounds = new Rectangle()
                        {
                            X = _minimizeLocation.X,
                            Y = yPos,
                            Width = width,
                            Height = height,
                        };

                        yPos += height + MINIMIZE_SPACING;
                    }
                    break;
            }
        }

        internal void Minimize(UIWindow window)
        {
            if(!_minimized.Contains(window))
                _minimized.Add(window);

            Refresh();
        }

        internal void Restore(UIWindow window)
        {
            if(_minimized.Contains(window))
                _minimized.Remove(window);

            Refresh();
        }

        internal void Close(UIWindow window)
        {
            if (_minimized.Contains(window))
                _minimized.Remove(window);

            if(!_closed.Contains(window))
                _closed.Add(window);
        }

        internal void Open(UIWindow window)
        {
            if (_closed.Contains(window))
                _closed.Remove(window);

            if (_minimized.Contains(window))
                _minimized.Remove(window);
        }

        public void MinimizeAll()
        {
            for (int i = 0; i < _windows.Count; i++)
                Minimize(_windows[i]);
        }

        public void RestoreAll()
        {
            for (int i = 0; i < _minimized.Count; i++)
                Restore(_minimized[i]);

            _minimized.Clear();
        }

        /// <summary>Gets or sets the location on the screen at which minimized windows are placed.</summary>
        public IntVector2 MinimizeLocation
        {
            get { return _minimizeLocation; }
            set
            {
                _minimizeLocation = value;
                Refresh();
            }
        }

        /// <summary>Gets or sets the flow direction of minimized window bars.</summary>
        public MinimizeDirection Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                Refresh();
            }
        }
    }
}
