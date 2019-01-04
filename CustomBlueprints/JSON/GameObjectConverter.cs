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
            throw new NotImplementedException();
        }
        public override bool CanConvert(Type type)
        {
            return typeof(GameObject).IsAssignableFrom(type);
        }
    }
}