using Kingmaker.Blueprints.Classes.Spells;
using Newtonsoft.Json;
using System;

namespace CustomBlueprints
{
    public class SpellDescriptorWrapperConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            var spellDescriptorWrapper = (SpellDescriptorWrapper)o;
            szr.Serialize(w, spellDescriptorWrapper.Value);
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            string text = (string)reader.Value;
            var success = Enum.TryParse<SpellDescriptor>(text, out SpellDescriptor result);
            if (!success)
            {
                throw new ArgumentException($"{text} is not a valid SpellDescriptor");
            }
            return new SpellDescriptorWrapper(result);
        }

        public override bool CanConvert(Type type)
        {
            return type == typeof(SpellDescriptorWrapper);
        }
    }
}