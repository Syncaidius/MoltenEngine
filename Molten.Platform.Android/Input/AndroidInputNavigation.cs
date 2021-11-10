using Molten.Graphics;
using Molten.Input;
using Molten.Utility;
using System;

namespace Molten
{
    public class AndroidInputNavigation : IInputNavigation
    {
        public bool IsBackPressed { get; private set; }

        public bool IsContextButtonPressed => throw new NotImplementedException();

        public event MoltenEventHandler<IInputNavigation> OnBackPressed;
        public event MoltenEventHandler<IInputNavigation> OnContextButtonPressed;

        AndroidViewSurface _surface;
        bool _backProcessed;

        internal AndroidInputNavigation() { }

        internal void Clear()
        {
            _backProcessed = false;
            IsBackPressed = false;
        }

        internal void SetSurface(INativeSurface surface)
        {
            if (_surface != surface)
            {
                // Unsubscribe from previous surface
                if (_surface != null)
                {
                    if (_surface is AndroidViewSurface vSurface)
                        vSurface.TargetActivity.BackPressed -= TargetActivity_BackPressed;
                }

                if (surface is AndroidViewSurface vNewSurface)
                {
                    _surface = vNewSurface;
                    _surface.TargetActivity.BackPressed += TargetActivity_BackPressed;
                }
                else
                {
                    _surface = null;
                }
            }
        }

        private void TargetActivity_BackPressed(IMoltenAndroidActivity activity)
        {
            _backProcessed = false;
            IsBackPressed = true;
            OnBackPressed?.Invoke(this);
        }

        internal void Update(Timing time)
        {
            // Reset back if press was processed.
            if (_backProcessed)
                IsBackPressed = false;

            _backProcessed = IsBackPressed;
        }
    }
}