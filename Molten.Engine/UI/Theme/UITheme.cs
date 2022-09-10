using System.Reflection;
using Molten.Graphics;
using Newtonsoft.Json;

namespace Molten.UI
{
    [ContentReload(true)]
    public class UITheme : UIStyle
    {
        const char PATH_DELIMITER = '/';
        Dictionary<Type, MemberInfo[]> _memberCache;
        string _defaultFontName;
        bool _isInitialized;
        float _defaultFontSize = 16;

        public SpriteFont DefaultFont { get; private set; }

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

                    if (_isInitialized)
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
                Engine.Content.LoadFont(_defaultFontName, (font, isReload) => DefaultFont = font,
                new SpriteFontParameters()
                {
                    FontSize = DefaultFontSize
                });
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
            Type elementType = element.GetType();
            UIStyle style = FindStyle(element, elementType, this);

            // Apply the style
            if (style != null && style.Properties.Count > 0)
            {
                MemberInfo[] members = GetMembers(elementType);
                foreach (MemberInfo member in members)
                {
                    // Try to retrieve the member value from the current style, or lower-level/child/direct styles.
                    UIStyle memberStyle = style;
                    while (memberStyle != null)
                    {
                        if (memberStyle.Properties.TryGetValue(member, out UIStyleValue value))
                        {
                            SetMember(member, element, value[element.State]);
                        }
                        else if(memberStyle.PropertiesByName.TryGetValue(member.Name, out MemberInfo altMember))
                        {
                            if (memberStyle.Properties.TryGetValue(altMember, out value))
                                SetMember(member, element, value[element.State]);
                        }

                        memberStyle = memberStyle.Child;
                    }
                }
            }
        }

        private UIStyle FindStyle(UIElement element, Type elementType, UIStyle style)
        {
            // Find a more granular style. e.g. A style for a UIButton on a UIPanel, instead of just the generic/base UIButton style.
            while(typeof(UIElement).IsAssignableFrom(elementType))
            {
                if (style.Parents.TryGetValue(elementType, out UIStyle parentStyle))
                {
                    UIElement parent = element.ParentElement;
                    if (parent != null)
                        return FindStyle(parent, parent.GetType(), parentStyle);
                    else
                        return parentStyle;
                }

                // Move to base type of current element type
                elementType = elementType.BaseType;
            }

            // Return the last matching style.
            return style;
        }

        private void SetMember(MemberInfo member, object target, object value)
        {
            if (member is FieldInfo field)
            {
                field.SetValue(target, value);
            }
            else if (member is PropertyInfo property)
            {
                // Does the property have a setter?
                if (property.CanWrite)
                {
                    property.SetValue(target, value);
                }
                else // No... Now we need to try and set it by copying member values across instead...
                {                    
                    // ...But we can only do this for reference types, not on value-types of a get-only property.
                    if (property.PropertyType.IsValueType)
                        return;

                    // Get the target member's value
                    object targetMemberValue = null;
                    GetMemberValue(member, target, ref targetMemberValue);

                    // Get the members of the target's member value we just retrieved.
                    MemberInfo[] targetMembers = targetMemberValue.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    IEnumerable<MemberInfo> filteredMembers = targetMembers.Where(m =>
                    {
                        // TODO only retrieve members with the UIThemeMember, DataMember or JsonProperty attributes.
                        return (m is FieldInfo) || (m is PropertyInfo);
                    });

                    foreach (MemberInfo m in filteredMembers)
                    {
                        object valueMemberValue = null;
                        GetMemberValue(m, value, ref valueMemberValue);

                        // Now try to set the members of the target's member-value we just retrieved.
                        SetMember(m, targetMemberValue, valueMemberValue);
                    }
                }
            }
        }

        private void GetMemberValue(MemberInfo member, object obj, ref object value)
        {
            if (member is FieldInfo targetField)
                value = targetField.GetValue(obj);
            else if (member is PropertyInfo targetProperty)
                value = targetProperty.GetValue(obj);
        }
    }
}
