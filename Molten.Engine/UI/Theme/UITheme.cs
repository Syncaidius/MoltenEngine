using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UITheme
    {
        [DataMember]
        Dictionary<string, UIElementTheme> _themes = new Dictionary<string, UIElementTheme>();

        [DataMember]
        public UIElementTheme DefaultElementTheme { get; } = new UIElementTheme();

        /// <summary>
        /// Gets or sets the default font texture sheet size.
        /// </summary>
        [DataMember]
        public int FontTextureSize { get; set; } = 512;

        public UIElementTheme GetTheme<T>()
            where T : UIElement
        {
            return GetTheme(typeof(T));
        }

        public UIElementTheme GetTheme(Type elementType)
        {
            if (_themes.TryGetValue(elementType.FullName, out UIElementTheme theme))
                return theme;
            else
                return DefaultElementTheme;
        }

        public void SetTheme<T>(UIElementTheme theme)
            where T : UIElement
        {
            string tName = typeof(T).FullName;
            if (_themes.TryGetValue(tName, out theme))
                throw new Exception($"A theme is already set for {tName}");
            else
                _themes[tName] = theme;
        }        
    }
}
