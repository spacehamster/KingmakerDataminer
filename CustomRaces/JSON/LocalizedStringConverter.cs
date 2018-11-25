using JetBrains.Annotations;
using Kingmaker.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRaces
{
    public class LocalizedStringConverter : JsonConverter
    {
        [UsedImplicitly]
        public bool Enabled { get; set; }

        private LocalizedStringConverter() { }

        public LocalizedStringConverter(bool enabled)
        {
            Enabled = enabled;
        }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            var ls = (LocalizedString)o;
            int previewLength = 20;
            var text = ls.ToString().Remove(previewLength);
            if (ls.ToString().Length > previewLength) text += "...";
            w.WriteValue(string.Format($"LocalizedString:{ls.Key}:{text}"));
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
        private static readonly Type _tBlueprintScriptableObject = typeof(LocalizedString);

        public override bool CanConvert(Type type) => Enabled
          && _tBlueprintScriptableObject.IsAssignableFrom(type);
    }
}