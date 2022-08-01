using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.UI;
using Newtonsoft.Json;

namespace Molten
{
    public class ContentLoadJsonHandle : ContentLoadHandle
    {
        internal ContentLoadJsonHandle(
            ContentManager manager,
            string path,
            Type contentType,
            ContentLoadCallbackHandler<object> completionCallback, 
            JsonSerializerSettings jsonSettings,
            bool canHotReload) :
            base(manager, path, 1, contentType, null, null, completionCallback, canHotReload)
        {
            JsonSettings = jsonSettings.Clone();
        }

        protected override ContentHandleStatus OnRead(bool reinstantiate)
        {
            string json;
            using (FileStream stream = new FileStream(RelativePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                    json = reader.ReadToEnd();
            }

            if(!reinstantiate)
                JsonConvert.PopulateObject(json, Asset, JsonSettings);
            else
                Asset = JsonConvert.DeserializeObject(json, ContentType, JsonSettings);

            Complete();
            return ContentHandleStatus.Completed;
        }

        /// <summary>Gets the <see cref="JsonSerializerSettings"/> used for loading the asset. 
        /// <para>These are a copy of the original settings, so any changes will only affect the current <see cref="ContentLoadJsonHandle{T}"/></para></summary>.
        public JsonSerializerSettings JsonSettings { get; }
    }
}
