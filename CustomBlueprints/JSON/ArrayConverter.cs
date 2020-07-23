using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CustomBlueprints
{
    public class ArrayConverter : JsonConverter
    {
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }
        public ArrayConverter() { }
        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            JArray array = JArray.Load(reader);
            Array existingArray = (Array)existing;
            int offset = serializer.ObjectCreationHandling == ObjectCreationHandling.Reuse && existing != null ?
                   existingArray.Length : 0;
            var result = Array.CreateInstance(type.GetElementType(), array.Count + offset);
            for (int i = 0; i < offset; i++)
            {
                result.SetValue(existingArray.GetValue(i), i);
            }
            for (int i = 0; i < array.Count; i++)
            {
                var token = array[i];
                var value = token.ToObject(type.GetElementType(), serializer);
                result.SetValue(value, i + offset);
            }

            return result;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override bool CanConvert(Type type)
        {
            return type.IsArray;
        }

    }
}