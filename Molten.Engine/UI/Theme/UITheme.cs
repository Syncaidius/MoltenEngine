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

        [IgnoreDataMember]
        public Engine Engine { get; private set; }

        string _contentRoot;


        public UIElementTheme GetTheme<T>()
            where T : UIElement
        {
            return GetTheme(typeof(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="themeRootDirectory">The content root directory for the current theme, or null if default.</param>
        internal void Initialize(Engine engine, string themeRootDirectory = null)
        {
            Engine = engine;
            _contentRoot = themeRootDirectory;

            DefaultElementTheme.LoadContent(engine);
            foreach (UIElementTheme eTheme in _themes.Values)
                eTheme.LoadContent(Engine, themeRootDirectory);
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
            {
                throw new Exception($"A theme is already set for {tName}");
            }
            else
            {
                theme.LoadContent(Engine, _contentRoot);
                _themes[tName] = theme;
            }
        }        
    }
}
