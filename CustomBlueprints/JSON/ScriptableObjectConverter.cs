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

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
        public ScriptableObjectConverter() { }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            throw new NotImplementedException();
        }
        public object ReadResource(JsonReader reader, Type objectType, object existingValue, JsonSerializer szr)
        {
            JObject jObject = JObject.Load(reader);
            var name = (string)jObject["name"];
            Main.DebugLog($"Deserializing {name} of {objectType.Name} with {GetType().Name}");
            var typeName = (string)jObject["$type"];
            var realType = Type.GetType(typeName);
            ScriptableObject result = null;
            if (jObject["$append"] != null)
            {
                var settings = JsonBlueprints.CreateSettings(null);
                szr = JsonSerializer.Create(settings);
                szr.ObjectCreationHandling = ObjectCreationHandling.Reuse;
                var copy = (string)jObject["$append"];
                jObject.Remove("$append");
                var parts = copy.Split(':');
                result = ResourcesLibrary.TryGetResource<ScriptableObject>(parts[1]);
                name = result.name;
                Main.DebugLog($"Appending to {result.name}");
            }
            if (jObject["$replace"] != null)
            {

                var copy = (string)jObject["$replace"];
                jObject.Remove("$replace");
                var parts = copy.Split(':');
                result = ResourcesLibrary.TryGetBlueprint(parts[1]);
                name = result.name;
            }
            if (jObject["$copy"] != null)
            {
                var copy = (string)jObject["$copy"];
                jObject.Remove("$copy");
                var parts = copy.Split(':');
                var resource = ResourcesLibrary.TryGetResource<ScriptableObject>(parts[1]);
                result = (ScriptableObject)BlueprintUtil.ShallowClone(resource);
                Main.DebugLog($"Copying {resource.name}");
            }
            if (result == null)
            {
                result = ScriptableObject.CreateInstance(realType);
            }
            szr.Populate(jObject.CreateReader(), result);
            return result;
        }
        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer szr)
        {
            if (type == typeof(EquipmentEntity)
                || type == typeof(BakedCharacter)
                || type == typeof(SimClothTopology))
            {
                return ReadResource(reader, type, existingValue, szr);
            }
            if (reader.TokenType == JsonToken.Null) return null;
            JObject jObject = JObject.Load(reader);
            var typeName = (string)jObject["$type"];
            var realType = Type.GetType(typeName);
            var result = ScriptableObject.CreateInstance(realType);
            szr.Populate(jObject.CreateReader(), result);
            return result;
        }
        public override bool CanConvert(Type type)
        {
            return typeof(ScriptableObject).IsAssignableFrom(type)
              && !typeof(BlueprintScriptableObject).IsAssignableFrom(type);
        }
    }
}