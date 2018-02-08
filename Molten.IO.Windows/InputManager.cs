using SharpDX.DirectInput;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.IO
{
    public class InputManager : EngineObject, IInputManager
    {
        DirectInput _input;

        Logger _log;

        List<GamepadHandler> _gamepads;
        Dictionary<IWindowSurface, SurfaceGroup> _groups;
        SurfaceGroup _activeGroup;
        IWindowSurface _activeSurface;
        WindowsClipboard _clipboard;

        /// <summary>Initializes the current input manager instance. Avoid calling this directly unless you know what you are doing.</summary>
        /// <param name="settings">The <see cref="InputSettings"/> that was provided when the engine was instanciated.</param>
        /// <param name="log">A logger.</param>
        public void Initialize(InputSettings settings, Logger log)
        {
            _log = log;
            _input = new DirectInput();
            _groups = new Dictionary<IWindowSurface, SurfaceGroup>();
            _gamepads = new List<GamepadHandler>();
            _clipboard = new WindowsClipboard();

            for (int i = 0; i < 4; i++)
            {
                GamepadHandler handler = new GamepadHandler((GamepadIndex)i);
                _gamepads.Add(handler);
                handler.Initialize(this, _log, null);
            }
        }

        /// <summary>Gets a new or existing instance of an input handler for the specified <see cref="IWindowSurface"/>.</summary>
        /// <typeparam name="T">The type of handler to retrieve.</typeparam>
        /// <param name="surface">The surface for which to bind and return an input handler.</param>
        /// <returns>An input handler of the specified type.</returns>
        public T GetHandler<T>(IWindowSurface surface) where T : InputHandlerBase, new()
        {
            SurfaceGroup grp = null;
            if(!_groups.TryGetValue(surface, out grp))
            {
                grp = new SurfaceGroup(this, _log, surface);
                _groups.Add(surface, grp);
            }

            return grp.GetHandler<T>();
        }

        /// <summary>Sets the active/focused <see cref="IWindowSurface"/> which will receive input. Only one can receive input at any one time.</summary>
        /// <param name="surface">The surface to be set as active.</param>
        public void SetActiveWindow(IWindowSurface surface)
        {
            if(surface != _activeSurface)
            {
                if (!_groups.TryGetValue(surface, out _activeGroup))
                {
                    _activeGroup = new SurfaceGroup(this, _log, surface);
                    _groups.Add(surface, _activeGroup);
                }

                _activeSurface = surface;
            }
        }

        /// <summary>Update's the current input manager. Avoid calling directly unless you know what you're doing.</summary>
        /// <param name="time">An instance of timing for the current thread.</param>
        public void Update(Timing time)
        {
            _activeGroup?.Update(time);

            for (int i = 0; i < _gamepads.Count; i++)
                _gamepads[i].Update(time);
        }

        /// <summary>Retrieves a gamepad handler.</summary>
        /// <param name="index">The index of the gamepad.</param>
        /// <returns></returns>
        public GamepadHandler GetGamepadHandler(GamepadIndex index)
        {
            return _gamepads[(int)index];
        }

        protected override void OnDispose()
        {
            DisposeObject(ref _input);

            for (int i = 0; i < _gamepads.Count; i++)
            {
                GamepadHandler gph = _gamepads[i];
                DisposeObject(ref gph);
            }
            _gamepads.Clear();
        }

        public DirectInput DirectInput { get { return _input; } }

        /// <summary>Gets the handler for the gamepad at GamepadIndex.One.</summary>
        public GamepadHandler GamePad { get { return _gamepads[0]; } }

        public IClipboard Clipboard => _clipboard;
    }
}
