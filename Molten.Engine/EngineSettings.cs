using Molten.Collections;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    [DataContract]
    public class EngineSettings : SettingBank
    {
        public EngineSettings()
        {
            Graphics = new GraphicsSettings();
        }

        public void Load()
        {
            if (!File.Exists(Filename))
                return;

            string json = "";

            using (FileStream stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    json = reader.ReadToEnd();
                }
            }

            FromJson(json);
        }

        public void Save()
        {
            string json = ToJson();

            using (FileStream stream = new FileStream(Filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(json);
                }
            }
        }

        protected override void OnDispose() { }

        /// <summary>Gets the graphics settings bank.</summary>
        [DataMember]
        public GraphicsSettings Graphics { get; private set; }

        /// <summary>Gets or sets the settings file name.</summary>
        public string Filename { get; set; } = "settings.json";

        public string ContentRootDirectory { get; set; } = "/Assets/";

        public int ContentWorkerThreads { get; set; } = 1;

        public string ProductName { get; set; } = "Stone Bolt Game";

        public string DefaultFontName { get; set; } = "Arial";

        public int DefaultFontSize { get; set; } = 16;
    }
}
