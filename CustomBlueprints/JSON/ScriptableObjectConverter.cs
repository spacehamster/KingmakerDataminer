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
        public bool CannotRead;
        public override bool CanRead
        {
            get
            {
                return !CannotRead;
            }
        }
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
                var settings = JsonBlueprints.CreateSettings();
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
            if (reader.TokenType == JsonToken.Null) return null;
            if (type == typeof(EquipmentEntity)
                || type == typeof(BakedCharacter)
                || type == typeof(SimClothTopology))
            {
                return ReadResource(reader, type, existingValue, szr);
            }
            //TODO: Fix json reading of blueprint fields of type ScriptabeObject (BlueprintCueBase.ParentAsset)
            if (reader.TokenType == JsonToken.String)
            {
                return new BlueprintAssetIdConverter().ReadJson(reader, type, existingValue, szr);
            }
            using (new PushValue<bool>(true, () => CannotRead, (canRead) => CannotRead = canRead))
            {
                return szr.Deserialize(reader);
            }
        }
        public override bool CanConvert(Type type)
        {
            return typeof(ScriptableObject).IsAssignableFrom(type)
              && !typeof(BlueprintScriptableObject).IsAssignableFrom(type);
        }
    }
}