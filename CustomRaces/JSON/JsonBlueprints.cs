using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Visual.CharacterSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomRaces
{
    public static class JsonBlueprints
    {
        /*
         * PrototypeLink is only used to check if blueprint is a companion
         * will need to fix if custom companions are wanted
         */
        public static readonly HashSet<FieldInfo> FieldBlacklist = new HashSet<FieldInfo>(new[] {
          typeof(PrototypeableObjectBase).GetField("PrototypeLink")
        });
        public static List<MemberInfo> GetUnitySerializableMembers(Type objectType)
        {

            if (objectType == null)
                return new List<MemberInfo>();
            MemberInfo[] publicFields = objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            MemberInfo[] privateFields = objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            MemberInfo[] nameProperty = objectType == typeof(UnityEngine.Object) ?
                    new MemberInfo[] { objectType.GetProperty("name") } :
                    Array.Empty<MemberInfo>();
            var result = privateFields
                .Where((field) => Attribute.IsDefined(field, typeof(SerializeField)))
                .Concat(publicFields)
                .Concat(GetUnitySerializableMembers(objectType.BaseType))
                .Concat(nameProperty)
                .Where(field => !FieldBlacklist.Contains(field))
                .ToList();
            return result;
        }
        public static JsonSerializerSettings CreateSettings(Type blueprintType)
        {
            var RefJsonSerializerSettings = new JsonSerializerSettings
            {
                CheckAdditionalContent = false,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new BlueprintContractResolver(),
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DefaultValueHandling = DefaultValueHandling.Include,
                //Error = SkipJsonErrors,
                FloatFormatHandling = FloatFormatHandling.String,
                FloatParseHandling = FloatParseHandling.Double,
                Formatting = Formatting.Indented,
                // MaxDepth = 12, // ignored
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                StringEscapeHandling = StringEscapeHandling.Default,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.Objects
            };
            var bpCr = (BlueprintContractResolver)RefJsonSerializerSettings.ContractResolver;
            bpCr.RootBlueprintType = blueprintType;

            return RefJsonSerializerSettings;
        }

        public static T Load<T>(string filepath)
        {
            var type = typeof(BlueprintScriptableObject).IsAssignableFrom(typeof(T)) ? typeof(T) : null;
            var settings = CreateSettings(type);
            var text = File.ReadAllText(filepath);
            return JsonConvert.DeserializeObject<T>(text, settings);
        }
        public class RaceConverter : CustomCreationConverter<BlueprintRace>
        {
            public override BlueprintRace Create(Type objectType)
            {
                return new BlueprintRace();
            }
        }
        public static T Loads<T>(string text)
        {
            var type = typeof(BlueprintScriptableObject).IsAssignableFrom(typeof(T)) ? typeof(T) : null;
            var settings = CreateSettings(type);
            var serializer = JsonSerializer.Create(settings);
            var jsonReader = new JsonTextReader(new StringReader(text));
            using (StringReader sr = new StringReader(text))
            using (JsonReader writer = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonReader);
            }
        }
        public static void Dump(BlueprintScriptableObject blueprint)
        {
            Directory.CreateDirectory($"Blueprints/{blueprint.GetType()}");
            JsonSerializer serializer = JsonSerializer.Create(CreateSettings(blueprint.GetType()));
            //using (StreamWriter sw = new StreamWriter(Console.OpenStandardOutput()))
            using (StreamWriter sw = new StreamWriter($"Blueprints/{blueprint.GetType()}/{blueprint.name}.{blueprint.AssetGuid}.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, blueprint);
            }
        }
        public static void Dump(EquipmentEntity ee, string assetId)
        {
            Directory.CreateDirectory($"Blueprints/{ee.GetType()}");
            JsonSerializer serializer
                            = JsonSerializer.Create(CreateSettings(null));
            //using (StreamWriter sw = new StreamWriter(Console.OpenStandardOutput()))
            using (StreamWriter sw = new StreamWriter($"Blueprints/{ee.GetType()}/{ee.name}.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {

                serializer.Serialize(writer, ee);

            }
        }
        private static void SkipJsonErrors(object o, Newtonsoft.Json.Serialization.ErrorEventArgs err)
        {
            throw new Exception(err.ToString());
            err.ErrorContext.Handled = true;
        }
       

    }
}
