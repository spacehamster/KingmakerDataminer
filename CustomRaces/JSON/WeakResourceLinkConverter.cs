using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ResourceLinks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRaces
{
    public class WeakResourceLinkConverter : JsonConverter
    {
        [UsedImplicitly]
        public bool Enabled { get; set; }

        private WeakResourceLinkConverter() { }

        public WeakResourceLinkConverter(bool enabled)
        {
            Enabled = enabled;
        }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            var resource = (WeakResourceLink)o;
            w.WriteValue(string.Format($"Resource:{resource.AssetId}:{ResourcesLibrary.LibraryObject.ResourcePathsByAssetId[resource.AssetId]}"));
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
        private static readonly Type _tWeakResourceLink = typeof(WeakResourceLink);

        public override bool CanConvert(Type type) => Enabled
            && _tWeakResourceLink.IsAssignableFrom(type);
    }
}