using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Molten.UI
{
    public class UITheme2 : UIStyle
    {
        const char PATH_DELIMITER = '/';

        public UITheme2() : base(null)
        {
            Engine = Engine.Current;
        }

        /// <summary>
        /// Adds a theme style path to the current <see cref="UITheme"/>.
        /// </summary>
        /// <param name="stylePath">The path of the theme style.</param>
        /// <param name="populateWithDefault">If true, the created style will be populated with the default values of its <see cref="UIElement"/> type.</param>
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
                    Engine.Log.Error($"Type '{typeNames[i]}' not found for theme path of '{stylePath}'");
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
                    node.Populate(startType, values);
            }

            return node;
        }

        /// <summary>
        /// Applies the theme to the given <see cref="UIElement"/>.
        /// </summary>
        /// <param name="element"></param>
        public void ApplyStyle(UIElement element)
        {
            UIStyle chosenNode = null;
            UIStyle node = this;
            UIElement e = element;

            // Get the the closest match we can find. For example:
            // We want "UIWindow/UIButton/UIText" but only "UIButton/UIText" is available, the latter will be returned.
            // Remember styles are stored backwards - inner-most style first. This is for performance reasons.
            //
            // When searching for a style we'd actually look for UIText -> UIButton -> UIWindow
            // and get then hopefully get a style stored for UIText in the UIWindow style node
            while(e != null)
            {
                Type t = e.GetType();

                if (!node.Parents.TryGetValue(t, out node))
                    break;

                e = element.Parent;
                chosenNode = node;
            }

            // No theme found, generate default for it.
            if(chosenNode == null)
            {
                Type et = element.GetType();
                chosenNode = AddStyle(et.FullName);
            }
        }

        /// <summary>
        /// Gets the engine that the current <see cref="UITheme2"/> is bound to.
        /// </summary>
        public Engine Engine { get; private set; }
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

        internal void Populate(Type t, Dictionary<string, Dictionary<UIElementState, object>> values)
        {
            object objInstance = Activator.CreateInstance(t);
            BindingFlags bFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            if (values != null)
            {
                MemberTypes mTypes = MemberTypes.Field | MemberTypes.Property;

                foreach (string memberName in values.Keys)
                {
                    Dictionary<UIElementState, object> stateValues = values[memberName];

                    MemberInfo[] members = t.GetMember(memberName, mTypes, bFlags);
                    if(members.Length > 0)
                    {
                        MemberInfo mInfo = members[0]; // We only care about the first one.
                        object defaultVal = GetMemberValue(mInfo, objInstance);

                        UIStyleValue styleVal = new UIStyleValue(defaultVal);
                        Properties[mInfo] = styleVal;

                        foreach (UIElementState state in stateValues.Keys)
                            styleVal[state] = stateValues[state];
                    }
                }
            }
            else
            {
                MemberInfo[] members = t.GetMembers(bFlags);

                foreach (MemberInfo mInfo in members)
                {
                    UIThemeMemberAttribute att = mInfo.GetCustomAttribute<UIThemeMemberAttribute>(true);

                    if (att != null)
                    {
                        object defaultVal = GetMemberValue(mInfo, objInstance);
                        Properties[mInfo] = new UIStyleValue(defaultVal);
                    }
                }
            }
        }

        private object GetMemberValue(MemberInfo info, object obj)
        {
            if (info is FieldInfo fInfo)
                return fInfo.GetValue(obj);
            else if (info is PropertyInfo pInfo)
                return pInfo.GetValue(obj);
            else
                return null;
        }
    }

    public class UIStyleValue
    {
        internal Dictionary<UIElementState, object> Values = new Dictionary<UIElementState, object>();

        internal UIStyle Style { get; }

        internal UIStyleValue(object defaultValue)
        {
            Values[UIElementState.Default] = defaultValue;
        }

        internal void PopulateWith(UIStyleValue value)
        {

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
