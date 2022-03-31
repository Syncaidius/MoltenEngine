using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.Input
{
    /// <summary>
    /// A virtual device which emulates touch input using a <see cref="MouseDevice"/>.
    /// </summary>
    public class MouseTouchEmulatorDevice : TouchDevice
    {
        /// <summary>
        /// Gets or sets the underlying <see cref="MouseDevice"/>.
        /// </summary>
        public MouseDevice Mouse
        {
            get => _mouse;
            set
            {
                if(value != _mouse)
                {
                    // Unbind old mouse, if available.
                    if(_mouse != null)
                    {
                        _mouse.OnButtonDown -= QueueTouchState;
                        _mouse.OnButtonUp -= QueueTouchState;
                        _mouse.OnMoved -= QueueTouchState;
                        _mouse.OnHeld -= QueueTouchState;
                        _mouse.OnConnected -= MouseConnectionChanged;
                        _mouse.OnDisconnected -= MouseConnectionChanged;
                    }

                    // Bind to new mouse, if available.
                    _mouse = value;
                    if(_mouse != null)
                    {
                        _mouse.OnButtonDown += QueueTouchState;
                        _mouse.OnButtonUp += QueueTouchState;
                        _mouse.OnMoved += QueueTouchState;
                        _mouse.OnHeld += QueueTouchState;
                        _mouse.OnConnected += MouseConnectionChanged;
                        _mouse.OnDisconnected += MouseConnectionChanged;
                        IsConnected = Mouse.IsConnected;
                    }
                    else
                    {
                        IsConnected = false;
                    }
                }
            }
        }

        MouseDevice _mouse;

        protected override List<InputDeviceFeature> OnInitialize(InputService service)
        {
            List<InputDeviceFeature> baseFeatures =  base.OnInitialize(service);
            
            Service.Settings.Input.TouchBufferSize.OnChanged += TouchSampleBufferSize_OnChanged;

            return baseFeatures;
        }

        private void MouseConnectionChanged(InputDevice o)
        {
            IsConnected = Mouse.IsConnected;
        }

        private void QueueTouchState(MouseDevice mouse, in MouseButtonState state)
        {
            QueueState(new TouchPointState()
            {
                Action = state.Action,
                PressTimestamp = state.PressTimestamp,
                ID = 0,
                Delta = (Vector2F)state.Delta,
                Orientation = 0,
                Position = (Vector2F)state.Position,
                Pressure = 1,
                Size = 1
            });
        }

        public override int TouchPointCount { get; protected set; }

        public override string DeviceName => "Mouse Touch Emulator Device";

        public override void OpenControlPanel() { }

        protected override int GetMaxSimultaneousStates()
        {
            return 1;
        }

        private void TouchSampleBufferSize_OnChanged(int oldValue, int newValue)
        {
            BufferSize = newValue;
        }

        protected override void OnBind(INativeSurface surface) { }

        protected override void OnClearState()
        {
            Mouse.ClearState();
        }

        protected override void OnDispose() { }

        protected override void OnUnbind(INativeSurface surface) { }

        protected override void OnUpdate(Timing time) { }
    }
}
