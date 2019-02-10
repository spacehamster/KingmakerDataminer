using Harmony12;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomBlueprints
{
    public class LocalizedStringConverter : JsonConverter
    {
        public LocalizedStringConverter() { }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            var ls = (LocalizedString)o;
            for(int i = 0; i < 50 && ls.Shared != null; i++)
            {
                ls = ls.Shared.String;
            }
            var text = ls.ToString();
            w.WriteValue($"LocalizedString:{ls.Key}:{text}");
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            string text = (string)reader.Value;
            if (text == null || text == "null")
            {
                return null;
            }
            if(text.StartsWith("LocalizedString") || text.StartsWith("CustomString"))
            {
                var parts = text.Split(':');
                if (parts.Length < 2) return null;
                var localizedString = new LocalizedString();
                Traverse.Create(localizedString).Field("m_Key").SetValue(parts[1]);
                return localizedString;
            }
            else
            {
                var localizedString = new LocalizedString();
                Traverse.Create(localizedString).Field("m_Key").SetValue(text);
                return localizedString;
            }
        }

        public override bool CanConvert(Type type)
        {
            return typeof(LocalizedString).IsAssignableFrom(type);
        }
    }
}