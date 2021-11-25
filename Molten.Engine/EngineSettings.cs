using Molten.Graphics;
using Molten.Input;
using Molten.Network;
using Molten.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Molten
{
    [DataContract]
    public class EngineSettings : SettingBank
    {
        public EngineSettings()
        {
            Graphics = new GraphicsSettings();
            Input = new InputSettings();
            UI = new UISettings();
        }

        public void AddService<T, S>(MoltenEventHandler<EngineService<S>, S> initCallback) 
            where T : EngineService<S>, new()
            where S : SettingBank
        {
            // Check if service is already in startup list.
            Type t = typeof(T);
            foreach(EngineService s in StartupServices)
            {
                Type sType = s.GetType();
                if (sType.IsAssignableFrom(t) || t.IsAssignableFrom(sType))
                    throw new Exception($"Cannot add service of the same or derived type as another ({sType} and {t}).");
            }

            T service = new T();
            service.OnInitialized += initCallback;
            StartupServices.Add(service);
        }

        public void Load()
        {
            if (!File.Exists(Filename))
                return;

            string json = "";

            JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
            jsonSettings.Converters = JsonConverters;

            using (FileStream stream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    json = reader.ReadToEnd();
                }
            }

            JsonConvert.PopulateObject(json, this, jsonSettings);
        }

        public void Save()
        {
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
            jsonSettings.Converters = JsonConverters;
            string json = JsonConvert.SerializeObject(this, jsonSettings);

            using (FileStream stream = new FileStream(Filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(json);
                }
            }
        }

        /// <summary>Gets the graphics settings bank.</summary>
        [DataMember]
        public GraphicsSettings Graphics { get; private set; }

        /// <summary>
        /// Gets the input settings bank.
        /// </summary>
        [DataMember]
        public InputSettings Input { get; private set; }

        /// <summary>
        /// Gets the network settings bank.
        /// </summary>
        [DataMember]
        public NetworkSettings Network { get; private set; }

        /// <summary>
        /// Gets the user interface (UI) settings bank.
        /// </summary>
        [DataMember]
        public UISettings UI { get; private set; }

        /// <summary>Gets or sets the name of the settings file.</summary>
        public string Filename { get; set; } = "settings.json";

        /// <summary>Gets or sets the number of content worker threads. Changing this value will only have an affect before <see cref="Engine"/> is instantiated.</summary>
        public int ContentWorkerThreads { get; set; } = 2;

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
        };

        /// <summary>Gets or sets the product name.</summary>
        public string ProductName { get; set; } = "Stone Bolt Game";

        /// <summary>Gets or sets the default font.</summary>
        public string DefaultFontName { get; set; } = "Arial";

        /// <summary>Gets or sets the default font size.</summary>
        public int DefaultFontSize { get; set; } = 16;

        /// <summary>
        /// Gets or sets whether the engine should render into a native GUI control.
        /// </summary>
        public bool UseGuiControl { get; set; } = false;

        /// <summary>
        /// A list of services to be initialized once the engine is started.
        /// </summary>
        internal List<EngineService> StartupServices { get; } = new List<EngineService>();
    }
}
