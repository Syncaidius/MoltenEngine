using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.UI;
using Newtonsoft.Json;

namespace Molten
{
    public class ContentLoadJsonHandle<T> : ContentLoadHandle<T>
    {
        internal ContentLoadJsonHandle(
            ContentManager manager,
            string path,
            ContentLoadCallbackHandler<T> completionCallback, 
            JsonSerializerSettings jsonSettings,
            bool canHotReload) :
            base(manager, path, null, null, completionCallback, canHotReload)
        {
            JsonSettings = jsonSettings.Clone();
        }

        protected override bool OnProcess()
        {
            using (Stream stream = new FileStream(RelativePath, FileMode.Open, FileAccess.Read))
            {
                string json;
                using (StreamReader reader = new StreamReader(stream))
                    json = reader.ReadToEnd();

                bool reload = Asset != null;

                if (reload)
                {
                    Type assetType = Asset.GetType();
                    object[] att = assetType.GetCustomAttributes(typeof(ContentReloadAttribute), true);
                    if(att.Length > 0)
                    {
                        if (att[0] is ContentReloadAttribute attReload && !attReload.Reinstantiate)
                        {
                            JsonConvert.PopulateObject(json, Asset, JsonSettings);
                            return true;
                        }
                    }
                }

                // If we have an existing asset that is to be re-instantiated, dispose of old instance.
                if (Asset != null && Asset is IDisposable disposable)
                    disposable.Dispose();

                Asset = JsonConvert.DeserializeObject(json, ContentType, JsonSettings);
                return true;
            }
        }

        /// <summary>Gets the <see cref="JsonSerializerSettings"/> used for loading the asset. 
        /// <para>These are a copy of the original settings, so any changes will only affect the current <see cref="ContentLoadJsonHandle{T}"/></para></summary>.
        public JsonSerializerSettings JsonSettings { get; }
    }
}
