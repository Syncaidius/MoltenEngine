using Molten.Input;
using System;

namespace Molten
{
    internal class SceneClickTracker
    {
        internal ICursorAcceptor PressedObject = null;
        internal Vector2F DragDistance;
        internal bool InputDragged = false;
        internal float DragThreshold = 10; // Pixels
        internal MouseButton Button;

        public SceneClickTracker(MouseButton button)
        {
            Button = button;
        }

        internal void Update(SceneManager handler, IMouseDevice mouse, Timing time)
        {
            Vector2F mousePos = mouse.Position;
            Vector2F mouseMove = mouse.Delta;

            //handle clicking and dragging.
            if (mouse.IsDown(Button))
            {
                //check if we're starting a new click 
                if (PressedObject == null)
                {
                    //store the component as being dragged
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
                        PressedObject.InvokeCursorClickStarted(mousePos, Button);
                    }

                    InputDragged = false;
                    DragDistance = new Vector2F();

                }
                else
                {
                    //update drag checks
                    DragDistance += mouseMove;

                    float distDragged = Math.Abs(DragDistance.Length());
                    if (distDragged >= DragThreshold)
                    {
                        InputDragged = true;
                        PressedObject.InvokeCursorDrag(mousePos, mouseMove, Button);
                    }
                }
            }
            else
            {
                //check if the tap was released outside or inside of the component
                if (PressedObject != null)
                {
                    if (PressedObject.Contains(mousePos) == true)
                        PressedObject.InvokeCursorClickCompleted(mousePos, InputDragged, Button);
                    else
                        PressedObject.InvokeCursorClickCompletedOutside(mousePos, Button);

                    PressedObject = null;
                }
            }
        }
    }
}

