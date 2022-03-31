using Molten.Input;

namespace Molten
{
    internal class SceneClickTracker
    {
        internal IInputAcceptor PressedObject = null;
        internal Vector2F DragDistance;
        internal bool InputDragged = false;
        internal float DragThreshold = 10; // Pixels
        public MouseButton Button { get; private set; }

        public SceneClickTracker(MouseButton button)
        {
            Button = button;
        }

        internal void Update(SceneManager handler, MouseDevice mouse, Timing time)
        {
            Vector2F mousePos = (Vector2F)mouse.Position;
            Vector2F mouseMove = (Vector2F)mouse.Delta;

            // Handle clicking and dragging.
            if (mouse.IsDown(Button))
            {
                // Check if we're starting a new click 
                if (PressedObject == null)
                {
                    // Store the component as being dragged
                    PressedObject = handler.Hovered;

                    if (PressedObject != null)
                    {
                        // Check if focused control needs unfocusing.
                        if (handler.Focused != PressedObject && handler.Focused != null)
                        {
                            if (handler.Focused.Contains(mousePos) == false)
                                handler.Unfocus();
                        }

                        // Trigger press-start event
                        PressedObject.CursorClickStarted(mousePos, Button);
                    }

                    InputDragged = false;
                    DragDistance = new Vector2F();
                }
                else
                {
                    // Update drag checks
                    DragDistance += mouseMove;

                    float distDragged = Math.Abs(DragDistance.Length());
                    if (distDragged >= DragThreshold)
                    {
                        InputDragged = true;
                        PressedObject.CursorDrag(mousePos, mouseMove, Button);
                    }
                }
            }
            else
            {
                // Check if the tap was released outside or inside of the component
                if (PressedObject != null)
                {
                    if (PressedObject.Contains(mousePos) == true)
                        PressedObject.CursorClickCompleted(mousePos, InputDragged, Button);
                    else
                        PressedObject.CursorClickCompletedOutside(mousePos, Button);

                    PressedObject = null;
                }
            }
        }
    }
}

