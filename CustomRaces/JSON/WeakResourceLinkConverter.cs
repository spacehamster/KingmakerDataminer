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
            string path = null;
            ResourcesLibrary.LibraryObject.ResourcePathsByAssetId.TryGetValue(resource.AssetId, out path);
            w.WriteValue(string.Format($"Resource:{resource.AssetId}:{path ?? "NULL"}"));
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            string text = (string)reader.Value;
            if (text == null || text == "null")
            {
                return null;
            }
            if (text.StartsWith("Resource"))
            {
                var parts = text.Split(':');
                var link = (WeakResourceLink)Activator.CreateInstance(type);
                link.AssetId = parts[1];
                return link;
            }
            throw new NotImplementedException($"Not implemented for type {type} with value {text}");
        }

        // ReSharper disable once IdentifierTypo
        private static readonly Type _tWeakResourceLink = typeof(WeakResourceLink);

        public override bool CanConvert(Type type) => Enabled
            && _tWeakResourceLink.IsAssignableFrom(type);
    }
}