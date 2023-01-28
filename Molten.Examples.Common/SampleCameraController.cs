using Molten.Input;

namespace Molten.Examples
{
    public class SampleCameraController : SceneComponent, 
        IInputReceiver<MouseDevice>, 
        IInputReceiver<KeyboardDevice>, 
        IInputReceiver<GamepadDevice>
    {
        float _rotateSpeed = 0.25f;
        float _moveSpeed = 0.5f;

        public void HandleInput(MouseDevice mouse, Timing timing)
        {
            if (!AcceptInput)
                return;            

            if (mouse.IsDown(PointerButton.Left))
            {
                Vector2F axisDelta = new Vector2F(mouse.Delta.Y, mouse.Delta.X);
                axisDelta *= _rotateSpeed;
                axisDelta *= timing.Delta;

                Object.Transform.LocalRotationX += axisDelta.X;
                Object.Transform.LocalRotationY += axisDelta.Y;
            }
        }

        public void HandleInput(KeyboardDevice kb, Timing timing)
        {
            Vector3F moveDelta = Vector3F.Zero;
            if (kb.IsDown(KeyCode.W)) moveDelta += Object.Transform.Global.Backward;
            if (kb.IsDown(KeyCode.S)) moveDelta += Object.Transform.Global.Forward;
            if (kb.IsDown(KeyCode.A)) moveDelta += Object.Transform.Global.Left;
            if (kb.IsDown(KeyCode.D)) moveDelta += Object.Transform.Global.Right;
            Object.Transform.LocalPosition += (moveDelta * _moveSpeed) * timing.Delta;
        }

        public void HandleInput(GamepadDevice gamepad, Timing timing)
        {
            // Gamepad movement
            Vector2F rightAxis = new Vector2F(gamepad.RightStick.Y, gamepad.RightStick.X);
            rightAxis *= _rotateSpeed * 4;
            rightAxis *= timing.Delta;

            Object.Transform.LocalRotationX += rightAxis.X;
            Object.Transform.LocalRotationY += rightAxis.Y;

            Vector3F moveDelta = Vector3F.Zero;
            if (gamepad.IsDown(GamepadButtons.DPadUp)) moveDelta += Object.Transform.Global.Backward ;
            if (gamepad.IsDown(GamepadButtons.DPadDown)) moveDelta += Object.Transform.Global.Forward ;
            if (gamepad.IsDown(GamepadButtons.DPadLeft)) moveDelta += Object.Transform.Global.Left;
            if (gamepad.IsDown(GamepadButtons.DPadRight)) moveDelta += Object.Transform.Global.Right;

            Vector2F leftAxis = new Vector2F(gamepad.LeftStick.X, gamepad.LeftStick.Y);
            leftAxis *= _rotateSpeed * 3;
            leftAxis *= timing.Delta;

            moveDelta += Object.Transform.Global.Right * leftAxis.X;
            moveDelta += Object.Transform.Global.Forward * -leftAxis.Y;

            Object.Transform.LocalPosition += (moveDelta * _moveSpeed) * timing.Delta;
        }

        public bool AcceptInput { get; set; } = true;

        protected override void OnDispose() { }

        public void InitializeInput(MouseDevice device, Timing timing) { }

        public void DeinitializeInput(MouseDevice device, Timing timing) { }

        public void InitializeInput(KeyboardDevice device, Timing timing) { }

        public void DeinitializeInput(KeyboardDevice device, Timing timing) { }

        public void InitializeInput(GamepadDevice device, Timing timing) { }

        public void DeinitializeInput(GamepadDevice device, Timing timing) { }

        public bool IsFirstInput(MouseDevice device) { return false; }

        public bool IsFirstInput(KeyboardDevice device) { return false; }

        public bool IsFirstInput(GamepadDevice device) { return false; }
    }
}
