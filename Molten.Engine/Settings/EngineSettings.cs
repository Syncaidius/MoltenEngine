using System.Runtime.Serialization;
using System.Text;
using Molten.Input;
using Molten.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Molten
{
    [DataContract]
    public class EngineSettings : SettingBank
    {
        List<EngineService> _startupServices;

        public EngineSettings()
        {
            _startupServices = new List<EngineService>();
            StartupServices = _startupServices.AsReadOnly();
            Graphics = AddBank<GraphicsSettings>();
            Input = AddBank<InputSettings>();
            Network = AddBank<NetworkSettings>();
            Audio = AddBank<AudioSettings>();
            UI = AddBank<UISettings>();


            // Add third-party JSON converters.
            JsonConverters = new List<JsonConverter>()
            {
                new StringEnumConverter(),
            };

            // Detect Molten's JSON converters and add them.
            IEnumerable<Type> jcTypes = ReflectionHelper.FindType<JsonConverter>(this.GetType().Assembly);
            foreach (Type t in jcTypes)
            {
                JsonConverter jc = Activator.CreateInstance(t) as JsonConverter;
                JsonConverters.Add(jc);
            }
        }

        /// <summary>
        /// Adds a new <see cref="EngineService"/> to be attached to an <see cref="Engine"/> instance upon initialization.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="EngineService"/>.</typeparam>
        /// <param name="initCallback">The initialization callback to attach to the service.</param>
        /// <exception cref="Exception"></exception>
        public void AddService<T>(string name = null, MoltenEventHandler<EngineService> initCallback = null)
            where T : EngineService, new()
        {
            Type t = typeof(T);
            AddService(t, name, initCallback);
        }

        /// <summary>
        /// Adds a new <see cref="EngineService"/> to be attached to an <see cref="Engine"/> instance upon initialization.
        /// </summary>
        /// <param name="serviceType">The type of <see cref="EngineService"/>.</param>
        /// <param name="initCallback">The initialization callback to attach to the service.</param>
        /// <exception cref="Exception"></exception>
        public void AddService(Type serviceType, string name = null, MoltenEventHandler<EngineService> initCallback = null)
        {
            // Check if service is already in startup list.
            
            foreach (EngineService s in StartupServices)
            {
                Type sType = s.GetType();
                if (sType.IsAssignableFrom(serviceType) || serviceType.IsAssignableFrom(sType))
                    throw new EngineServiceException(null,
                        $"Cannot add startup service of the same or derived type as another ({sType} and {serviceType}).");
            }

            EngineService service = Activator.CreateInstance(serviceType) as EngineService;
            service.Name = name ?? service.Name;

            if (initCallback != null)
                service.OnInitialized += initCallback;

            _startupServices.Add(service);
        }

        /// <summary>
        /// Load settings from the file located at the path defined in <see cref="Path"/>.
        /// </summary>
        public void Load()
        {
            if (!File.Exists(Path))
                return;

            string json = "";

            JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
            jsonSettings.Converters = JsonConverters;

            using (FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    json = reader.ReadToEnd();
                }
            }

            JsonConvert.PopulateObject(json, this, jsonSettings);
        }

        /// <summary>
        /// Saves the current <see cref="EngineSettings"/> to file as serialized JSON.
        /// </summary>
        public void Save()
        {
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
            jsonSettings.Converters = JsonConverters;
            string json = JsonConvert.SerializeObject(this, jsonSettings);

            using (FileStream stream = new FileStream(Path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(json);
                }
            }
        }

        /// <summary>Gets the graphics settings.</summary>
        [DataMember]
        public GraphicsSettings Graphics { get; }

        /// <summary>
        /// Gets the input settings.
        /// </summary>
        [DataMember]
        public InputSettings Input { get; }

        /// <summary>
        /// Gets the network settings.
        /// </summary>
        [DataMember]
        public NetworkSettings Network { get; }

        /// <summary>
        /// Gets the audio settings.
        /// </summary>
        [DataMember]
        public AudioSettings Audio { get; }

        /// <summary>
        /// Gets the user interface (UI) settings bank.
        /// </summary>
        [DataMember]
        public UISettings UI { get; }

        /// <summary>Gets or sets the path (and filename) of the settings file.</summary>
        public string Path { get; set; } = "settings.json";

        public string DefaultAssetPath { get; set; } = "assets/";

        /// <summary>Gets or sets the number of content worker threads. Changing this value will only have an affect before <see cref="Engine"/> is instantiated.</summary>
        public int ContentWorkerThreads { get; set; } = 2;

        /// <summary>
        /// Gets or sets whether or not hot-reloading of content is allowed. Default value is <see cref="true"/>.
        /// </summary>
        public bool AllowContentHotReload { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of milliseconds between hot-reload checks.
        /// </summary>
        public int ContentHotReloadInterval { get; set; } = 500;

        /// <summary>
        /// Gets a list of <see cref="JsonConverter"/> instances that will be to added every new instantiation of <see cref="ContentManager"/>.
        /// </summary>
        public List<JsonConverter> JsonConverters { get; }


        /// <summary>Gets or sets the product name.</summary>
        public string ProductName { get; set; } = "Molten Game";

        /// <summary>
        /// Gets or sets whether the engine should render into a native GUI control.
        /// </summary>
        public bool UseGuiControl { get; set; } = false;

        /// <summary>
        /// A list of services to be initialized once the engine is started.
        /// </summary>
        public IReadOnlyCollection<EngineService> StartupServices { get; }
    }
}
