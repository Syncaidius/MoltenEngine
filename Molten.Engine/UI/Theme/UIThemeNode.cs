using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UITheme2 : UIStyle
    {
        const char PATH_DELIMITER = '/';

        public void Initialize(Engine engine)
        {
            Engine = engine;
        }

        /// <summary>
        /// Adds a theme style path to the current <see cref="UITheme"/>.
        /// </summary>
        /// <param name="stylePath">The path of the theme style.</param>
        /// <param name="populateWithDefault">If true, the created style will be populated with the default values of its <see cref="UIElement"/> type.</param>
        /// <returns>The created <see cref="UIStyle"/>.</returns>
        public UIStyle AddStyle(string stylePath, bool populateWithDefault = true)
        {
            string[] typeNames = stylePath.Split(PATH_DELIMITER, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Iterate backwards. Paths are parsed in reverse order - end-most child first.
            UIStyle node = this;
            Type startType = null;

            for (int i = typeNames.Length - 1; i >= 0; i--)
            {
                Type t = Type.GetType(typeNames[i]);
                if(t == null)
                {
                    Engine.Log.Error($"Type '{typeNames[i]}' not found for theme path of '{stylePath}'");
                    continue;
                }

                startType = startType ?? t;

                if (!node.Parents.TryGetValue(t, out UIStyle parent))
                {
                    parent = new UIStyle();
                    node.Parents.Add(t, parent);
                }

                node = parent;
                if (i == 0 && populateWithDefault)
                    node.PopulateProperties(startType);
            }

            return node;
        }

        /// <summary>
        /// Applies the theme to the given <see cref="UIElement"/>.
        /// </summary>
        /// <param name="element"></param>
        public void ApplyTheme(UIElement element)
        {
            UIStyle chosenNode = null;
            UIStyle node = this;
            UIElement e = element;

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
        internal Dictionary<MemberInfo, UIThemeValue> Properties { get; } = new Dictionary<MemberInfo, UIThemeValue>();

        internal Dictionary<Type, UIStyle> Parents { get; } = new Dictionary<Type, UIStyle>();

        internal void PopulateProperties(Type t)
        {
            MemberInfo[] members = t.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object objInstance = Activator.CreateInstance(t);

            foreach (MemberInfo info in members)
            {
                UIThemeMemberAttribute att = info.GetCustomAttribute<UIThemeMemberAttribute>(true);

                if (att != null)
                {
                    object defaultVal = null;
                    if (info is FieldInfo fInfo)
                        defaultVal = fInfo.GetValue(objInstance);
                    else if (info is PropertyInfo pInfo)
                        defaultVal = pInfo.GetValue(objInstance);

                    Properties[info] = new UIThemeValue(defaultVal);
                }
            }
        }
    }

    internal class UIThemeValue
    {
        public Dictionary<UIElementState, object> _values = new Dictionary<UIElementState, object>();

        internal UIThemeValue(object defaultValue)
        {
            _values[UIElementState.Default] = defaultValue;
        }

        public object this[UIElementState state]
        {
            get
            {
                if (state != UIElementState.Default & _values.TryGetValue(state, out object val))
                    return val;
                else
                    return _values[UIElementState.Default];
            }

            set => _values[state] = value;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class UIThemeMemberAttribute : Attribute
    {

    }
}
