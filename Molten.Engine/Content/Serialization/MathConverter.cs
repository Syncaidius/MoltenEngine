using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Molten
{
    public class MathConverter : JsonConverter
    {
        Type _valueType = typeof(ValueType);

        public override bool CanConvert(Type objectType)
        {
            return (objectType.Assembly.GetName().Name == "Molten.Math") && _valueType.IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("MathConverter cannot read because CanRead is false.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Type t = value.GetType();

            FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            JObject jo = new JObject();
            JProperty p = null;

            for (int i = 0; i < fields.Length; i++)
            {
                object v = fields[i].GetValue(value);

                p = new JProperty(fields[i].Name, v);
                jo.Add(p);
            }

            jo.WriteTo(writer);
        }

        public override bool CanRead
        {
            get { return false; }
        }
    }
}
