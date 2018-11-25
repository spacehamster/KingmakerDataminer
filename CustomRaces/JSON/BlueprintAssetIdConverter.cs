using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomRaces
{
    public class BlueprintAssetIdConverter : JsonConverter
    {
        [UsedImplicitly]
        public bool Enabled { get; set; }

        private BlueprintAssetIdConverter() { }

        public BlueprintAssetIdConverter(bool enabled)
        {
            Enabled = enabled;
        }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            var bp = (BlueprintScriptableObject)o;

            var j = new JObject {
            {"$type", o.GetType().AssemblyQualifiedName},
            {"AssetGuid", bp.AssetGuid},
            {"InstanceId", bp.GetInstanceID()}
          };
            j.WriteTo(w);
        }

        public override object ReadJson(
          JsonReader reader,
          Type objectType,
          object existingValue,
          JsonSerializer serializer
        )
        {
            throw new NotImplementedException();
        }

        // ReSharper disable once IdentifierTypo
        private static readonly Type _tBlueprintScriptableObject = typeof(BlueprintScriptableObject);

        public override bool CanConvert(Type type) => Enabled
          && _tBlueprintScriptableObject.IsAssignableFrom(type);
    }
}