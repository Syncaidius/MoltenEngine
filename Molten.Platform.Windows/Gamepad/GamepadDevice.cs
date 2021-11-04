using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.XInput;
using SharpDX;
using System.Runtime.InteropServices;

namespace Molten.Input
{
    using Molten.Graphics;
    using Molten.Utilities;
    using State = SharpDX.XInput.State;

    public class GamepadDevice : WinInputDeviceBase<GamepadButtonFlags>, IGamepadDevice
    {
        Gamepad _state;
        Gamepad _statePrev;
        Controller _pad;
        Capabilities _capabilities;
        int _padIndex;
        float _vibrationLeft;
        float _vibrationRight;
        GamepadStick _leftThumbstick;
        GamepadStick _rightThumbstick;
        GamepadTrigger _leftTrigger;
        GamepadTrigger _rightTrigger;
        GamepadButtonFlags _buttons;
        GamepadButtonFlags _prevButtons;

        string _deviceName;
        bool _isConnected;

        Dictionary<int, int> _heldTimers; // keeps track of how long buttons are held for.

        public GamepadDevice(GamepadIndex index)
        {
            _padIndex = (int)index;
        }

        internal override void Initialize(IInputManager manager, Logger log)
        {
            base.Initialize(manager, log);
            _pad = new Controller((UserIndex)_padIndex);
            _state = new Gamepad();

            //initialize hold timer dictionaries.
            _heldTimers = new Dictionary<int, int>();
            _deviceName = "Gamepad " + _padIndex;
            _isConnected = _pad.IsConnected;

            _leftThumbstick = new GamepadStick(32767);
            _rightThumbstick = new GamepadStick(32767);
            _leftTrigger = new GamepadTrigger(255);
            _rightTrigger = new GamepadTrigger(255);

            //only get state and capabilities if connected.
            if (_isConnected)
                RetrieveDeviceInformation();

            // Initialize previous state
            _statePrev = new Gamepad();
        }

        internal override void Bind(INativeSurface surface)
        {
            // TODO simply store the window we're bound to and only accept input if it is focused.
        }

        internal override void Unbind(INativeSurface surface)
        {
            // TODO simply store the window we're bound to and only accept input if it is focused.
        }

        public override void ClearState()
        {
            //reset hold timers
            foreach (int button in _heldTimers.Keys)
                _heldTimers[button] = 0;

            _leftThumbstick.Clear();
            _rightThumbstick.Clear();
            _leftTrigger.Clear();
            _rightTrigger.Clear();

            _buttons = GamepadButtonFlags.None;
            _prevButtons = GamepadButtonFlags.None;
        }

        protected override void OnDispose()
        {
            SetVibration(0, 0);
        }

        private void RetrieveDeviceInformation()
        {
            State padState = _pad.GetState();
            _state = padState.Gamepad;
            _capabilities = _pad.GetCapabilities(DeviceQueryType.Gamepad);

            // Add the sub-type into the name if device is not a normal gamepad.
            if (_capabilities.SubType != DeviceSubType.Gamepad)
                _deviceName += " ( " + _capabilities.SubType + ")";
        }

        /// <summary>Returns true if the button was only just pressed. Returns false if it was pressed in the last update too.</summary>
        /// <param name="playerIndex">The controller index to read from.</param>
        /// <param name="value">The button(s) to check.</param>
        /// <param name="repeatCheck">Set to true if the button must not have been pressed in the previous update.</param>
        /// <returns>Returns true if the button is considered pressed.</returns>
        public override bool IsPressed(GamepadButtonFlags value)
        {
            return _buttons.HasFlag(value);
        }

        public override bool IsTapped(GamepadButtonFlags value)
        {
            return (_buttons.HasFlag(value) == true && _prevButtons.HasFlag(value) == false);
        }

        /// <summary>Returns true if the specified button combination was pressed in both the previous and current frame. </summary>
        /// <param name="playerIndex">The controller index to read from.</param>
        /// <param name="buttons">The button(s) to do a held test for.</param>
        /// <param name="interval">The interval of time the button(s) must be held for to be considered as held.</param>
        /// <returns>True if button(s) considered held.</returns>
        public override bool IsHeld(GamepadButtonFlags buttons)
        {
            return _buttons.HasFlag(buttons) && _prevButtons.HasFlag(buttons);
        }

        /// <summary>Returns details about the status of a battery.</summary>
        /// <returns></returns>
        public BatteryInformation GetBatteryDetails(BatteryDeviceType type)
        {
            return _pad.GetBatteryInformation(type);
        }

        public override void OpenControlPanel() { }

