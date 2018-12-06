using Kingmaker.Blueprints;
using Kingmaker.Visual.CharacterSystem;
using Newtonsoft.Json;
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
        public static Dictionary<string, UnityEngine.Object> Blueprints = new Dictionary<string, UnityEngine.Object>();
        public static Dictionary<string, string> ResourceAssetIds = new Dictionary<string, string>();
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
                FloatFormatHandling = FloatFormatHandling.String,
                FloatParseHandling = FloatParseHandling.Double,
                Formatting = Formatting.Indented,
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
            var serializer = JsonSerializer.Create(settings);

            using (StreamReader sr = new StreamReader(filepath))
            using (JsonReader jsonReader = new JsonTextReader(sr))
            {
                try
                {
                    return serializer.Deserialize<T>(jsonReader);
                } catch(Exception ex)
                {
                    throw new Exception($"Error deserializing {filepath}", ex);
                }
            }
        }
        public static T Loads<T>(string text)
        {
            var type = typeof(BlueprintScriptableObject).IsAssignableFrom(typeof(T)) ? typeof(T) : null;
            var settings = CreateSettings(type);
            var serializer = JsonSerializer.Create(settings);
            using (StringReader sr = new StringReader(text))
            using (JsonReader jsonReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonReader);
            }
        }
        public static void Dump(BlueprintScriptableObject blueprint)
        {
            Directory.CreateDirectory($"Blueprints/{blueprint.GetType()}");
            JsonSerializer serializer = JsonSerializer.Create(CreateSettings(blueprint.GetType()));
            using (StreamWriter sw = new StreamWriter($"Blueprints/{blueprint.GetType()}/{blueprint.name}.{blueprint.AssetGuid}.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, blueprint);
            }
        }
        public static void Dump(UnityEngine.Object ee, string assetId)
        {
            Directory.CreateDirectory($"Blueprints/{ee.GetType()}");
            JsonSerializer serializer
                            = JsonSerializer.Create(CreateSettings(null));
            using (StreamWriter sw = new StreamWriter($"Blueprints/{ee.GetType()}/{ee.name}.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {

                serializer.Serialize(writer, ee);

            }
        }
    }
}
