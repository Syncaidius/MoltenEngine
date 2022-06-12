using Molten.Graphics;
using Molten.Windows32;
using Silk.NET.Core.Contexts;
using Silk.NET.XInput;

namespace Molten.Input
{
    public class WinGamepadDevice : GamepadDevice
    {
        const uint ERROR_SUCCESS = 0;
        const uint ERROR_DEVICE_NOT_CONNECTED = 0x48F;
        const uint XINPUT_FLAG_GAMEPAD = 1;
        const byte BATTERY_DEVTYPE_GAMEPAD = 0;
        const byte BATTERY_DEVTYPE_HEADSET = 1;
        const string XINPUT_LIBRARY_WIN10 = "xinput1_4.dll";

        static GamepadButtons[] _buttons;
        static XInput _api;

        Capabilities _capabilities;
        GamepadButtons _prevButtons;
        GamepadSubType _subType;

        string _deviceName;
        uint _lastPacketNumber;

        static WinGamepadDevice()
        {
            DefaultNativeContext dnc = new DefaultNativeContext(XINPUT_LIBRARY_WIN10);
            _api = new XInput(dnc);
            _buttons = ReflectionHelper.GetEnumValues<GamepadButtons>();
        }

        protected unsafe override List<InputDeviceFeature> OnInitialize(InputService service)
        {
            List<InputDeviceFeature> baseFeatures = base.OnInitialize(service);

            // Initialize hold timer dictionaries.
            _deviceName = "Gamepad " + Index;

            State initState;
            IsConnected = _api.GetState((uint)Index, &initState) == ERROR_SUCCESS;

            LeftStick = new InputAnalogStick("Left", 32767);
            RightStick = new InputAnalogStick("Right", 32767);
            LeftTrigger = new InputAnalogTrigger("Left", 255);
            RightTrigger = new InputAnalogTrigger("Right", 255);
            VibrationLeft = new InputVibration("Left", 1.0f);
            VibrationRight = new InputVibration("Right", 1.0f);

            // Only get state and capabilities if connected.
            if (IsConnected)
                RetrieveDeviceInformation();

            List<InputDeviceFeature> features = new List<InputDeviceFeature>()
            {
                LeftStick, RightStick,
                LeftTrigger, RightTrigger,
                VibrationLeft, VibrationRight
            };

            if (baseFeatures != null)
                features.AddRange(baseFeatures);

            return features;
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

        private void RetrieveDeviceInformation()
        {
            IsConnected = _api.GetCapabilities((uint)Index, XINPUT_FLAG_GAMEPAD, ref _capabilities) == ERROR_SUCCESS;

            // Add the sub-type into the name if device is not a normal gamepad.
            _subType = (GamepadSubType)_capabilities.SubType;

            if (_subType != GamepadSubType.Gamepad)
                _deviceName += $" ({_subType})";
        }

        /// <summary>Returns details about the status of a battery.</summary>
        /// <returns></returns>
        internal unsafe BatteryInformation GetBatteryDetails()
        {
            // TODO abstract this with https://github.com/Syncaidius/MoltenEngine/issues/41
            // TODO add support for retrieving headset (BATTERY_DEVTYPE_HEADSET) battery status

            BatteryInformation info;
            _api.GetBatteryInformation((uint)Index, BATTERY_DEVTYPE_GAMEPAD, &info);
            return info;
        }

        public override void OpenControlPanel() { }

        /// <summary>Update input handler.</summary>
        /// <param name="time">The snapshot of time.</param>
        /// <param name="releaseInput">If set to true, will reset all held timers and stop retrieving the latest state.</param>
        protected unsafe override void OnUpdate(Timing time)
        {
            State state;

            IsConnected = _api.GetState((uint)Index, &state) == ERROR_SUCCESS;

            if (!IsEnabled)
                return;

            // TODO test against all windows, not just the current release input if window is not focused.
            IntPtr focusedHandle = Win32.GetForegroundWindow();
            IntPtr winHandle = focusedHandle; //TODO fix this. _input.GraphicsDevice.CurrentOutput.Handle;
            bool releaseInput = winHandle != focusedHandle;

            // Only update properly if release-input is not active.
            if (releaseInput == false && IsConnected)
            {
                // Update states
                if (state.DwPacketNumber != _lastPacketNumber)
                {
                    Gamepad gp = state.Gamepad;
                    GamepadButtons buttons = (GamepadButtons)gp.WButtons;

                    // Check each available button.
                    foreach (GamepadButtons b in _buttons)
                    {
                        if (b == GamepadButtons.None)
                            continue;

                        GamepadButtonState gps = new GamepadButtonState()
                        {
                            Button = TranslateButton(b),
                            Pressure = 1.0f,
                            SetID = 0, // A standard gamepad only has one set of buttons.
                        };

                        bool pressed = buttons.HasFlag(b);
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
                    _lastPacketNumber = state.DwPacketNumber;

                    // Update thumbsticks, triggers and vibration
                    LeftStick.SetValues(gp.SThumbLX, gp.SThumbLY);
                    RightStick.SetValues(gp.SThumbRX, gp.SThumbRY);
                    LeftTrigger.SetValue(gp.BLeftTrigger);
                    RightTrigger.SetValue(gp.BRightTrigger);
                    SetVibration(VibrationLeft.Value, VibrationRight.Value);
                }
            }
            else
            {
                ClearState();
            }
        }

        private GamepadButtons TranslateButton(GamepadButtons b)
        {
            return b; // No translation needed. GamepadButtons matches XInput mapping.
        }

        private unsafe void SetVibration(float leftMotor, float rightMotor)
        {
            if (IsConnected == false)
                return;

            Vibration vib = new Vibration()
            {
                WLeftMotorSpeed = (ushort)(65535f * leftMotor),
                WRightMotorSpeed = (ushort)(65535f * rightMotor),
            };

            IsConnected = _api.SetState((uint)Index, &vib) == ERROR_SUCCESS;
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
        public override GamepadSubType SubType => _subType;

        /// <summary>Gets the name of the gamepad.</summary>
        public override string DeviceName => _deviceName;
    }
}
