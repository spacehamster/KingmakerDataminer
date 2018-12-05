using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace CustomRaces
{
    public class GameObjectAssetIdConverter : JsonConverter
    {
        [UsedImplicitly]
        public bool Enabled { get; set; }

        private GameObjectAssetIdConverter() { }

        public GameObjectAssetIdConverter(bool enabled)
        {
            Enabled = enabled;
        }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {

            var go = (GameObject)o;
            var j = new JObject {
                {"$type", go.GetType().Name},
                {"InstanceId", go.GetInstanceID()},
                {"name", go.name },
            };
            j.WriteTo(w);
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            if (reader.Value == null) return null;
            JObject jObject = JObject.Load(reader);
            int instanceId = (int)jObject["InstanceId"];
            var result = RaceUtil.FindObjectByInstanceId(instanceId, type);
            if (result == null)
            {
                throw new System.Exception($"Couldn't find object with InstanceId {instanceId}");
            }
            return result;
        }

        // ReSharper disable once IdentifierTypo
        private static readonly Type _tGameObject = typeof(GameObject);

        public override bool CanConvert(Type type) => Enabled
          && _tGameObject.IsAssignableFrom(type);
    }
}