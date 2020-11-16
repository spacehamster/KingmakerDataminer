using Newtonsoft.Json;
using System;
using TMPro;

namespace CustomBlueprints
{
    public class TMPFontConverter : JsonConverter
    {
        public TMPFontConverter() { }
        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            w.WriteValue($"{o}");
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            //TODO: Get actual font
            return new TMP_FontAsset();
        }
        public override bool CanConvert(Type type) {
            return typeof(TMP_FontAsset).IsAssignableFrom(type);

        }
    }
}