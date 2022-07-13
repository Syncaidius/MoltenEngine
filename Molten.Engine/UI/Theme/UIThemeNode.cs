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
    public class UITheme2 : UIStyle
    {
        const char PATH_DELIMITER = '/';
        Dictionary<Type, MemberInfo[]> _memberCache;
        string _defaultFontName;
        float _defaultFontSize = 16;

        public TextFont DefaultFont { get; private set; }

        public Engine Engine { get; private set; }

        [JsonProperty]
        public float DefaultFontSize
        {
            get => _defaultFontSize;
            set
            {
                if(_defaultFontSize != value)
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
                if(_defaultFontName != value)
                {
                    _defaultFontName = value;
                    if (!string.IsNullOrWhiteSpace(_defaultFontName))
                    {
                        ContentRequest cr = Engine.Current.Content.BeginRequest();
                        cr.Load<TextFontSource>(_defaultFontName);
                        cr.OnCompleted += Content_OnCompleted;
                    }
                }
            }
        }

        private void Content_OnCompleted(ContentRequest request)
        {
            TextFontSource src = request.Get<TextFontSource>(0);
            if (src != null)
                DefaultFont = new TextFont(src, DefaultFontSize);
        }

        public UITheme2() : base(null)
        {
            _memberCache = new Dictionary<Type, MemberInfo[]>();
            Engine = Engine.Current;
        }

        /// <summary>
        /// Adds a theme style path to the current <see cref="UITheme"/>. 
        /// <para>
        /// If <paramref name="values"/> is null or empty, default values will be taken from the UI element type referred to in <paramref name="stylePath"/>.
        /// </para>
        /// </summary>
        /// <param name="stylePath">The path of the theme style.</param>
        /// <param name="values">A set of values to populate the style with. Useful when deserializing <see cref="UITheme2"/> values.</param>
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
                    if (!_memberCache.TryGetValue(startType, out MemberInfo[] members))
                    {                            
                        BindingFlags bFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                        members = startType.GetMembers(bFlags);
                        members = members.Where(m =>
                        {
                            UIThemeMemberAttribute att = m.GetCustomAttribute<UIThemeMemberAttribute>(true);
                            return att != null;
                        }).ToArray();

                        _memberCache.Add(startType, members);
                    }

                    node.Populate(startType, members, values);
                }
            }

            return node;
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

            while(e != null)
            {
                Type eType = e.GetType();
                if (Parents.TryGetValue(eType, out UIStyle nextStyle))
                    break;

                //A more precise styling is available.
                e = e.Parent;
                style = nextStyle;
            }

            // No style found, not even a default one. Lets make a default.
            if(style == null)
                style = AddStyle(elementType.FullName);

            // Apply the style
            MemberInfo[] members = _memberCache[elementType];
            foreach(MemberInfo member in members)
            {
                object val = style.GetValue(member, element.State);
                if (member is FieldInfo field)
                    field.SetValue(element, val);
                else if (member is PropertyInfo property)
                    property.SetValue(element, val);
            }
        }
    }

    public class UIStyle
    {
        internal Dictionary<MemberInfo, UIStyleValue> Properties { get; } = new Dictionary<MemberInfo, UIStyleValue>();

        internal Dictionary<Type, UIStyle> Parents { get; } = new Dictionary<Type, UIStyle>();

        internal UIStyle Child { get; }

        internal UIStyle(UIStyle child)
        {
            Child = child;
        }

        internal object GetValue(MemberInfo member, UIElementState state)
        {
            // First try to find the value on self or child styles
            UIStyle style = this;
            while (style != null)
            {
                if (Properties.TryGetValue(member, out UIStyleValue value))
                    return value[state];

                style = style.Child;
            }

            return null;
        }

        internal void Populate(Type t, MemberInfo[] members, Dictionary<string, Dictionary<UIElementState, object>> values)
        {
            object objInstance = Activator.CreateInstance(t);

            if (values != null || values.Count == 0)
            {
                foreach (string memberName in values.Keys)
                {
                    Dictionary<UIElementState, object> stateValues = values[memberName];

                    foreach (MemberInfo member in members)
                    {
                        if (member.Name == memberName)
                        {
                            object defaultVal = GetMemberValue(member, objInstance);

                            UIStyleValue styleVal = new UIStyleValue(this, defaultVal);
                            Properties[member] = styleVal;

                            foreach (UIElementState state in stateValues.Keys)
                                styleVal[state] = stateValues[state];

                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (MemberInfo mInfo in members)
                {
                    object defaultVal = GetMemberValue(mInfo, objInstance);
                    Properties[mInfo] = new UIStyleValue(this, defaultVal);
                }
            }
        }

        private object GetMemberValue(MemberInfo member, object obj)
        {
            if (member is FieldInfo fInfo)
                return fInfo.GetValue(obj);
            else if (member is PropertyInfo pInfo)
                return pInfo.GetValue(obj);
            else
                return null;
        }
    }

    public class UIStyleValue
    {
        internal Dictionary<UIElementState, object> Values = new Dictionary<UIElementState, object>();

        /// <summary>
        /// Gets the parent style which owns the current <see cref="UIStyleValue"/>.
        /// </summary>
        internal UIStyle Style { get; }

        internal UIStyleValue(UIStyle parentStyle, object defaultValue)
        {
            Style = parentStyle;
            Values[UIElementState.Default] = defaultValue;
        }

        public object this[UIElementState state]
        {
            get
            {
                if (state != UIElementState.Default & Values.TryGetValue(state, out object val))
                    return val;
                else
                    return Values[UIElementState.Default];
            }

            set => Values[state] = value;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class UIThemeMemberAttribute : Attribute
    {

    }
}
