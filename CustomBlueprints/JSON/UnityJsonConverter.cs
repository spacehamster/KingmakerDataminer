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
                  typeof(Material)
            }
        );

        public UnityJsonConverter() { }

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
                        // Object.FindObjectFromInstanceID
                        var o = new JObject();
                        o.Add("$type", type);
                        o.Add("name", t.name);
                        o.Add("InstanceId", t.GetInstanceID());
                        o.WriteTo(w);
                        return;
                    }
                case Sprite s:
                    {
                        var o = new JObject();
                        o.Add("$type", type);
                        o.Add("name", s.name);
                        o.Add("InstanceId", s.GetInstanceID());
                        o.WriteTo(w);
                        return;
                    }
                case Mesh m:
                    {
                        var o = new JObject();
                        o.Add("$type", type);
                        o.Add("name", m.name);
                        o.Add("InstanceId", m.GetInstanceID());
                        o.WriteTo(w);
                        return;
                    }
                case Material m:
                    {
                        var o = new JObject();
                        o.Add("$type", type);
                        o.Add("name", m.name);
                        o.Add("InstanceId", m.GetInstanceID());
                        o.WriteTo(w);
                        return;
                    }
            }
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            JArray a = null;
            JObject o = null;
            if (reader.TokenType == JsonToken.StartArray)
            {
                a = JArray.Load(reader);
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                o = JObject.Load(reader);
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            if (type == typeof(Vector2)) return new Vector2((float)a[0], (float)a[1]);
            if (type == typeof(Vector3)) return new Vector3((float)a[0], (float)a[1], (float)a[2]);
            if (type == typeof(Vector4)) return new Vector4((float)a[0], (float)a[1], (float)a[2], (float)a[3]);
            if (type == typeof(Vector2Int)) return new Vector2Int((int)a[0], (int)a[1]);
            if (type == typeof(Vector3Int)) return new Vector3Int((int)a[0], (int)a[1], (int)a[2]);
            if (type == typeof(Rect)) return new Rect((float)a[0], (float)a[1], (float)a[2], (float)a[3]);
            if (type == typeof(RectInt)) return new RectInt((int)a[0], (int)a[1], (int)a[2], (int)a[3]);
            if (type == typeof(Color)) return new Color((float)a[0], (float)a[1], (float)a[2], (float)a[3]);
            if (type == typeof(Color32)) return new Color32((byte)a[0], (byte)a[1], (byte)a[2], (byte)a[3]);
            if (type == typeof(Matrix4x4))
            {
                var row0 = (JArray)a[0];
                var row1 = (JArray)a[1];
                var row2 = (JArray)a[2];
                var row3 = (JArray)a[3];
                return new Matrix4x4()
                {
                    m00 = (float)row0[0],
                    m01 = (float)row0[1],
                    m02 = (float)row0[2],
                    m03 = (float)row0[3],
                    m10 = (float)row1[0],
                    m11 = (float)row1[1],
                    m12 = (float)row1[2],
                    m13 = (float)row1[3],
                    m20 = (float)row2[0],
                    m21 = (float)row2[1],
                    m22 = (float)row2[2],
                    m23 = (float)row2[3],
                    m30 = (float)row3[0],
                    m31 = (float)row3[1],
                    m32 = (float)row3[2],
                    m33 = (float)row3[3],
                };
            }
            if (type == typeof(Bounds))
            {
                var a1 = (JArray)a[0];
                var a2 = (JArray)a[1];
                return new Bounds(
                    new Vector3((float)a1[0], (float)a1[1], (float)a1[2]),
                    new Vector3((float)a2[0], (float)a2[1], (float)a2[2])
                );
            }
            if (type == typeof(BoundsInt))
            {
                var a1 = (JArray)a[0];
                var a2 = (JArray)a[1];
                return new BoundsInt(
                     new Vector3Int((int)a1[0], (int)a1[1], (int)a1[2]),
                     new Vector3Int((int)a2[0], (int)a2[1], (int)a2[2])
                );
            }
            if (type == typeof(Texture2D) || type == typeof(Sprite) || type == typeof(Mesh) || type == typeof(Material))
            {
                int instanceId = (int)o["InstanceId"];
                var result = BlueprintUtil.FindObjectByInstanceId(instanceId, type);
                if (result == null) {
                    Main.DebugLog($"Couldn't find resource {type.Name}({instanceId}) {o["name"]}");
                } else
                {
                    Main.DebugLog($"Found resource {type.Name}({instanceId}) {result.name}");
                }
                return result;
            }
            return null;
        }
        public override bool CanConvert(Type objectType) => SupportedTypes.Contains(objectType);
    }
}