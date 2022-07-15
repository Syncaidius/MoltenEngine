using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Molten
{
    public class ColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken obj = JToken.Load(reader);
            string strValue = obj.ToObject<string>();
            string[] parts = strValue.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            Color col = new Color();
            for(int i = 0; i < parts.Length; i++)
            {
                if (byte.TryParse(parts[i], out byte bVal))
                    col[i] = bVal;
            }

            // Set alpha if not present.
            if (parts.Length < 4)
                col.A = 255;

            return col;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => true;
    }
}
