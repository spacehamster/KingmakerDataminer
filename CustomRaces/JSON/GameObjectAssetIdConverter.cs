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
#pragma warning disable 618
            var j = new JObject {
        {"$type", typeof(GameObject).AssemblyQualifiedName},
        {"AssetId", UnityEngine.Networking.NetworkTransport.GetAssetId(go)},
        {"InstanceId", go.GetInstanceID()}
      };
#pragma warning restore 618
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
        private static readonly Type _tGameObject = typeof(GameObject);

        public override bool CanConvert(Type type) => Enabled
          && _tGameObject.IsAssignableFrom(type);
    }
}