        /// <summary>Update input handler.</summary>
        /// <param name="time">The snapshot of time.</param>
        /// <param name="releaseInput">If set to true, will reset all held timers and stop retrieving the latest state.</param>
        internal override void Update(Timing time)
        {
            // Handle event invocation.
            if (_isConnected != _pad.IsConnected)
            {
                _isConnected = _pad.IsConnected;
                InvokeConnectionStatus(_pad.IsConnected);

                if (_isConnected)
                    InvokeOnConnected();
                else
                    InvokeOnDisconnected();
            }        

            //store previous states
            for (int gp = 0; gp < 4; gp++)
                _statePrev = _state;

            //TODO test against all windows, not just the current
            //release input if window is not focused.
            IntPtr focusedHandle = Win32.GetForegroundWindow();
            IntPtr winHandle = focusedHandle; //TODO fix this. _input.GraphicsDevice.CurrentOutput.Handle;
            bool releaseInput = winHandle != focusedHandle;

            //only update properly if release-input is not active.
            if (releaseInput == false && _isConnected)
            {
                // Update states
                _state = _pad.GetState().Gamepad;
                _buttons = _state.Buttons.FromApi();
                _prevButtons = _statePrev.Buttons.FromApi();

                // Update thumbsticks and triggers
                _leftThumbstick.SetValues(_state.LeftThumbX, _state.LeftThumbY);
                _rightThumbstick.SetValues(_state.RightThumbX, _state.RightThumbY);
                _leftTrigger.SetValue(_state.LeftTrigger);
                _rightTrigger.SetValue(_state.RightTrigger);

                // Update hold timers
                foreach (int button in _heldTimers.Keys)
                    _heldTimers[button] += time.ElapsedTime.Milliseconds;
            }
            else
            {
                ClearState();
            }
        }

        private void SetVibration(float leftMotor, float rightMotor)
        {
            if (_isConnected == false)
                return;

            Vibration vib = new Vibration()
            {
                LeftMotorSpeed = (ushort)(65535f * leftMotor),
                RightMotorSpeed = (ushort)(65535f * rightMotor),
            };

            _pad.SetVibration(vib);
        }

        /// <summary>Gets the index of the game pad.</summary>
        public GamepadIndex PlayerIndex => (GamepadIndex)_padIndex;

        /// <summary>Gets the underlying state of the gamepad.</summary>
        public Gamepad State => _state;

        /// <summary>Gets the underlying state of the gamepad during the previous update.</summary>
        public Gamepad PreviousState => _statePrev;

        /// <summary>Gets extra capability information about the gamepad.</summary>
        public Capabilities CapabilityInfo => _capabilities;

        /// <summary>Gets or sets the vibration level of the left force-feedback motor.</summary>
        public float VibrationLeft
        {
            get { return _vibrationLeft; }
            set
            {
                _vibrationLeft = MathHelper.Clamp(value, 0, 1.0f);
                SetVibration(_vibrationLeft, _vibrationRight);
            }
        }

        /// <summary>Gets or sets the vibration level of the right force-feedback motor.</summary>
        public float VibrationRight
        {
            get { return _vibrationRight; }
            set
            {
                _vibrationRight = MathHelper.Clamp(value, 0, 1.0f);
                SetVibration(_vibrationLeft, _vibrationRight);
            }
        }

        /// <summary>
        /// Gets the X and Y axis values of the left thumbstick, or null if it doesn't have one.
        /// </summary>
        public IGamepadStick LeftThumbstick => _leftThumbstick;

        /// <summary>
        /// Gets the X and Y axis values of the right thumbstick, or null if it doesn't have one.
        /// </summary>
        public IGamepadStick RightThumbstick => _rightThumbstick;

        /// <summary>
        /// Gets the gamepad's left trigger, or null if it doesn't have one.
        /// </summary>
        public IGamepadTrigger LeftTrigger => _leftTrigger;

        /// <summary>
        /// Gets the gamepad's right trigger, or null if it doesn't have one.
        /// </summary>
        public IGamepadTrigger RightTrigger => _rightTrigger;

        /// <summary>Gets whether or not the gamepad is connected.</summary>
        public override bool IsConnected => _isConnected;

        /// <summary>
        /// Gets the gamepad/controller sub-type. For example, a joystick or steering wheel.
        /// </summary>
        public GamepadSubType SubType => _capabilities.SubType.FromApi();

        /// <summary>Gets the name of the gamepad.</summary>
        public override string DeviceName => _deviceName;

        /// <summary>
        /// Gets a flags value containing all of the currently-pressed buttons.
        /// </summary>
        public GamepadButtonFlags PressedButtons => _buttons;
    }
}
