using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
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

            if (values == null || values.Count == 0)
            {
                foreach (MemberInfo mInfo in members)
                {
                    object defaultVal = GetMemberValue(mInfo, objInstance);
                    Properties[mInfo] = new UIStyleValue(this, defaultVal);
                }
            }
            else
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
}
