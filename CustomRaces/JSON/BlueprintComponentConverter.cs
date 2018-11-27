using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace CustomRaces
{
    public class BlueprintComponentConverter : JsonConverter
    {
        [UsedImplicitly]
        public bool Enabled { get; set; }

        private BlueprintComponentConverter() { }

        public BlueprintComponentConverter(bool enabled)
        {
            Enabled = enabled;
        }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            JToken token = JToken.FromObject(o);
            JObject j = (JObject)token;
            j.AddFirst(new JProperty("$type", o.GetType().Name));
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
        private static readonly Type _tBlueprintComponent = typeof(BlueprintComponent);

        public override bool CanConvert(Type type) => Enabled
            && _tBlueprintComponent.IsAssignableFrom(type);
    }
}