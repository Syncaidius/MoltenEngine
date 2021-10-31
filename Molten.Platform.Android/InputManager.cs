using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public class InputManager : EngineObject, IInputManager
    {
        Logger _log;

        public void Initialize(InputSettings settings, Logger log)
        {
            _log = log;
        }

        public IClipboard Clipboard => throw new NotImplementedException();

        public IInputCamera Camera { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IGamepadDevice GetGamepad(GamepadIndex index)
        {
            throw new NotImplementedException();
        }

        public IKeyboardDevice GetKeyboard()
        {
            throw new NotImplementedException();
        }

        public IMouseDevice GetMouse()
        {
            throw new NotImplementedException();
        }

        public void Update(Timing time)
        {
            throw new NotImplementedException();
        }

        T IInputManager.GetCustomDevice<T>()
        {
            throw new NotImplementedException();
        }
    }
}
