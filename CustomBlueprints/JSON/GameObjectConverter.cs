using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace CustomBlueprints
{
    public class GameObjectConverter : JsonConverter
    {
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }
        public GameObjectConverter() { }
        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {

            var go = (UnityEngine.GameObject)o;
            var components = new JArray();
            foreach(var component in go.GetComponents<MonoBehaviour>())
            {
                components.Add(component);
            }
            var j = new JObject {
                {"$type", go.GetType().Name},
                {"transform", JToken.FromObject(go, szr) },
                {"components", components },
                {"name", go.name },
            };
            j.WriteTo(w);
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override bool CanConvert(Type type)
        {
            return typeof(GameObject).IsAssignableFrom(type);
        }
    }
}