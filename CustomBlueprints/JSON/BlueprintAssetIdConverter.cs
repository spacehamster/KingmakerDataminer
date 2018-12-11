using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomBlueprints
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
            w.WriteValue(string.Format($"Blueprint:{bp.AssetGuid}:{bp.name}"));
        }

        public override object ReadJson(
          JsonReader reader,
          Type objectType,
          object existingValue,
          JsonSerializer serializer
        )
        {
            string text = (string)reader.Value;
            if (text == null || text == "null")
            {
                return null;
            }
            if (text.StartsWith("Blueprint"))
            {
                var parts = text.Split(':');
                BlueprintScriptableObject blueprintScriptableObject = ResourcesLibrary.TryGetBlueprint(parts[1]);
                if (blueprintScriptableObject == null)
                {
                    throw new JsonSerializationException(string.Format("Failed to load blueprint by guid {0}", text));
                }
                return blueprintScriptableObject;
            }
            if (text.StartsWith("File"))
            {
                var parts = text.Split(':');
                var path = $"{Main.ModPath}/data/{parts[1]}";
                var blueprintName = Path.GetFileNameWithoutExtension(path);
                if (JsonBlueprints.Blueprints.ContainsKey(blueprintName))
                {
                    return JsonBlueprints.Blueprints[blueprintName];
                }
                Main.DebugLog($"Reading blueprint from file: {text}");
                var result = JsonBlueprints.Load(path, objectType);
                return result;
            }
            throw new JsonSerializationException(string.Format("Invalid blueprint format {0}", text));
        }

        // ReSharper disable once IdentifierTypo
        private static readonly Type _tBlueprintScriptableObject = typeof(BlueprintScriptableObject);

        public override bool CanConvert(Type type) => Enabled
          && _tBlueprintScriptableObject.IsAssignableFrom(type);
    }
}