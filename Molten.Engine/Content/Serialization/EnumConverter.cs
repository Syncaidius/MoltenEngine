using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Reflection;

namespace Molten
{
    /// <summary>
    /// A JsonConverter for all <see cref="Enum"/> types.
    /// </summary>
    public class EnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken obj = JToken.Load(reader);
            string strValue = obj.ToObject<string>();

            if(!Enum.TryParse(objectType, strValue, true, out object result))
            {
                Array acceptedValues = Enum.GetValues(objectType);
                string errorValues = "";
                for (int i = 0; i < acceptedValues.Length; i++)
                    errorValues += $"{acceptedValues.GetValue(i)}{(i != acceptedValues.Length - 1 ? ", " :"")}";

                throw new Exception($"Invalid enum value '{strValue}'. Accepted values are: {errorValues}.");
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Type t = value.GetType();

            JValue jValue = new JValue(value.ToString());
            jValue.WriteTo(writer);
        }

        public override bool CanRead
        {
            get { return true; }
        }
    }
}
