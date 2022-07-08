using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using Newtonsoft.Json;

namespace Molten.UI
{
    public class UIElementTheme
    {
        [JsonProperty]
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
        [JsonProperty]
        public string FontName { get; set; } = "Arial";

        [JsonIgnore]
        public TextFont Font { get; private set; }

        string _requestedFontName;

        internal void OnRequestContent(ContentRequest cr)
        {
            _requestedFontName = FontName;

            // Don't request the same font we already have loaded, if any.
            if (Font != null && Font.Source.Name == _requestedFontName)
                return;

            cr.Load<TextFont>(_requestedFontName);
        }

        internal void OnContentLoaded(ContentRequest cr)
        {
            Font = cr.Get<TextFont>(_requestedFontName);
        }

        /// <summary>
        /// Sets all of the <see cref="UIElementTheme"/> state themes to the provided <see cref="UIStateTheme"/>.
        /// </summary>
        /// <param name="stateTheme"></param>
        public void Set(UIStateTheme stateTheme)
        {
            foreach (UIStateTheme theme in _states.Values)
                theme.Set(stateTheme);
        }

        /// <summary>
        /// Gets the <see cref="UIStateTheme"/> for the given <see cref="UIElementState"/>. 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public UIStateTheme this[UIElementState state]
        {
            get => _states[state];
        }
    }
}
