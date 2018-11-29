using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using JetBrains.Annotations;
using Newtonsoft.Json.Converters;

namespace CustomRaces
{
    public class UnityJsonConverter : JsonConverter
    {
        [UsedImplicitly]
        public static readonly ISet<Type> SupportedTypes = new HashSet<Type>(
            new Type[] {
                  typeof(Vector2),
                  typeof(Vector3),
                  typeof(Vector4),
                  typeof(Vector2Int),
                  typeof(Vector3Int),
                  typeof(Matrix4x4),
                  typeof(Rect),
                  typeof(RectInt),
                  typeof(Bounds),
                  typeof(BoundsInt),
                  typeof(Color),
                  typeof(Color32),
                  typeof(Texture2D),
                  typeof(Sprite),
                  typeof(Mesh)
            }
        );

        [UsedImplicitly]
        public bool Enabled { get; set; }

        private UnityJsonConverter() { }

        public UnityJsonConverter(bool enabled)
        {
            Enabled = enabled;
        }

        private static readonly JsonConverter[] NoConverters = Array.Empty<JsonConverter>();

        private static readonly JsonConverter[] SafeConverters = {
      new StringEnumConverter(true), new UnityJsonConverter()
    };

        private static readonly string AqnTexture2D = typeof(Texture2D).AssemblyQualifiedName;
        private static readonly string AqnSprite = typeof(Sprite).AssemblyQualifiedName;
        private static readonly string AqnMesh = typeof(Mesh).AssemblyQualifiedName;

        public override void WriteJson(JsonWriter w, object value, JsonSerializer szr)
        {
            var type = value.GetType().Name;
            switch (value)
            {
                case Vector2 v:
                    {
                        new JArray(v.x, v.y)
                          .WriteTo(w, NoConverters);
                        return;
                    }
                case Vector3 v:
                    {
                        new JArray(v.x, v.y, v.z)
                          .WriteTo(w, NoConverters);
                        return;
                    }
                case Vector4 v:
                    {
                        new JArray(v.x, v.y, v.z, v.w)
                          .WriteTo(w, NoConverters);
                        return;
                    }
                case Vector2Int v:
                    {
                        new JArray(v.x, v.y)
                          .WriteTo(w, NoConverters);
                        return;
                    }
                case Vector3Int v:
                    {
                        new JArray(v.x, v.y, v.z)
                          .WriteTo(w, NoConverters);
                        return;
                    }
                case Matrix4x4 m:
                    {
                        new JArray(
                            new JArray(m.m00, m.m01, m.m02, m.m03),
                            new JArray(m.m10, m.m11, m.m12, m.m13),
                            new JArray(m.m20, m.m21, m.m22, m.m23),
                            new JArray(m.m30, m.m31, m.m32, m.m33)
                          )
                          .WriteTo(w, NoConverters);
                        return;
                    }
                case Rect r:
                    {
                        // ReSharper disable once SimilarAnonymousTypeNearby // float
                        JObject.FromObject(new { r.x, r.y, r.width, r.height })
                          .WriteTo(w, NoConverters);
                        return;
                    }
                case RectInt r:
                    {
                        // ReSharper disable once SimilarAnonymousTypeNearby // int
                        JObject.FromObject(new { r.x, r.y, r.width, r.height })
                          .WriteTo(w, NoConverters);
                        return;
                    }
                case Bounds b:
                    {
                        // ReSharper disable once SimilarAnonymousTypeNearby // float
                        JObject.FromObject(new { b.min, b.max })
                          .WriteTo(w, NoConverters);
                        return;
                    }
                case BoundsInt b:
                    {
                        // ReSharper disable once SimilarAnonymousTypeNearby // int
                        JObject.FromObject(new { b.min, b.max })
                          .WriteTo(w, NoConverters);
                        return;
                    }
                case Color c:
                    {
                        var lc = c.linear;
                        // ReSharper disable once SimilarAnonymousTypeNearby // float
                        JObject.FromObject(new { lc.r, lc.g, lc.b, lc.a })
                          .WriteTo(w, NoConverters);
                        return;
                    }
                case Color32 c:
                    {
                        // ReSharper disable once SimilarAnonymousTypeNearby // byte
                        JObject.FromObject(new { c.r, c.g, c.b, c.a })
                          .WriteTo(w, NoConverters);
                        return;
                    }
                case Texture2D t:
                    {
                        // Object.FindObjectFromInstanceID
                        var o = JObject.FromObject(new
                        {
                            instanceId = t.GetInstanceID(),
                            t.name,
                            t.width,
                            t.height,
                            t.format,
                            t.filterMode
                        });
                        o.Add("$type", type);
                        o.Add("name", t.name);
                        o.Add("InstanceId", t.GetInstanceID());
                        o.WriteTo(w, SafeConverters);
                        return;
                    }
                case Sprite s:
                    {
                        var o = JObject.FromObject(new
                        {
                            instanceId = s.GetInstanceID(),
                            s.name,
                            s.texture,
                            s.rect,
                            s.packed
                        });
                        o.Add("$type", type);
                        o.Add("name", s.name);
                        o.Add("InstanceId", s.GetInstanceID());
                        o.WriteTo(w, SafeConverters);
                        return;
                    }
                case Mesh m:
                    {
                        var o = JObject.FromObject(new
                        {
                            instanceId = m.GetInstanceID(),
                            m.name,
                            m.bounds
                        });
                        o.Add("$type", type);
                        o.Add("name", m.name);
                        o.Add("InstanceId", m.GetInstanceID());
                        o.WriteTo(w, SafeConverters);
                        return;
                    }
            }
        }

        public override object ReadJson(JsonReader r, Type type, object value, JsonSerializer szr)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) => Enabled && SupportedTypes.Contains(objectType);
    }
}