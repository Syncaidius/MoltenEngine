using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Input;

namespace Molten.Examples
{
    public class SampleCameraController : SceneComponent, IInputHandler
    {
        public void HandleInput(MouseDevice mouse, TouchDevice touch, KeyboardDevice keyboard, GamepadDevice gamepad, Timing timing)
        {
            if (!AcceptInput)
                return;

            // Keyboard input - Again messy code for now
            float rotSpeed = 0.25f;
            float moveSpeed = 0.5f;

            Vector2F axisDelta = Vector2F.Zero;

            if (AcceptInput && mouse.IsDown(PointerButton.Left))
                axisDelta = new Vector2F(mouse.Delta.Y, mouse.Delta.X) * rotSpeed;

            // Gamepad movement
            axisDelta += new Vector2F(gamepad.RightStick.Y, gamepad.RightStick.X) * rotSpeed * 2f;

            Object.Transform.LocalRotationX += axisDelta.X;
            Object.Transform.LocalRotationY += axisDelta.Y;

            // Handle forward, backward, left and right movement. 
            // For now, we'll just add the keyboard and gamepad values together. In a real game, this isn't a good idea!
            Vector3F moveDelta = UpdateKeyboardMovement(keyboard, timing, moveSpeed);
            moveDelta += UpdateGamepadMovement(gamepad, timing, moveSpeed);

            Object.Transform.LocalPosition += moveDelta * timing.Delta * moveSpeed;
        }

        private Vector3F UpdateKeyboardMovement(KeyboardDevice keyboard, Timing time, float speed)
        {
            Vector3F moveDelta = Vector3F.Zero;
            if (keyboard.IsDown(KeyCode.W)) moveDelta += Object.Transform.Global.Backward * speed;
            if (keyboard.IsDown(KeyCode.S)) moveDelta += Object.Transform.Global.Forward * speed;
            if (keyboard.IsDown(KeyCode.A)) moveDelta += Object.Transform.Global.Left * speed;
            if (keyboard.IsDown(KeyCode.D)) moveDelta += Object.Transform.Global.Right * speed;

            return moveDelta;
        }

        private Vector3F UpdateGamepadMovement(GamepadDevice gamepad, Timing time, float speed)
        {
            Vector3F moveDelta = Vector3F.Zero;
            if (gamepad.IsDown(GamepadButtons.DPadUp)) moveDelta += Object.Transform.Global.Backward * speed;
            if (gamepad.IsDown(GamepadButtons.DPadDown)) moveDelta += Object.Transform.Global.Forward * speed;
            if (gamepad.IsDown(GamepadButtons.DPadLeft)) moveDelta += Object.Transform.Global.Left * speed;
            if (gamepad.IsDown(GamepadButtons.DPadRight)) moveDelta += Object.Transform.Global.Right * speed;

            moveDelta += new Vector3F(gamepad.LeftStick.X, 0, gamepad.LeftStick.Y) * speed * 2;
            return moveDelta;
        }

        public bool AcceptInput { get; set; } = true;

        protected override void OnDispose() { }
    }
}
