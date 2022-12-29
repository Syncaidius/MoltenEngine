using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;

namespace Molten
{
    /// <summary>
    /// A JsonConverter for <see cref="SettingValue{T}"/> and <see cref="SettingValueList{T}"/> objects.
    /// </summary>
    public class SettingValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(SettingValue).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            existingValue = existingValue ?? Activator.CreateInstance(objectType);

            JToken obj = JToken.Load(reader);
            Type settingType = objectType.GenericTypeArguments[0];

            // If it's the old type of settings file/json, grab the value out of the "Value(s)" property
            if (typeof(SettingValueList<>).MakeGenericType(settingType) == objectType)
            {
                PropertyInfo valueProperty = objectType.GetProperty("Values");
                object settingVal = valueProperty.GetValue(existingValue);
                IList list = settingVal as IList;

                // Old style with 'Values' property?
                if (obj.Type == JTokenType.Object)
                {
                    existingValue = obj.ToObject(objectType);
                }
                else if (obj.Type == JTokenType.Array)
                {
                    existingValue = obj.ToObject(settingType);
                }
                else // Remaining possibility is a list of values/strings
                {
                    string strValue = obj.ToObject<string>();
                    string[] values = strValue.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    for (int i = 0; i < values.Length; i++)
                    {
                        JValue v = new JValue(values[i]);
                        list.Add(v.ToObject(settingType));
                    }
                }
            }
            else
            {

                if (obj.Type == JTokenType.Object)
                {
                    existingValue = obj.ToObject(objectType);
                }
                else // Old style with 'Value' property?
                {
                    string strValue = obj.ToObject<string>();
                    JValue jValue = new JValue(strValue);
                    PropertyInfo pInfo = objectType.GetProperty("Value");

                    if (IsSingleMemberType(settingType, out Type valType, out MemberInfo singleMember))
                    {
                        object singleValue = jValue.ToObject(valType);
                        object val = Activator.CreateInstance(settingType);

                        if (singleMember is PropertyInfo singleProperty)
                            singleProperty.SetValue(val, singleValue);
                        else if (singleMember is FieldInfo singleField)
                            singleField.SetValue(val, singleValue);

                        pInfo.SetValue(existingValue, val);
                    }
                    else
                    {
                        object val = jValue.ToObject(settingType);
                        pInfo.SetValue(existingValue, val);
                    }
                }
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Type t = value.GetType();
            Type settingType = t.GenericTypeArguments[0];

            if (typeof(SettingValueList<>).MakeGenericType(settingType) == t)
            {
                PropertyInfo pInfo = t.GetProperty("Values");
                object settingValue = pInfo.GetValue(value);
                if(settingValue is IList list)
                {
                    if (settingType.IsPrimitive|| settingType.IsEnum || settingType == typeof(string))
                    {
                        string jsonValue = "";
                        for (int i = 0; i < list.Count; i++)
                            jsonValue += $"{list[i]}{(i < list.Count - 1 ? ", " : "")}";

                        JValue val = new JValue(jsonValue);
                        val.WriteTo(writer);
                    }
                    else
                    {
                        JArray valObject = JArray.FromObject(list);
                        valObject.WriteTo(writer);
                    }
                }
            }
            else
            {
                PropertyInfo pInfo = t.GetProperty("Value");
                object settingValue = pInfo.GetValue(value);
                SerializeSettingValue(writer, settingType, settingValue, serializer);
            }
        }

        private void SerializeSettingValue(JsonWriter writer, Type type, object value, JsonSerializer serializer)
        {
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            {
                JToken val = JToken.FromObject(value ?? string.Empty, serializer);
                val.WriteTo(writer);
            }
            else
            {
                if(IsSingleMemberType(type, out Type memberValueType, out MemberInfo singleMember))
                {
                    if (singleMember is PropertyInfo pMember)
                        value = pMember.GetValue(value);
                    else if (singleMember is FieldInfo fMember)
                        value = fMember.GetValue(value);

                    if (value != null)
                        SerializeSettingValue(writer, memberValueType, value, serializer);
                }
                else
                {
                    JObject valObject = JObject.FromObject(value, serializer);
                    valObject.WriteTo(writer);
                }
            }
        }

        private bool IsSingleMemberType(Type type, out Type memberValueType, out MemberInfo member)
        {
            memberValueType = null;
            member = null;

            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            IEnumerable<MemberInfo> members = type.GetMembers(bindingFlags).Where((member) =>
            {
                if (member.MemberType == MemberTypes.Field ||
                    member.MemberType == MemberTypes.Property)
                {
                    DataMemberAttribute attData = member.GetCustomAttribute<DataMemberAttribute>();
                    JsonPropertyAttribute attJson = member.GetCustomAttribute<JsonPropertyAttribute>();
                    return attData != null || attJson != null;
                }

                return false;
            });

            if (members.Count() == 1)
            {
                member = members.First();
                if (member is PropertyInfo pMember)
                    memberValueType = pMember.PropertyType;
                else if (member is FieldInfo fMember)
                    memberValueType = fMember.FieldType;

                return true;
            }

            return false;
        }

        public override bool CanRead
        {
            get { return true; }
        }
    }
}
