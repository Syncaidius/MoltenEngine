using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Molten.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class UIJsonConverter : JsonConverter
    {
        Type _uiType;

        public UIJsonConverter() {
            _uiType = typeof(UIComponent);
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType.IsSubclassOf(_uiType) && !objectType.IsAbstract);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            UIComponent com = null;

            try
            {
                JObject jo = JObject.Load(reader);
                com = ReadUIComponent(jo, serializer);
            }
            catch (Exception e)
            {
                //Engine.Current.Log.WriteError(e.Message, reader.Path);
            }

            return com;
        }

        private UIComponent ReadUIComponent(JObject o, JsonSerializer serializer)
        {
            JToken tType = o["$type"];

            if (tType == null)
                throw new MissingFieldException("The $type property is missing from UI component: " + o.ToString());

            string typeName = tType.Value<string>();
            Type t = Type.GetType(typeName);
            UIComponent com = Activator.CreateInstance(t, Engine.Current.UI) as UIComponent;

            JToken tProperties = o["Children"];
            o.Remove("Children");

            JsonReader joReader = o.CreateReader();

            serializer.Populate(joReader, com);
            joReader.Close();

            // Parse children
            JEnumerable<JToken> children = tProperties.Children();
            foreach (JObject c in children)
            {
                UIComponent child = ReadUIComponent(c, serializer);
                com.AddChild(child);
            }
            return com;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JProperty typeProperty = new JProperty("$type", ReflectionHelper.GetQualifiedTypeName(value.GetType()));
            JObject jo = new JObject();
            jo.Add(typeProperty);

            foreach (PropertyInfo prop in value.GetType().GetProperties())
            {
                if (prop.CanRead)
                {
                    if (prop.GetCustomAttribute<DataMemberAttribute>() == null)
                        continue;

                    object propValue = prop.GetValue(value);
                    if (propValue != null)
                    {
                        jo.Add(prop.Name, JToken.FromObject(propValue, serializer));
                    }
                }
            }

            jo.WriteTo(writer);
        }

        //public override bool CanWrite
        //{
        //    get { return false; }
        //}
    }
}
