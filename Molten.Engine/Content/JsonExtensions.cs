using Newtonsoft.Json;

namespace Molten
{
    internal static class JsonExtensions
    {
        public static JsonSerializerSettings Clone(this JsonSerializerSettings settings)
        {
            JsonSerializerSettings copiedSerializer = new JsonSerializerSettings
            {
                Context = settings.Context,
                Culture = settings.Culture,
                ContractResolver = settings.ContractResolver,
                ConstructorHandling = settings.ConstructorHandling,
                CheckAdditionalContent = settings.CheckAdditionalContent,
                DateFormatHandling = settings.DateFormatHandling,
                DateFormatString = settings.DateFormatString,
                DateParseHandling = settings.DateParseHandling,
                DateTimeZoneHandling = settings.DateTimeZoneHandling,
                DefaultValueHandling = settings.DefaultValueHandling,
                EqualityComparer = settings.EqualityComparer,
                FloatFormatHandling = settings.FloatFormatHandling,
                Formatting = settings.Formatting,
                FloatParseHandling = settings.FloatParseHandling,
                MaxDepth = settings.MaxDepth,
                MetadataPropertyHandling = settings.MetadataPropertyHandling,
                MissingMemberHandling = settings.MissingMemberHandling,
                NullValueHandling = settings.NullValueHandling,
                ObjectCreationHandling = settings.ObjectCreationHandling,
                PreserveReferencesHandling = settings.PreserveReferencesHandling,
                ReferenceLoopHandling = settings.ReferenceLoopHandling,
                StringEscapeHandling = settings.StringEscapeHandling,
                TraceWriter = settings.TraceWriter,
                TypeNameHandling = settings.TypeNameHandling,
                SerializationBinder = settings.SerializationBinder,
                TypeNameAssemblyFormatHandling = settings.TypeNameAssemblyFormatHandling,
                ReferenceResolver = settings.ReferenceResolver,
                ReferenceResolverProvider = settings.ReferenceResolverProvider,
                TypeNameAssemblyFormat = settings.TypeNameAssemblyFormat,
                Binder = settings.Binder,
            };

            foreach (JsonConverter converter in settings.Converters)
            {
                copiedSerializer.Converters.Add(converter);
            }
            return copiedSerializer;
        }
    }
}
