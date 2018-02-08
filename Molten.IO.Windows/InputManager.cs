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

        public InputManager(Logger log)
        {
            _log = log;
            _input = new DirectInput();
            _groups = new Dictionary<IWindowSurface, SurfaceGroup>();
            _gamepads = new List<GamepadHandler>();

            for (int i = 0; i < 4; i++)
            {
                GamepadHandler handler = new GamepadHandler((GamepadIndex)i);
                _gamepads.Add(handler);
                handler.Initialize(this, _log, null);
            }
        }

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

        public void SetActiveWindow(IWindowSurface surface)
        {
            if(surface != _activeSurface)
            {
                if (!_groups.TryGetValue(surface, out _activeGroup))
                {
                    _activeGroup = new SurfaceGroup(this, _log, surface);
                    _groups.Add(surface, _activeGroup);
                }
            }
        }

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
    }
}
