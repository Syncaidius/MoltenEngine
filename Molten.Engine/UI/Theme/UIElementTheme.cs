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
        public event ObjectHandler<UIElementTheme> OnContentLoaded;

        [DataMember]
        Dictionary<UIElementState, UIStateTheme> _states = new Dictionary<UIElementState, UIStateTheme>()
        {
            [UIElementState.Default] = new UIStateTheme(),
            [UIElementState.Pressed] = new UIStateTheme(),
            [UIElementState.Active] = new UIStateTheme(),
            [UIElementState.Hovered] = new UIStateTheme(),
            [UIElementState.Disabled] = new UIStateTheme()
        };

        /// <summary>
        /// Gets or sets the default font path, or name of a system font.
        /// </summary>
        [DataMember]
        public string FontName { get; set; } = "Arial";

        [IgnoreDataMember]
        public TextFont Font { get; private set; }

        internal void LoadContent(Engine engine, string rootDirectory = null)
        {
            if (Font != null && Font.Source.Name == FontName)
                return;

            ContentRequest cr = engine.Content.BeginRequest(rootDirectory);
            cr.Load<TextFont>(FontName);
            cr.OnCompleted += LoadContent_Request;
            cr.Commit();
        }

        private void LoadContent_Request(ContentRequest cr)
        {
            Font = cr.Get<TextFont>(0);
            OnContentLoaded?.Invoke(this);
        }

        /// <summary>
        /// Gets the <see cref="UIStateTheme"/> for a particular <see cref="UIElementState"/>.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public UIStateTheme this[UIElementState state]
        {
            get => _states[state];
        }
    }
}
