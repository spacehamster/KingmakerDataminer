using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
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
            var newSerializer = JsonSerializer.Create(JsonBlueprints.CreateSettings(null));
            var j = new JObject();
            j.AddFirst(new JProperty("$type", o.GetType().Name));
            foreach (var field in GetSerializableMembers(o.GetType()))
            {
                var value = Traverse.Create(o).Field(field.Name).GetValue();
                j.Add(field.Name, value != null ? JToken.FromObject(value, newSerializer) : null);
            }
            j.WriteTo(w);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            JToken token = JToken.Load(reader);
            throw new NotImplementedException();
        }
        List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            return JsonBlueprints.GetUnitySerializableMembers(objectType);
        }
        // ReSharper disable once IdentifierTypo
        private static readonly Type _tBlueprintComponent = typeof(BlueprintComponent);

        public override bool CanConvert(Type type) => Enabled
            && _tBlueprintComponent.IsAssignableFrom(type);
    }
}