using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
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
        public static readonly HashSet<FieldInfo> FieldBlacklist = new HashSet<FieldInfo>(new[] {
          typeof(PrototypeableObjectBase).GetField("PrototypeLink")
        });
        public static List<MemberInfo> GetUnitySerializableMembers(Type objectType)
        {

            if (objectType == null)
                return new List<MemberInfo>();
            MemberInfo[] publicFields = objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            MemberInfo[] privateFields = objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var result = privateFields
                .Where((field) => Attribute.IsDefined(field, typeof(SerializeField)))
                .Concat(publicFields)
                .Concat(GetUnitySerializableMembers(objectType.BaseType))
                .Where(field => !FieldBlacklist.Contains(field))
                .ToList();
            return result;
        }
        public static JsonSerializer CreateSerializer(BlueprintScriptableObject blueprint)
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
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Include,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                StringEscapeHandling = StringEscapeHandling.Default,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            };
            JsonSerializer serializer
                = JsonSerializer.Create(RefJsonSerializerSettings);
            var bpCr = (BlueprintContractResolver)serializer.ContractResolver;
            bpCr.RootBlueprint = blueprint;
            bpCr.RootBlueprintType = blueprint?.GetType();
            return serializer;
        }

        public static void Dump(BlueprintScriptableObject blueprint)
        {
            Directory.CreateDirectory($"Blueprints/{blueprint.GetType()}");
            var serializer = CreateSerializer(blueprint);
            using (StreamWriter sw = new StreamWriter(Console.OpenStandardOutput()))
            //using (StreamWriter sw = new StreamWriter($"Blueprints/{blueprint.GetType()}/{blueprint.name}.json"))
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
                typeof(BlueprintSpellbook),
                typeof(BlueprintSpellList),
                typeof(BlueprintSpellsTable),
            };
            var blueprints = ResourcesLibrary.GetBlueprints<BlueprintScriptableObject>();
            foreach (var blueprint in blueprints)
            {
                if (types.Contains(blueprint.GetType()))
                {
                    Dump(blueprint);
                }
            }
        }
        public static void DumpQuick()
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
                            typeof(BlueprintSpellbook),
                            typeof(BlueprintSpellList),
                            typeof(BlueprintSpellsTable),
                            typeof(BlueprintItemWeapon),
            };
            foreach(var type in types)
            {
                string assetId;
                RaceUtil.FallbackTable.TryGetValue(type, out assetId);
                if(assetId == null)
                {
                    Main.DebugLog($"No Default {type}");
                    continue;
                }
                Dump(ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(assetId));
            }
        }
        public static void DumpAllBlueprints()
        {
            var blueprints = ResourcesLibrary.GetBlueprints<BlueprintScriptableObject>();
            foreach (var blueprint in blueprints)
            {
                Dump(blueprint);
            }
        }
    }
}
