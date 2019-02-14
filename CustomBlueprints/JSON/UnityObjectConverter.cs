using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using JetBrains.Annotations;
using Newtonsoft.Json.Converters;

namespace CustomBlueprints
{
    /*
     * Needed to read Bandits.2d37aa73f6f734d46bcf4b905c003511.json
     * Newtonsoft.Json.JsonSerializationException
     * Error converting value "Blueprint:2c358841b7cb7f94c8ddab068b734fb1:Hugard" to type 'UnityEngine.Object'. Path 'InnerResources[0]'
     * ArgumentException: Could not cast or convert from System.String to UnityEngine.Object.
     */
    public class UnityObjectConverter : JsonConverter
    {
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
        bool CannotRead;
        public override bool CanRead
        {
            get
            {
                return !CannotRead;
            }
        }
        public override void WriteJson(JsonWriter w, object value, JsonSerializer szr)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            if(reader.TokenType == JsonToken.String)
            {
                //TODO: Fix
                return new BlueprintAssetIdConverter().ReadJson(reader, type, existing, serializer);
            }
            using (new PushValue<bool>(true, () => CannotRead, (canRead) => CannotRead = canRead))
            {
                return serializer.Deserialize(reader);
            }
        }
        public override bool CanConvert(Type objectType)
        {
            return typeof(UnityEngine.Object).IsAssignableFrom(objectType);
        }
    }
}