using Molten.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    internal class UIClickTracker
    {
        internal UIComponent PressedComponent = null;
        internal Vector2 DragDistance;
        internal bool InputDragged = false;
        internal float DragThreshold = 10; // Pixels
        internal MouseButton Button;
        MouseHandler _lastHandler;

        public UIClickTracker(MouseButton button)
        {
            Button = button;
        }

        internal void Update(UISystem ui, Timing time)
        {
            // if the parent UI system's handlers were switched out, reset input here.
            if(_lastHandler != ui.Mouse)
            {
                DragDistance = Vector2.Zero;
                PressedComponent = null;
                _lastHandler = ui.Mouse;
            }

            Vector2 mousePos = ui.Mouse.Position;
            Vector2 mouseMove = ui.Mouse.Moved;

            //handle clicking and dragging.
            if (ui.Mouse.IsPressed(Button))
            {
                //check if we're starting a new click 
                if (PressedComponent == null)
                {
                    //store the component as being dragged
                    PressedComponent = ui.Hovered;

                    if (PressedComponent != null)
                    {
                        // Check if focused control needs unfocusing.
                        if (ui.Focused != PressedComponent && ui.Focused != null)
                        {
                            if (ui.Focused.Contains(mousePos) == false)
                                ui.Unfocus();
                        }

                        // Trigger press-start event
                        PressedComponent.InvokePressStarted(mousePos, Button);
                    }

                    InputDragged = false;
                    DragDistance = new Vector2();

                }
                else
                {
                    //update drag checks
                    DragDistance += mouseMove;

                    float distDragged = Math.Abs(DragDistance.Length());
                    if (distDragged >= DragThreshold)
                    {
                        InputDragged = true;
                        PressedComponent.InvokeDrag(mousePos, mouseMove, Button);
                    }
                }
            }
            else
            {
                //check if the tap was released outside or inside of the component
                if (PressedComponent != null)
                {
                    if (PressedComponent.Contains(mousePos) == true)
                        PressedComponent.InvokePressCompleted(mousePos, InputDragged, Button);
                    else
                        PressedComponent.InvokeCompletedOutside(mousePos, mouseMove, Button);

                    PressedComponent = null;
                }
            }
        }
    }
}
