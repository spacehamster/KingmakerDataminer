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

namespace CustomRaces
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
            var copy = (string)jObject["$copy"];
            if (copy != null) jObject.Remove("$copy");
            ScriptableObject result = null;
            if (copy != null)
            {
                var parts = copy.Split(':');
                var resource = ResourcesLibrary.TryGetResource<ScriptableObject>(parts[1]);
                result = (ScriptableObject)BlueprintUtil.ShallowClone(resource);
                Main.DebugLog($"Copying {resource.name}");
            }
            else
            {
                result = ScriptableObject.CreateInstance(realType);
            }
            szr.Populate(jObject.CreateReader(), result);
            //Main.DebugLog($"Deserializing {result.name}!");
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