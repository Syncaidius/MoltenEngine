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
        /// <summary>
        /// A lookup table of available parent elements for the element-type represented by the current <see cref="UIStyle"/>.
        /// </summary>
        internal Dictionary<Type, UIStyle> Parents { get; } = new Dictionary<Type, UIStyle>();

        internal UIStyle Child { get; }

        internal Dictionary<MemberInfo, UIStyleValue> Properties { get; } = new Dictionary<MemberInfo, UIStyleValue>();

        internal Dictionary<string, MemberInfo> PropertiesByName { get; } = new Dictionary<string, MemberInfo>();

        internal UIStyle(UIStyle child)
        {
            Child = child;
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
                    PropertiesByName[mInfo.Name] = mInfo;
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
