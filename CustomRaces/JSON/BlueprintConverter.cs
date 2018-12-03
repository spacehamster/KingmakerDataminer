using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomRaces
{
    public class BlueprintConverter : JsonConverter
    {
        [UsedImplicitly]
        public bool Enabled { get; set; }

        private BlueprintConverter() { }

        public BlueprintConverter(bool enabled)
        {
            Enabled = enabled;
        }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            var bp = (BlueprintScriptableObject)o;
            w.WriteValue(string.Format($"Blueprint:{bp.AssetGuid}:{bp.name}"));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer
        )
        {
            var result = (BlueprintScriptableObject)Activator.CreateInstance(objectType);
            JsonBlueprints.Blueprints["TODO: Get name"] = result;
            //todo: populate values
            return result;
        }

        // ReSharper disable once IdentifierTypo
        private static readonly Type _tBlueprintScriptableObject = typeof(BlueprintScriptableObject);

        public override bool CanConvert(Type type) => Enabled
          && _tBlueprintScriptableObject.IsAssignableFrom(type);
    }
}