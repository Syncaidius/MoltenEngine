using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Molten.Input
{
    public class AndroidInputManager : InputManager
    {
        AndroidInputNavigation _navigation;

        public override IClipboard Clipboard => throw new NotImplementedException();

        public override IInputNavigation Navigation => _navigation;

        protected override void OnInitialize()
        {
            _navigation = new AndroidInputNavigation();
        }

        protected override T OnGetCustomDevice<T>()
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            return Activator.CreateInstance(typeof(T), flags, null, args: new object[] { this }, null) as T;
        }

        protected override void OnBindSurface(INativeSurface surface)
        {
            _navigation.SetSurface(surface);
        }

        protected override GamepadDevice OnGetGamepad(int index, GamepadSubType subtype)
        {
            throw new NotImplementedException();
        }

        public override KeyboardDevice GetKeyboard()
        {
            throw new NotImplementedException();
        }

        public override MouseDevice GetMouse()
        {
            throw new NotImplementedException();
        }

        public override TouchDevice GetTouch()
        {
            return GetCustomDevice<AndroidTouchDevice>();
        }

        /// <summary>Update's the current input manager. Avoid calling directly unless you know what you're doing.</summary>
        /// <param name="time">An instance of timing for the current thread.</param>
        protected override void OnUpdate(Timing time)
        {
            _navigation.Update(time);
        }

        protected override void OnClearState()
        {
            _navigation.ClearState();
        }
    }
}
