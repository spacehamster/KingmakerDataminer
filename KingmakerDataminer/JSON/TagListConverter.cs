using Harmony12;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;

namespace CustomBlueprints
{
    public class TagListConverter : JsonConverter
    {
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
        /*
         * There seems to be a bug in TagLists that allow them to be created without the Values field being initialized.
         */
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            if (value == null)
            {
                serializer.Serialize(writer, null);
            } else
            {
                var values = AccessTools.Field(value.GetType(), "Values").GetValue(value);
                if (values == null)
                {
                    var obj = new JObject();
                    obj["$type"] = value.GetType().FullName;
                    obj["Values"] = null;
                    serializer.Serialize(writer, obj);
                } else
                {
                    var foo = (IEnumerable)value;
                    var arr = new JArray();
                    foreach(var item in foo)
                    {
                        arr.Add(item);
                    }
                    serializer.Serialize(writer, arr);
                }
            }
        }
        public override bool CanConvert(Type type)
        {
            return typeof(TagListBase).IsAssignableFrom(type);
        }
    }
}