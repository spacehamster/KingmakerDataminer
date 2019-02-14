using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CustomBlueprints
{
    public class BlueprintConverter : JsonConverter
    {
        public bool CannotWrite;
        public override bool CanWrite
        {
            get
            {
                return !CannotWrite;
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
        public BlueprintConverter() { }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            using (new PushValue<bool>(true, () => CannotWrite, (canWrite) => CannotWrite = canWrite))
            {
                szr.Serialize(w, o);
            }
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer szr)
        {
            if(reader.TokenType == JsonToken.String)
            {
                //TODO: Fix BlueprintConverter receiving string members
                return new BlueprintAssetIdConverter().ReadJson(reader, objectType, existingValue, szr);
            }
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            JObject jObject = JObject.Load(reader);
            var name = (string)jObject["name"];
            var typeName = (string)jObject["$type"];
            var realType = Type.GetType(typeName);
            var serializer = szr;
            BlueprintScriptableObject result = null;
            if (jObject["$append"] != null)
            {
                serializer.ObjectCreationHandling = ObjectCreationHandling.Reuse;
                var copy = (string)jObject["$append"];
                jObject.Remove("$append");
                var parts = copy.Split(':');
                result = JsonBlueprints.AssetProvider.GetBlueprint(realType, parts[1]);
                name = result.name;
                Main.DebugLog($"Appending to {result.name}");
            }
            if (jObject["$replace"] != null)
            {
                var copy = (string)jObject["$replace"];
                jObject.Remove("$replace");
                var parts = copy.Split(':');
                result = JsonBlueprints.AssetProvider.GetBlueprint(realType, parts[1]);
                name = result.name;
                Main.DebugLog($"replacing to {result.name}");
            }
            if (jObject["$copy"] != null)
            {
                var copy = (string)jObject["$copy"];
                jObject.Remove("$copy");
                var parts = copy.Split(':');
                var resource = JsonBlueprints.AssetProvider.GetBlueprint(realType, parts[1]);
                result = (BlueprintScriptableObject)BlueprintUtil.ShallowClone(resource);
                Main.DebugLog($"Copying {resource.name}");
            }
            if (name == null)
            {
                throw new System.Exception("Missing name");
            }
            if (JsonBlueprints.Blueprints.ContainsKey(name))
            {
                //throw new System.Exception("Cannot create blueprint twice");
            }
            if (result == null)
            {
                result = ScriptableObject.CreateInstance(realType) as BlueprintScriptableObject;
            }
            var assetId = (string)jObject["m_AssetGuid"];
            if (assetId == null) assetId = name;
            JsonBlueprints.AssetProvider.AddBlueprint(result, assetId);
            using (new PushValue<bool>(true, () => CannotRead, (canRead) => CannotRead = canRead))
            {
                serializer.Populate(jObject.CreateReader(), result);
            }
            return result;
        }
        public override bool CanConvert(Type type)
        {
            return typeof(BlueprintScriptableObject).IsAssignableFrom(type);
        }
    }
}