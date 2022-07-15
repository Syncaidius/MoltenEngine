using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            IEnumerable<JProperty> entries = jObject.Properties();

            foreach(JProperty entry in entries)
            {
                UIStyle style = theme.AddStyle(entry.Name);

                Dictionary<string, MemberInfo> infoByName = new Dictionary<string, MemberInfo>();
                foreach (MemberInfo m in style.Properties.Keys)
                    infoByName.Add(m.Name.ToLower(), m);

                if (entry.HasValues)
                {
                    if (entry.Value.Type == JTokenType.Object)
                    {
                        JObject styleObj = entry.Value as JObject;
                        IEnumerable<JProperty> styleProperties = styleObj.Properties();
                        foreach(JProperty p in styleProperties)
                        {
                            switch (p.Type)
                            {
                                case JTokenType.Object:
                                    // We have multiple style values for this property. e.g. Default, Pressed, etc
                                    break;

                                case JTokenType.Array:
                                    // TODO The style values were provided as an array.
                                    // We'll interpret them in the order matching the UIElementState enum values. e.g. Default, Pressed, Disabled, Hover, etc
                                    break;

                                default:
                                    // TODO Assume we've been given a single value. Try to directly serialize it into the style member's 'Default' value.
                                    break;
                            }
                            if (p.Type == JTokenType.Object)
                            {
                                
                            }
                        }
                    }
                    else
                    {
                        // Invalid value, each theme style entry should be a Json Object full of relevant key-values.
                    }
                }
            }

            return theme;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
