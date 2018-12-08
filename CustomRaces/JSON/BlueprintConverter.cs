using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CustomRaces
{
    public class BlueprintConverter : JsonConverter
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
        private BlueprintConverter() { }

        public BlueprintConverter(bool enabled)
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

            var result = ScriptableObject.CreateInstance(realType) as BlueprintScriptableObject;
            JsonBlueprints.Blueprints[name] = result;
            RaceUtil.AddBlueprint(result, name);
            serializer.Populate(jObject.CreateReader(), result);
            return result;
        }

        // ReSharper disable once IdentifierTypo
        private static readonly Type _tBlueprintScriptableObject = typeof(BlueprintScriptableObject);

        public override bool CanConvert(Type type) => Enabled
          && _tBlueprintScriptableObject.IsAssignableFrom(type);
    }
}