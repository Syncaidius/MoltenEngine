using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using Newtonsoft.Json;

namespace Molten.UI
{
    public class UITheme : UIStyle
    {
        const char PATH_DELIMITER = '/';
        Dictionary<Type, MemberInfo[]> _memberCache;
        string _defaultFontName;
        bool _isInitialized;
        float _defaultFontSize = 16;

        public TextFont DefaultFont { get; private set; }

        public Engine Engine { get; private set; }

        [JsonProperty]
        public float DefaultFontSize
        {
            get => _defaultFontSize;
            set
            {
                if (_defaultFontSize != value)
                {
                    _defaultFontSize = value;
                    if (DefaultFont != null)
                        DefaultFont.Size = _defaultFontSize;
                }
            }
        }

        [JsonProperty]
        public string DefaultFontName
        {
            get => _defaultFontName;
            set
            {
                if (_defaultFontName != value)
                {
                    _defaultFontName = value;

                    if(_isInitialized)
                        LoadFont();
                }
            }
        }

        public UITheme() : base(null)
        {
            _memberCache = new Dictionary<Type, MemberInfo[]>();
        }

        internal void Clear()
        {
            _memberCache.Clear();
            DefaultFont = null;
            Properties.Clear();
            Parents.Clear();
        }

        private void Content_OnCompleted(ContentRequest request)
        {
            TextFontSource src = request.Get<TextFontSource>(0);
            if (src != null)
                DefaultFont = new TextFont(src, DefaultFontSize);
        }


        internal void Initialize(Engine engine)
        {
            if (_isInitialized)
                return;

            Engine = engine;
            LoadFont();
            _isInitialized = true;
        }

        private void LoadFont()
        {
            if (!string.IsNullOrWhiteSpace(_defaultFontName))
            {
                ContentRequest cr = Engine.Current.Content.BeginRequest();
                cr.Load<TextFontSource>(_defaultFontName);
                cr.OnCompleted += Content_OnCompleted;
                cr.CommitImmediate();
            }
        }

        /// <summary>
        /// Adds a theme style path to the current <see cref="UITheme"/>. 
        /// <para>
        /// If <paramref name="values"/> is null or empty, default values will be taken from the UI element type referred to in <paramref name="stylePath"/>.
        /// </para>
        /// </summary>
        /// <param name="stylePath">The path of the theme style.</param>
        /// <param name="values">A set of values to populate the style with. Useful when deserializing <see cref="UITheme"/> values.</param>
        /// <returns>The created <see cref="UIStyle"/>.</returns>
        public UIStyle AddStyle(string stylePath, Dictionary<string, Dictionary<UIElementState, object>> values = null)
        {
            string[] typeNames = stylePath.Split(PATH_DELIMITER, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Iterate backwards. Paths are parsed in reverse order - end-most child first.
            UIStyle node = this;
            Type startType = null;

            for (int i = typeNames.Length - 1; i >= 0; i--)
            {
                Type t = Type.GetType(typeNames[i]);
                if (t == null)
                {
                    Engine.Current.Log.Error($"Type '{typeNames[i]}' not found for theme path of '{stylePath}'");
                    continue;
                }

                startType = startType ?? t;

                if (!node.Parents.TryGetValue(t, out UIStyle parent))
                {
                    parent = new UIStyle(node);
                    node.Parents.Add(t, parent);
                }

                node = parent;
                if (i == 0)
                {
                    MemberInfo[] members = GetMembers(startType);
                    node.Populate(startType, members, values);
                }
            }

            return node;
        }

        public MemberInfo[] GetMembers(Type type)
        {
            if (!typeof(UIElement).IsAssignableFrom(type))
                throw new Exception($"The specified type is not derived from UIElement");

            if (!_memberCache.TryGetValue(type, out MemberInfo[] members))
            {
                BindingFlags bFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                members = type.GetMembers(bFlags);
                members = members.Where(m =>
                {
                    UIThemeMemberAttribute att = m.GetCustomAttribute<UIThemeMemberAttribute>(true);
                    return att != null;
                }).ToArray();

                _memberCache.Add(type, members);
            }

            return members;
        }

        /// <summary>
        /// Applies the theme to the given <see cref="UIElement"/>.
        /// </summary>
        /// <param name="element"></param>
        public void ApplyStyle(UIElement element)
        {
            UIStyle style = null;
            UIElement e = element;
            Type elementType = element.GetType();

            while (e != null)
            {
                Type eType = e.GetType();
                if (Parents.TryGetValue(eType, out UIStyle nextStyle))
                    break;

                //A more precise styling is available.
                e = e.Parent;
                style = nextStyle;
            }

            // No style found, not even a default one. Lets make a default.
            if (style == null)
                style = AddStyle(elementType.FullName);

            // Apply the style
            MemberInfo[] members = _memberCache[elementType];
            foreach (MemberInfo member in members)
            {
                object val = style.GetValue(member, element.State);
                if (member is FieldInfo field)
                    field.SetValue(element, val);
                else if (member is PropertyInfo property)
                    property.SetValue(element, val);
            }
        }
    }
}
