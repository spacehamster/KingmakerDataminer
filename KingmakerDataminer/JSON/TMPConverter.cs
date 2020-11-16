using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TMPro;

namespace CustomBlueprints
{
    public class TMPConverter : JsonConverter
    {
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }
        public TMPConverter() { }
        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            var tmp = o as TextMeshProUGUI;
            if(tmp == null)
            {
                w.WriteNull();
                return;
            }
            var j = new JObject();
            j.Add("$type", JsonBlueprints.GetTypeName(tmp.GetType()));
            j.Add("text", tmp.text);
            j.WriteTo(w);
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type type) {
            return typeof(TextMeshProUGUI).IsAssignableFrom(type);

        }
    }
}