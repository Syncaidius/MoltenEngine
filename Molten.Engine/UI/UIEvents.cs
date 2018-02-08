using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Molten.IO;

namespace Molten.UI
{
    public delegate void UIComponentEventHandler<T>(UIEventData<T> data) where T : struct;
    public delegate void UIComponentHandler<T>(T component) where T : UIComponent;

    public struct UIEventData<T> where T : struct
    {
        public Vector2 Position;

        /// <summary>The movement delta. For the mouse scroll wheel, this is stored the Y axis.</summary>
        public Vector2 Delta;

        /// <summary>The value which describes the button or key that was pressed.</summary>
        public T InputValue;

        public UIComponent Component;

        public bool WasDragged;
    }
}
