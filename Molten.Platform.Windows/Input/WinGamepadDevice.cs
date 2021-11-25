using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.XInput;
using Molten.Graphics;
using State = SharpDX.XInput.State;
using Molten.Windows32;

namespace Molten.Input
{
    public class WinGamepadDevice : GamepadDevice
    {
        static GamepadButtonFlags[] _buttons;

        Controller _pad;
        Capabilities _capabilities;
        GamepadButtonFlags _prevButtons;

        string _deviceName;
        int _lastPacketNumber;

        static WinGamepadDevice()
        {
            _buttons = ReflectionHelper.EnumToArray<GamepadButtonFlags>();
        }

        internal WinGamepadDevice(WinInputManager manager, int index) : 
            base(manager, index)
        {
        }

        protected override int GetMaxSimultaneousStates()
        {
            return (int)GamepadButton.Y + 1;
        }

        protected override List<InputDeviceFeature> Initialize()
        {
            // Initialize hold timer dictionaries.
            _deviceName = "Gamepad " + Index;
            _pad = new Controller((UserIndex)Index);
            IsConnected = _pad.IsConnected;

            LeftStick = new InputAnalogStick("Left", 32767);
            RightStick = new InputAnalogStick("Right", 32767);
            LeftTrigger = new InputAnalogTrigger("Left", 255);
            RightTrigger = new InputAnalogTrigger("Right", 255);
            VibrationLeft = new InputVibration("Left", 1.0f);
            VibrationRight = new InputVibration("Right", 1.0f);

            // Only get state and capabilities if connected.
            if (IsConnected)
                RetrieveDeviceInformation();

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
            SetVibration(0, 0);
        }

        protected override void OnDispose() { }

        private void RetrieveDeviceInformation()
        {
            _capabilities = _pad.GetCapabilities(DeviceQueryType.Gamepad);

            // Add the sub-type into the name if device is not a normal gamepad.
            if (_capabilities.SubType != DeviceSubType.Gamepad)
                _deviceName += " ( " + _capabilities.SubType + ")";
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

            // TODO test against all windows, not just the current release input if window is not focused.
            IntPtr focusedHandle = Win32.GetForegroundWindow();
            IntPtr winHandle = focusedHandle; //TODO fix this. _input.GraphicsDevice.CurrentOutput.Handle;
            bool releaseInput = winHandle != focusedHandle;

            // Only update properly if release-input is not active.
            if (releaseInput == false && IsConnected)
            {
                // Update states
                State state = _pad.GetState();
                if (state.PacketNumber != _lastPacketNumber)
                {
                    Gamepad gp = state.Gamepad;
                    GamepadButtonFlags buttons = gp.Buttons;

                    // Check each available button.
                    foreach (GamepadButtonFlags b in _buttons)
                    {
                        if (b == GamepadButtonFlags.None)
                            continue;

                        GamepadButtonState gps = new GamepadButtonState()
                        {
                            Button = TranslateButton(b),
                            Pressure = 1.0f,
                        };

                        bool pressed = gp.Buttons.HasFlag(b);
                        bool wasPressed = _prevButtons.HasFlag(b);

                        if (pressed && !wasPressed)
                        {
                            gps.PressTimestamp = DateTime.UtcNow;
                            gps.Action = InputAction.Pressed;
                            QueueState(gps);
                        }
                        else if (pressed && wasPressed)
                        {
                            gps.Action = InputAction.Held;
                            QueueState(gps);
                        }
                        else if (!pressed && wasPressed)
                        {
                            gps.Action = InputAction.Released;
                            QueueState(gps);
                        }
                    }

                    _prevButtons = buttons;
                    _lastPacketNumber = state.PacketNumber;

                    // Update thumbsticks, triggers and vibration
                    LeftStick.SetValues(gp.LeftThumbX, gp.LeftThumbY);
                    RightStick.SetValues(gp.RightThumbX, gp.RightThumbY);
                    LeftTrigger.SetValue(gp.LeftTrigger);
                    RightTrigger.SetValue(gp.RightTrigger);
                    SetVibration(VibrationLeft.Value, VibrationRight.Value);
                }
            }
            else
            {
                ClearState();
            }
        }

        private GamepadButton TranslateButton(GamepadButtonFlags b)
        {
            switch (b)
            {
                default:
                case GamepadButtonFlags.None: return GamepadButton.None;
                case GamepadButtonFlags.A: return GamepadButton.A;
                case GamepadButtonFlags.B: return GamepadButton.B;
                case GamepadButtonFlags.Back: return GamepadButton.Back;
                case GamepadButtonFlags.DPadDown: return GamepadButton.DPadDown;
                case GamepadButtonFlags.DPadLeft: return GamepadButton.DPadLeft;
                case GamepadButtonFlags.DPadRight: return GamepadButton.DPadRight;
                case GamepadButtonFlags.DPadUp: return GamepadButton.DPadUp;
                case GamepadButtonFlags.LeftShoulder: return GamepadButton.LeftShoulder;
                case GamepadButtonFlags.LeftThumb: return GamepadButton.LeftThumb;
                case GamepadButtonFlags.RightShoulder: return GamepadButton.RightShoulder;
                case GamepadButtonFlags.RightThumb: return GamepadButton.RightThumb;
                case GamepadButtonFlags.Start: return GamepadButton.Start;
                case GamepadButtonFlags.X: return GamepadButton.X;
                case GamepadButtonFlags.Y: return GamepadButton.Y;
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
        public override string DeviceName => _deviceName;
    }
}
