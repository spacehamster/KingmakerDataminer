using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
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
        public static void Dump(BlueprintScriptableObject blueprint)
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
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                StringEscapeHandling = StringEscapeHandling.Default,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            };
            Directory.CreateDirectory($"Blueprints/{blueprint.GetType()}");
            JsonSerializer serializer
                = JsonSerializer.Create(RefJsonSerializerSettings);
            var bpCr = (BlueprintContractResolver)serializer.ContractResolver;
            bpCr.RootBlueprint = blueprint;
            bpCr.RootBlueprintType = blueprint.GetType();
            using (StreamWriter sw = new StreamWriter($"Blueprints/{blueprint.GetType()}/{blueprint.name}.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {

                serializer.Serialize(writer, blueprint);

            }
        }
        private static void SkipJsonErrors(object o, Newtonsoft.Json.Serialization.ErrorEventArgs err)
        {
            err.ErrorContext.Handled = true;
        }
        public static void DumpBlueprints()
        {
            var types = new Type[]
            {
                typeof(BlueprintCharacterClass),
                typeof(BlueprintRaceVisualPreset),
                typeof(BlueprintRace),
                typeof(BlueprintArchetype),
                typeof(BlueprintProgression),
                typeof(BlueprintStatProgression),
                typeof(BlueprintFeature),
                typeof(BlueprintSpellbook)
            };
            var blueprints = ResourcesLibrary.GetBlueprints<BlueprintScriptableObject>();
            foreach(var blueprint in blueprints)
            {
                if (types.Contains(blueprint.GetType())){
                    Dump(blueprint);
                }
            }


        }
    }

}
