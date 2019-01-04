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
            int previewLength = 20;
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
            if(text.StartsWith("LocalizedString"))
            {
                var parts = text.Split(':');
                if (parts.Length < 2) return null;
                var localizedString = new LocalizedString();
                Traverse.Create(localizedString).Field("m_Key").SetValue(parts[1]);
                return localizedString;
            }
            {
                var parts = text.Split(':');
                string key = null;
                if (parts.Length < 2 || !text.StartsWith("CustomString"))
                {
                    key = "InvalidKey" + text;
                    LocalizationManager.CurrentPack.Strings[key] = key;
                }
                else
                {
                    key = parts[1];
                    if (!LocalizationManager.CurrentPack.Strings.ContainsKey(key))
                    {
                        LocalizationManager.CurrentPack.Strings[key] = "Missing" + text;
                    }
                }
                var localizedString = new LocalizedString();
                Traverse.Create(localizedString).Field("m_Key").SetValue(key);
                return localizedString;
            }
        }

        // ReSharper disable once IdentifierTypo
        private static readonly Type _tBlueprintScriptableObject = typeof(LocalizedString);

        public override bool CanConvert(Type type) => _tBlueprintScriptableObject.IsAssignableFrom(type);
    }
}