using System.Collections.Concurrent;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Molten
{
    /// <summary>
    /// A JsonConverter for all Molten.Math value types.
    /// </summary>
    public class ValueTypeConverter : JsonConverter
    {
        Type _valueType = typeof(ValueType);
        ConcurrentDictionary<Type, FieldInfo[]> _fieldCache = new ConcurrentDictionary<Type, FieldInfo[]>();

        public override bool CanConvert(Type objectType)
        {
            if (objectType.IsEnum || !objectType.IsValueType)
                return false;

            return (objectType.Assembly.GetName().Name.StartsWith("Molten")) && _valueType.IsAssignableFrom(objectType);
        }

        private FieldInfo[] GetCachedFields(Type objectType)
        {
            if (!_fieldCache.TryGetValue(objectType, out FieldInfo[] fields))
            {
                fields = objectType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                _fieldCache.TryAdd(objectType, fields);
            }

            return fields;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken obj = JToken.Load(reader);
            string strValue = obj.ToObject<string>();
            string[] parts = strValue.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            FieldInfo[] fields = GetCachedFields(objectType);

            if (parts.Length < fields.Length)
                throw new Exception($"Expected {fields.Length} values for type '{objectType.FullName}' but got {parts.Length} values.");

            existingValue = existingValue ?? Activator.CreateInstance(objectType);
            for (int i = 0; i < fields.Length; i++)
            {
                JValue valObj = new JValue(parts[i]);
                object pVal = valObj.ToObject(fields[i].FieldType);
                fields[i].SetValue(existingValue, pVal);
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Type t = value.GetType();

            FieldInfo[] fields = GetCachedFields(t);

            string strValue = "";
            for (int i = 0; i < fields.Length; i++)
                strValue += $"{fields[i].GetValue(value)} ";

            strValue = strValue.TrimEnd();
            JValue jValue = new JValue(strValue);
            jValue.WriteTo(writer);
        }

        public override bool CanRead => true;
    }
}
