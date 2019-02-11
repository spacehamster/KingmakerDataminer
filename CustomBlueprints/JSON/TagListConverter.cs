using Harmony12;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomBlueprints
{
    public class TagListConverter : JsonConverter
    {
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            JArray array = JArray.Load(reader);
            if (type == typeof(BlueprintKingdomEvent.TagList))
            {
                var tag = new BlueprintKingdomEvent.TagList();
                tag.Values = new bool[] { };
                foreach (var value in array)
                {
                    var tagEnum = (BlueprintKingdomEvent.TagType)Enum.Parse(typeof(BlueprintKingdomEvent.TagType), (string)value, true);
                    tag.SetTag(tagEnum, true);
                }
                return tag;
            } else
            {
                var tag = new EventLocationTagList();
                tag.Values = new bool[] { };
                foreach (var value in array)
                {
                    var tagEnum = (EventLocationTagType)Enum.Parse(typeof(EventLocationTagType), (string)value, true);
                    tag.SetTag(tagEnum, true);
                }
                return tag;
            }
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override bool CanConvert(Type type)
        {
            return typeof(TagListBase).IsAssignableFrom(type);
        }
    }
}