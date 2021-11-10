using System;

namespace Molten.UI
{
    public class UIException : Exception
    {
        public UIException(UIComponent component, string message) : base(message)
        {
            Component = component;
        }

        /// <summary>
        /// Gets the component which threw or caused the exception.
        /// </summary>
        public UIComponent Component { get; private set; }
    }
}
