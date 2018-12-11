using System;
using System.Linq;
using Harmony12;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CustomBlueprints
{
    public class BlueprintConverter : JsonConverter
    {
        [UsedImplicitly]
        public bool Enabled { get; set; }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }
        private BlueprintConverter() { }

        public BlueprintConverter(bool enabled)
        {
            Enabled = enabled;
        }
        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            var settings = JsonBlueprints.CreateSettings(null);
            var newSerializer = JsonSerializer.Create(settings);
            var j = new JObject();
            j.AddFirst(new JProperty("$type", JsonBlueprints.GetTypeName(o.GetType())));
            foreach (var field in JsonBlueprints.GetUnitySerializableMembers(o.GetType()))
            {
                var value = Traverse.Create(o).Field(field.Name).GetValue();
                j.Add(field.Name, value != null ? JToken.FromObject(value, newSerializer) : null);
            }
            j.WriteTo(w);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer szr
        )
        {
            JObject jObject = JObject.Load(reader);
            var name = (string)jObject["name"];
            var typeName = (string)jObject["$type"];
            var realType = Type.GetType(typeName);
            var settings = JsonBlueprints.CreateSettings(realType);
            var serializer = JsonSerializer.Create(settings);
            if (name == null)
            {
                throw new System.Exception("Missing name");
            }
            if (JsonBlueprints.Blueprints.ContainsKey(name))
            {
                throw new System.Exception("Cannot create blueprint twice");
            }
            BlueprintScriptableObject result = null;
            if (jObject["$append"] != null)
            {
                var copy = (string)jObject["$append"];
                jObject.Remove("$append");
                var parts = copy.Split(':');
                result = ResourcesLibrary.TryGetBlueprint(parts[1]);
                Main.DebugLog($"Appending to {result.name}");
            }
            if (jObject["$copy"] != null)
            {
                var copy = (string)jObject["$copy"];
                jObject.Remove("$copy");
                var parts = copy.Split(':');
                var resource = ResourcesLibrary.TryGetBlueprint(parts[1]);
                result = (BlueprintScriptableObject)BlueprintUtil.ShallowClone(resource);
                Main.DebugLog($"Copying {resource.name}");
            }
            if(result == null)
            {
                result = ScriptableObject.CreateInstance(realType) as BlueprintScriptableObject;
            }
            JsonBlueprints.Blueprints[name] = result;
            BlueprintUtil.AddBlueprint(result, name);
            serializer.Populate(jObject.CreateReader(), result);
            return result;
        }

        // ReSharper disable once IdentifierTypo
        private static readonly Type _tBlueprintScriptableObject = typeof(BlueprintScriptableObject);

        public override bool CanConvert(Type type) => Enabled
          && _tBlueprintScriptableObject.IsAssignableFrom(type);
    }
}