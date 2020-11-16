using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using JetBrains.Annotations;
using Newtonsoft.Json.Converters;

namespace CustomBlueprints
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
                  typeof(Mesh),
                  typeof(Material),
                  typeof(AnimationCurve)
            }
        );

        public UnityJsonConverter() { }

        private static readonly JsonConverter[] NoConverters = Array.Empty<JsonConverter>();

        private static readonly JsonConverter[] SafeConverters = {
        new StringEnumConverter(true), new UnityJsonConverter()
    };
        public override void WriteJson(JsonWriter w, object value, JsonSerializer szr)
        {
            var type = JsonBlueprints.GetTypeName(value.GetType());
            switch (value)
            {
                case Vector2 v:
                    {
                        new JArray(v.x, v.y)
                          .WriteTo(w);
                        return;
                    }
                case Vector3 v:
                    {
                        new JArray(v.x, v.y, v.z)
                          .WriteTo(w);
                        return;
                    }
                case Vector4 v:
                    {
                        new JArray(v.x, v.y, v.z, v.w)
                          .WriteTo(w);
                        return;
                    }
                case Vector2Int v:
                    {
                        new JArray(v.x, v.y)
                          .WriteTo(w);
                        return;
                    }
                case Vector3Int v:
                    {
                        new JArray(v.x, v.y, v.z)
                          .WriteTo(w);
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
                          .WriteTo(w);
                        return;
                    }
                case Rect r:
                    {
                        // ReSharper disable once SimilarAnonymousTypeNearby // float
                        JObject.FromObject(new { r.x, r.y, r.width, r.height })
                          .WriteTo(w);
                        return;
                    }
                case RectInt r:
                    {
                        // ReSharper disable once SimilarAnonymousTypeNearby // int
                        JObject.FromObject(new { r.x, r.y, r.width, r.height })
                          .WriteTo(w);
                        return;
                    }
                case Bounds b:
                    {
                        // ReSharper disable once SimilarAnonymousTypeNearby // float
                        //Bounds stores vectors as center and extent internally,
                        //but size vector is serialized to be consistent with
                        //the constructor interface and with BoundsInt
                        new JArray(
                            new JArray(b.center.x, b.center.y, b.center.z),
                            new JArray(b.size.x, b.size.y, b.size.z)
                          )
                          .WriteTo(w);
                        return;
                    }
                case BoundsInt b:
                    {
                        // ReSharper disable once SimilarAnonymousTypeNearby // int
                        new JArray(
                            new JArray(b.center.x, b.center.y, b.center.z),
                            new JArray(b.size.x, b.size.y, b.size.z)
                          )
                          .WriteTo(w);
                        return;
                    }
                case Color c:
                    {
                        var a = new JArray(c.r, c.g, c.b, c.a);
                        a.WriteTo(w);
                        return;
                    }
                case Color32 c:
                    {
                        var a = new JArray(c.r, c.g, c.b, c.a);
                        a.WriteTo(w);
                        return;
                    }
                case Texture2D t:
                    {
                        var o = new JObject();
                        o.Add("$type", type);
                        o.Add("name", t.name);
                        o.WriteTo(w);
                        return;
                    }
                case Sprite s:
                    {
                        var o = new JObject();
                        o.Add("$type", type);
                        o.Add("name", s.name);
                        o.WriteTo(w);
                        return;
                    }
                case Mesh m:
                    {
                        var o = new JObject();
                        o.Add("$type", type);
                        o.Add("name", m.name);
                        o.WriteTo(w);
                        return;
                    }
                case Material m:
                    {
                        var o = new JObject();
                        o.Add("$type", type);
                        o.Add("name", m.name);
                        o.WriteTo(w);
                        return;
                    }
                case AnimationCurve ac:
                    {
                        var o = new JObject();
                        o.Add("$type", type);
                        o.Add("preWrapMode", ac.preWrapMode.ToString());
                        o.Add("postWrapMode", ac.postWrapMode.ToString());
                        var keys = new JArray();
                        foreach(var key in ac.keys)
                        {
                            var jkey = new JObject();
                            jkey.Add("time", key.time);
                            jkey.Add("value", key.value);
                            jkey.Add("inTangent", key.inTangent);
                            jkey.Add("outTangent", key.outTangent);
                            jkey.Add("inWeight", key.inWeight);
                            jkey.Add("outWeight", key.outWeight);
                            keys.Add(jkey);
                        }
                        o.Add("keys", keys);
                        o.WriteTo(w);
                        return;
                    }
            }
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override bool CanConvert(Type objectType) => SupportedTypes.Contains(objectType);
    }
}