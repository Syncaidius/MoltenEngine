using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Molten
{
    public abstract class ContentLoadJsonHandle<T> : ContentLoadHandle<T>
    {
        internal ContentLoadJsonHandle(
            ContentManager manager,
            Action<T> completionCallback, 
            JsonSerializerSettings jsonSettings,
            bool canHotReload = true) :
            base(manager, null, null, completionCallback, canHotReload)
        {
            JsonSettings = jsonSettings.Clone();
        }

        protected override bool OnProcess()
        {
            using (Stream stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                string json;
                using (StreamReader reader = new StreamReader(stream))
                    json = reader.ReadToEnd();

                bool reload = Asset != null;
                string strLoad = reload ? "RELOAD" : "LOAD";

                if (reload)
                    JsonConvert.PopulateObject(json, Asset);
                else
                    Asset = JsonConvert.DeserializeObject(json, ContentType, JsonSettings);

                return true;
            }
        }

        /// <summary>Gets the <see cref="JsonSerializerSettings"/> used for loading the asset. 
        /// <para>These are a copy of the original settings, so any changes will only affect the current <see cref="ContentLoadJsonHandle{T}"/></para></summary>.
        public JsonSerializerSettings JsonSettings { get; }
    }
}
