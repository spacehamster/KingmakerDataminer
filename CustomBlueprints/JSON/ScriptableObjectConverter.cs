using System;
using System.Linq;
using Harmony12;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.SimCloth;
using Kingmaker.Visual.CharacterSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CustomBlueprints
{
    public class ScriptableObjectConverter : JsonConverter
    {
        [UsedImplicitly]
        public bool Enabled { get; set; }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
        private ScriptableObjectConverter() { }

        public ScriptableObjectConverter(bool enabled)
        {
            Enabled = enabled;
        }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer szr
        )
        {
            JObject jObject = JObject.Load(reader);
            var name = (string)jObject["name"];
            Main.DebugLog($"Deserializing {name} of {objectType.Name} with {GetType().Name}");
            var typeName = (string)jObject["$type"];
            var realType = Type.GetType(typeName);
            ScriptableObject result = null;
            if (jObject["$append"] != null)
            {
                var copy = (string)jObject["$append"];
                jObject.Remove("$append");
                var parts = copy.Split(':');
                result = ResourcesLibrary.TryGetResource<BlueprintScriptableObject>(parts[1]);
                Main.DebugLog($"Appending to {result.name}");
            }
            if (jObject["$copy"] != null)
            {
                var copy = (string)jObject["$copy"];
                jObject.Remove("$copy");
                var parts = copy.Split(':');
                var resource = ResourcesLibrary.TryGetResource<BlueprintScriptableObject>(parts[1]);
                result = (BlueprintScriptableObject)BlueprintUtil.ShallowClone(resource);
                Main.DebugLog($"Copying {resource.name}");
            }
            if (result == null)
            {
                result = ScriptableObject.CreateInstance(realType) as BlueprintScriptableObject;
            }
            szr.Populate(jObject.CreateReader(), result);
            return result;
        }
        // ReSharper disable once IdentifierTypo
        private static readonly Type _tScriptableObject = typeof(ScriptableObject);
        public override bool CanConvert(Type type)
        {
            var isResourceType = type == typeof(EquipmentEntity)
                || type == typeof(BakedCharacter)
                || type == typeof(SimClothTopology);
            var result = Enabled
              && isResourceType
              && !typeof(BlueprintScriptableObject).IsAssignableFrom(type);
            return result;
        }
    }
}