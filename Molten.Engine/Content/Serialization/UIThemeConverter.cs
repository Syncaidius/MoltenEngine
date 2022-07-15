using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Molten
{
    internal class UIThemeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UITheme);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            UITheme theme = existingValue as UITheme ?? new UITheme();

            theme.Clear();

            JObject jObject = JObject.Load(reader);
            Dictionary<string, JToken> entries = new Dictionary<string, JToken>();
            IEnumerable<JProperty> children = jObject.Properties();

            foreach(JProperty child in children)
            {
                entries.Add(child.Name, child);
            }

            return theme;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
