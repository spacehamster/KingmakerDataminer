using Harmony12;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using TMPro;
using UnityEngine;

namespace CustomBlueprints
{
    public static class JsonBlueprints
    {
        /*
         * PrototypeLink is only used to check if blueprint is a companion
         * will need to fix if custom companions are wanted
         */
        private static IAssetProvider m_AssetProvider;
        public static IAssetProvider AssetProvider
        {
            get {
                if(m_AssetProvider == null)
                {
                    m_AssetProvider = new DefaultAssetProvider();
                }
                return m_AssetProvider;
            }
           set {
                m_AssetProvider = value;
            }
        }
        public static Dictionary<string, UnityEngine.Object> Blueprints = new Dictionary<string, UnityEngine.Object>();
        public static Dictionary<string, string> ResourceAssetIds = new Dictionary<string, string>();
        public static readonly HashSet<FieldInfo> FieldBlacklist = new HashSet<FieldInfo>(new[] {
          typeof(PrototypeableObjectBase).GetField("PrototypeLink"),
          AccessTools.Field(typeof(TextMeshProUGUI), "m_subTextObjects"),
          AccessTools.Field(typeof(TextMeshProUGUI), "m_textInfo"),
        });
        public static bool IsBlacklisted(FieldInfo fieldInfo)
        {
            return FieldBlacklist.Any(x => fieldInfo.Name == x.Name && fieldInfo.DeclaringType.FullName == x.DeclaringType.FullName);
        }
        public static bool IsBlacklisted(MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo fieldInfo) return IsBlacklisted(fieldInfo);
            return false;
        }
        public static List<MemberInfo> GetUnitySerializableMembers(Type objectType)
        {

            if (objectType == null)
                return new List<MemberInfo>();
            IEnumerable<MemberInfo> publicFields = objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(f => !f.IsInitOnly);
            IEnumerable<MemberInfo> privateFields = objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            IEnumerable<MemberInfo> newtonsoftFields = objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(f => (f.IsPublic && f.IsInitOnly || f.IsPrivate) && Attribute.IsDefined(f, typeof(JsonPropertyAttribute)));
            IEnumerable<MemberInfo> newtonsoftProperties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(p => Attribute.IsDefined(p, typeof(JsonPropertyAttribute)));
            IEnumerable<MemberInfo> nameProperty = objectType == typeof(UnityEngine.Object) ?
                    new MemberInfo[] { objectType.GetProperty("name") } :
                    Array.Empty<MemberInfo>();
            var result = privateFields
                .Where((field) => Attribute.IsDefined(field, typeof(SerializeField)))
                .Concat(publicFields)
                .Concat(GetUnitySerializableMembers(objectType.BaseType))
                .Concat(nameProperty)
                .Concat(newtonsoftProperties)
                .Concat(newtonsoftFields)
                .Where(field => !IsBlacklisted(field))
                .ToList();
            return result;
        }
        public static string GetTypeName(Type objectType)
        {
            return $"{objectType.FullName}, {objectType.Assembly.GetName().Name}";
        }
        public static JsonSerializerSettings CreateSettings()
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
            return RefJsonSerializerSettings;
        }
        public static object Load(string filepath, Type type)
        {
            var settingsType = typeof(BlueprintScriptableObject).IsAssignableFrom(type) ? type : null;
            var settings = CreateSettings();
            var serializer = JsonSerializer.Create(settings);
            using (StreamReader sr = new StreamReader(filepath))
            using (JsonReader jsonReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize(jsonReader, type);
            }
        }
        public static T Load<T>(string filepath)
        {
            return (T)Load(filepath, typeof(T));
        }
        public static object Loads(string text, Type type)
        {
            var settingsType = typeof(BlueprintScriptableObject).IsAssignableFrom(type) ? type : null;
            var settings = CreateSettings();
            var serializer = JsonSerializer.Create(settings);
            using (StringReader sr = new StringReader(text))
            using (JsonReader jsonReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize(jsonReader, type);
            }
        }
        public static T Loads<T>(string text)
        {
            return (T)Loads(text, typeof(T));
        }
        public static void Dump(BlueprintScriptableObject blueprint, string path)
        {
            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
            var settings = CreateSettings();
            JsonSerializer serializer = JsonSerializer.Create();
            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, blueprint);
            }
        }
        public static void Dump(object obj, string path)
        {
            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
            var settings = CreateSettings();
            JsonSerializer serializer = JsonSerializer.Create();
            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, obj);
            }
        }
        public static void DumpResource(object obj, string path)
        {
            var folder = Path.GetDirectoryName(path);
            if(!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
            var settings = CreateSettings();
            settings.Error = HandleDeserializationError;
            var contractResolver = settings.ContractResolver as BlueprintContractResolver;
            contractResolver.PreferredConverters.Insert(0, new GameObjectConverter());
            contractResolver.PreferredConverters.RemoveAll(c => c.GetType() == typeof(GameObjectAssetIdConverter));
            JsonSerializer serializer = JsonSerializer.Create(settings);
            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, obj);
            }
        }

        private static void HandleDeserializationError(object sender, 
            Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            var currentError = e.ErrorContext.Error.Message;
            Main.DebugLog(currentError);
            e.ErrorContext.Handled = true;
        }
    }
}
