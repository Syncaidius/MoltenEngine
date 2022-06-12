using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UIElementTheme
    {
        /// <summary>
        /// Gets or sets the default colors used when representing a a normal, unmodified state of functionality
        /// </summary>
        [DataMember]
        public UIStateTheme Default { get; set; } = new UIStateTheme();

        /// <summary>
        /// Gets or sets the colors used when represending a hover action. e.g. mouse or touch-hold gesture.
        /// </summary>
        [DataMember]
        public UIStateTheme Hover { get; set; } = new UIStateTheme();

        /// <summary>
        /// Gets or sets the colors used when representing a click, press or touch interaction.
        /// </summary>
        [DataMember]
        public UIStateTheme Pressed { get; set; } = new UIStateTheme();

        /// <summary>
        /// Gets or sets the colors used by elements to represent a disabled state of functionality.
        /// </summary>
        [DataMember]
        public UIStateTheme Disabled { get; set; } = new UIStateTheme();

        /// <summary>
        /// Gets or sets the colors used when representing active or selected functionality.
        /// </summary>
        [DataMember]
        public UIStateTheme Active { get; set; } = new UIStateTheme();

        /// <summary>
        /// Gets or sets the default font path, or name of a system font.
        /// </summary>
        [DataMember]
        public string FontName { get; set; } = "Arial";

        public void RequestFont(Engine engine, ContentRequestHandler loadCallback)
        {
            ContentRequest cr = engine.Content.BeginRequest("");
            cr.Load<TextFont>(FontName);
            cr.OnCompleted += loadCallback;
            cr.Commit();
        }
    }
}
