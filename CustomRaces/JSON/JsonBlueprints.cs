using Kingmaker.Blueprints;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace CustomRaces
{
    public static class JsonBlueprints
    {
        public static void Dump(BlueprintScriptableObject obj)
        {
            var RefJsonSerializerSettings = new JsonSerializerSettings
            {
                CheckAdditionalContent = false,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new BlueprintContractResolver(),
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Error = SkipJsonErrors,
                FloatFormatHandling = FloatFormatHandling.String,
                FloatParseHandling = FloatParseHandling.Double,
                Formatting = Formatting.Indented,
                // MaxDepth = 12, // ignored
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Include,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                StringEscapeHandling = StringEscapeHandling.Default,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            };
            Directory.CreateDirectory($"Blueprints/{obj.GetType()}");
            JsonSerializer serializer
                = JsonSerializer.Create(RefJsonSerializerSettings);
            using (StreamWriter sw = new StreamWriter($"Blueprints/{obj.GetType()}/{obj.name}.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, obj);

            }
        }
        private static void SkipJsonErrors(object o, Newtonsoft.Json.Serialization.ErrorEventArgs err)
        {
            err.ErrorContext.Handled = true;
        }
    }
}
