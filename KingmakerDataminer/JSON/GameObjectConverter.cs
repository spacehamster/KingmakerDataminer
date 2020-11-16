using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CustomBlueprints
{
    public class GameObjectConverter : JsonConverter
    {
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }
        bool CannotWrite { get; set; }
        public override bool CanWrite { get { return !CannotWrite; } }
        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            GameObject gameObject = null;
            JToken t;
            if (o is GameObject go)
            {
                gameObject = go;
            }
            else if (o is Transform tf)
            {
                gameObject = tf.gameObject;
            }
            Console.WriteLine($"Serializing GameObject {gameObject?.name ?? "NULL"} with {this.GetType().Name}");
            using (new PushValue<bool>(true, () => CannotWrite, (canWrite) => CannotWrite = canWrite))
            {
                t = JToken.FromObject(gameObject, szr);
            }
            JObject j = (JObject)t;
            var components = gameObject
                .GetComponents<Component>()
                .Where(c => !typeof(Transform).IsAssignableFrom(c.GetType()));
            j.Add("components", JToken.FromObject(components, szr));

            var transform = new JObject();
            transform.Add("position", JToken.FromObject(gameObject.transform.localPosition, szr));
            transform.Add("rotation", JToken.FromObject(gameObject.transform.localEulerAngles, szr));
            transform.Add("scale", JToken.FromObject(gameObject.transform.localScale, szr));
            j.Add("transform", transform);

            j.Add("parent", JToken.FromObject(gameObject, szr));
            var children = new GameObject[gameObject.transform.childCount];
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                children[i] = gameObject.transform.GetChild(i).gameObject;
            }
            j.Add("children", JToken.FromObject(children, szr));
            szr.Serialize(w, j);
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override bool CanConvert(Type type)
        {
            return typeof(GameObject).IsAssignableFrom(type) || typeof(Transform).IsAssignableFrom(type);
        }
    }
}