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
            object rootBlueprint = null;
            {
                if (szr.ContractResolver is BlueprintContractResolver contractResolver)
                {
                    rootBlueprint = contractResolver.RootBlueprint;
                    contractResolver.RootBlueprint = null;
                    contractResolver.RootBlueprintType = null;
                }
            }
            var j = new JObject();
            j.AddFirst(new JProperty("$type", o.GetType().Name));
            foreach (var field in GetSerializableMembers(o.GetType()))
            {
                var value = Traverse.Create(o).Field(field.Name).GetValue();
                j.Add(field.Name, value != null ? JToken.FromObject(value, szr) : null);
            }
            j.WriteTo(w);
            {
                if (szr.ContractResolver is BlueprintContractResolver contractResolver)
                {
                    contractResolver.RootBlueprint = rootBlueprint;
                    contractResolver.RootBlueprintType = rootBlueprint.GetType();
                }
            }
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
        List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            if (objectType == null)
                return new List<MemberInfo>();
            MemberInfo[] publicFields = objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            MemberInfo[] privateFields = objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var result = privateFields
                .Where((field) => Attribute.IsDefined(field, typeof(SerializeField)))
                .Concat(publicFields)
                .Concat(GetSerializableMembers(objectType.BaseType))
                .ToList();
            return result;
        }
        // ReSharper disable once IdentifierTypo
        private static readonly Type _tBlueprintComponent = typeof(BlueprintComponent);

        public override bool CanConvert(Type type) => Enabled
            && _tBlueprintComponent.IsAssignableFrom(type);
    }
}