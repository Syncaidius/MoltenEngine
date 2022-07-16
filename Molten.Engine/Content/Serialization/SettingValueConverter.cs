using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

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

            // If it's the old type of settings file/json, grab the value out of the "Value(s)" probably
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
                // Old style with 'Value' property?
                if (obj.Type == JTokenType.Object)
                {
                    existingValue = obj.ToObject(objectType);
                }
                else
                {
                    string strValue = obj.ToObject<string>();
                    JValue jValue = new JValue(strValue);
                    PropertyInfo pInfo = objectType.GetProperty("Value");
                    object val = jValue.ToObject(pInfo.PropertyType);
                    pInfo.SetValue(existingValue, val);
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

                if (settingType.IsPrimitive || settingType.IsEnum || settingType == typeof(string))
                {
                    JToken val = JToken.FromObject(settingValue, serializer);
                    val.WriteTo(writer);
                }
                else
                {
                    JObject valObject = JObject.FromObject(settingValue, serializer);
                    valObject.WriteTo(writer);
                }
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }
    }
}
