using Kingmaker.Blueprints;
using Kingmaker.ResourceLinks;
using Newtonsoft.Json;
using System;

namespace CustomBlueprints
{
    public class WeakResourceLinkConverter : JsonConverter
    {
        public WeakResourceLinkConverter() { }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            var resource = (WeakResourceLink)o;
            string path = null;
            ResourcesLibrary.LibraryObject.ResourceNamesByAssetId.TryGetValue(resource.AssetId, out path);
            w.WriteValue(string.Format($"Resource:{resource.AssetId}:{path ?? "NULL"}"));
        }
        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private static readonly Type _tWeakResourceLink = typeof(WeakResourceLink);

        public override bool CanConvert(Type type) => _tWeakResourceLink.IsAssignableFrom(type);
    }
}