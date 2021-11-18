using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.XInput;
using Molten.Graphics;
using Molten.Utilities;
using State = SharpDX.XInput.State;

namespace Molten.Input
{
    public class WinGamepadDevice : GamepadDevice
    {
        Gamepad _state;
        Gamepad _statePrev;
        Controller _pad;
        Capabilities _capabilities;
        GamepadButtonFlags _buttons;
        GamepadButtonFlags _prevButtons;

        Dictionary<int, int> _heldTimers; // keeps track of how long buttons are held for.

        public WinGamepadDevice(WinInputManager manager, GamepadIndex index, Logger log) : base(manager, index, log)
        {
            _pad = new Controller((UserIndex)Index);
            _state = new Gamepad();
        }

        protected override List<InputDeviceFeature> Initialize()
        {
            // Initialize hold timer dictionaries.
            _heldTimers = new Dictionary<int, int>();
            DeviceName = "Gamepad " + Index;
            IsConnected = _pad.IsConnected;

            LeftStick = new InputAnalogStick("Left", 32767);
            RightStick = new InputAnalogStick("Right", 32767);
            LeftTrigger = new InputAnalogTrigger("Left", 255);
            RightTrigger = new InputAnalogTrigger("Right", 255);

            // Only get state and capabilities if connected.
            if (IsConnected)
                RetrieveDeviceInformation();

            // Initialize previous state
            _statePrev = new Gamepad();

            return new List<InputDeviceFeature>()
            {
                LeftStick, RightStick, 
                LeftTrigger, RightTrigger,
                VibrationLeft, VibrationRight
            };
        }

        protected override void OnBind(INativeSurface surface)
        {
            // TODO simply store the window we're bound to and only accept input if it is focused.
        }

        protected override void OnUnbind(INativeSurface surface)
        {
            // TODO simply store the window we're bound to and only accept input if it is focused.
        }
        protected override void OnClearState()
        {
            foreach (int button in _heldTimers.Keys)
                _heldTimers[button] = 0;

            _buttons = GamepadButtonFlags.None;
            _prevButtons = GamepadButtonFlags.None;
            SetVibration(0, 0);
        }

        protected override void OnDispose() { }

        private void RetrieveDeviceInformation()
        {
            State padState = _pad.GetState();
            _state = padState.Gamepad;
            _capabilities = _pad.GetCapabilities(DeviceQueryType.Gamepad);

            // Add the sub-type into the name if device is not a normal gamepad.
            if (_capabilities.SubType != DeviceSubType.Gamepad)
                DeviceName += " ( " + _capabilities.SubType + ")";
        }

        /// <summary>Returns true if the button was only just pressed. Returns false if it was pressed in the last update too.</summary>
        /// <param name="playerIndex">The controller index to read from.</param>
        /// <param name="value">The button(s) to check.</param>
        /// <param name="repeatCheck">Set to true if the button must not have been pressed in the previous update.</param>
        /// <returns>Returns true if the button is considered pressed.</returns>
        public override bool IsDown(GamepadButtonFlags value)
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
        protected override void OnUpdate(Timing time)
        {
            IsConnected = _pad.IsConnected;     

            // Store previous states
            for (int gp = 0; gp < 4; gp++)
                _statePrev = _state;

            // TODO test against all windows, not just the current release input if window is not focused.
            IntPtr focusedHandle = Win32.GetForegroundWindow();
            IntPtr winHandle = focusedHandle; //TODO fix this. _input.GraphicsDevice.CurrentOutput.Handle;
            bool releaseInput = winHandle != focusedHandle;

            // Only update properly if release-input is not active.
            if (releaseInput == false && IsConnected)
            {
                // Update states
                _state = _pad.GetState().Gamepad;
                _buttons = _state.Buttons.FromApi();
                _prevButtons = _statePrev.Buttons.FromApi();

                // Update thumbsticks, triggers and vibration
                LeftStick.SetValues(_state.LeftThumbX, _state.LeftThumbY);
                RightStick.SetValues(_state.RightThumbX, _state.RightThumbY);
                LeftTrigger.SetValue(_state.LeftTrigger);
                RightTrigger.SetValue(_state.RightTrigger);
                SetVibration(VibrationLeft.Value, VibrationRight.Value);

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
            if (IsConnected == false)
                return;

            Vibration vib = new Vibration()
            {
                LeftMotorSpeed = (ushort)(65535f * leftMotor),
                RightMotorSpeed = (ushort)(65535f * rightMotor),
            };

            _pad.SetVibration(vib);
        }

        /// <summary>Gets the underlying state of the gamepad.</summary>
        public Gamepad State => _state;

        /// <summary>Gets the underlying state of the gamepad during the previous update.</summary>
        public Gamepad PreviousState => _statePrev;

        /// <summary>Gets extra capability information about the gamepad.</summary>
        public Capabilities CapabilityInfo => _capabilities;

        /// <summary>Gets or sets the vibration level of the left force-feedback motor.</summary>
        public override InputVibration VibrationLeft { get; protected set; }

        /// <summary>Gets or sets the vibration level of the right force-feedback motor.</summary>
        public override InputVibration VibrationRight { get; protected set; }

        /// <summary>
        /// Gets the X and Y axis values of the left thumbstick, or null if it doesn't have one.
        /// </summary>
        public override InputAnalogStick LeftStick { get; protected set; }

        /// <summary>
        /// Gets the X and Y axis values of the right thumbstick, or null if it doesn't have one.
        /// </summary>
        public override InputAnalogStick RightStick { get; protected set; }

        /// <summary>
        /// Gets the gamepad's left trigger, or null if it doesn't have one.
        /// </summary>
        public override InputAnalogTrigger LeftTrigger { get; protected set; }

        /// <summary>
        /// Gets the gamepad's right trigger, or null if it doesn't have one.
        /// </summary>
        public override InputAnalogTrigger RightTrigger { get; protected set; }

        /// <summary>
        /// Gets the gamepad/controller sub-type. For example, a joystick or steering wheel.
        /// </summary>
        public override GamepadSubType SubType => _capabilities.SubType.FromApi();

        /// <summary>Gets the name of the gamepad.</summary>
        public override string DeviceName { get; protected set; }

        /// <summary>
        /// Gets a flags value containing all of the currently-pressed buttons.
        /// </summary>
        public override GamepadButtonFlags PressedButtons => _buttons;
    }
}
