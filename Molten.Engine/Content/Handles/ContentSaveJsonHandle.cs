using Newtonsoft.Json;

namespace Molten
{
    public class ContentSaveJsonHandle : ContentSaveHandle
    {
        internal ContentSaveJsonHandle(
            ContentManager manager,
            string path,
            object asset,
            JsonSerializerSettings jsonSettings,
            Action<FileInfo> completionCallback) :
            base(manager, path, asset, null, null, completionCallback)
        {
            JsonSettings = jsonSettings.Clone();
        }

        protected override ContentHandleStatus OnProcess()
        {
            string json = JsonConvert.SerializeObject(Asset, JsonSettings);

            if (!Info.Directory.Exists)
                Info.Directory.Create();

            using (Stream stream = new FileStream(RelativePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                    writer.Write(json);
            }

            return ContentHandleStatus.Completed;
        }

        /// <summary>Gets the <see cref="JsonSerializerSettings"/> used for saving the asset. 
        /// <para>These are a copy of the original settings, so any changes will only affect the current <see cref="ContentLoadJsonHandle{T}"/></para></summary>.
        public JsonSerializerSettings JsonSettings { get; }
    }
}
