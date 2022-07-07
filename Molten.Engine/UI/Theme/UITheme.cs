using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UITheme
    {
        public event ObjectHandler<UITheme> OnContentLoaded;

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
            LoadContent(engine);
        }

        /// <summary>
        /// (Re)loads the content of the current <see cref="UITheme"/>.
        /// </summary>
        /// <param name="engine">The engine instance to use when loading content.</param>
        public void LoadContent(Engine engine)
        {
            IsLoaded = false;
            ContentRequest cr = engine.Content.BeginRequest(_contentRoot);

            DefaultElementTheme.OnRequestContent(cr);
            foreach (UIElementTheme eTheme in _themes.Values)
                eTheme.OnRequestContent(cr);

            cr.OnCompleted += ContentRequest_OnCompleted;
            cr.Commit();
        }

        private void ContentRequest_OnCompleted(ContentRequest request)
        {
            DefaultElementTheme.OnContentLoaded(request);
            foreach (UIElementTheme eTheme in _themes.Values)
                eTheme.OnContentLoaded(request);

            OnContentLoaded?.Invoke(this);
            IsLoaded = true;
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

        /// <summary>
        /// Gets whether or not the theme's content has finished loading.
        /// </summary>
        public bool IsLoaded { get; private set; }
    }
}
