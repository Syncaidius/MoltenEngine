using Molten.Collections;
using Molten.Graphics;
using Molten.Input;
using Newtonsoft.Json;
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
            Input = new InputSettings();
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

        [DataMember]
        public InputSettings Input { get; private set; }

        /// <summary>Gets or sets the settings file name.</summary>
        public string Filename { get; set; } = "settings.json";

        /// <summary>Gets or sets the number of content worker threads.</summary>
        public int ContentWorkerThreads { get; set; } = 1;

        /// <summary>
        /// Gets a list of <see cref="ContentProcessor"/> instances that will be added to every new instantiation of <see cref="ContentManager"/>.
        /// </summary>
        public List<ContentProcessor> CustomContentProcessors { get; private set; } = new List<ContentProcessor>();

        /// <summary>
        /// Gets a list of <see cref="JsonConverter"/> instances that will be to added every new instantiation of <see cref="ContentManager"/>.
        /// </summary>
        public List<JsonConverter> JsonConverters { get; private set; } = new List<JsonConverter>()
        {
            new MathConverter(),
            new UIJsonConverter(),
        };

        /// <summary>Gets or sets the product name.</summary>
        public string ProductName { get; set; } = "Stone Bolt Game";

        /// <summary>Gets or sets the default font.</summary>
        public string DefaultFontName { get; set; } = "Arial";

        /// <summary>Gets or sets the default font size.</summary>
        public int DefaultFontSize { get; set; } = 16;
    }
}
