using System;
using System.Linq;
using Harmony12;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.SimCloth;
using Kingmaker.Visual.CharacterSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CustomBlueprints
{
    public class ScriptableObjectConverter : JsonConverter
    {

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
        public bool CannotRead;
        public override bool CanRead
        {
            get
            {
                return !CannotRead;
            }
        }
        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            throw new NotImplementedException();
        }
        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer szr)
        {
            throw new NotImplementedException();
        }
        public override bool CanConvert(Type type)
        {
            return typeof(ScriptableObject).IsAssignableFrom(type)
              && !typeof(BlueprintScriptableObject).IsAssignableFrom(type);
        }
    }
}