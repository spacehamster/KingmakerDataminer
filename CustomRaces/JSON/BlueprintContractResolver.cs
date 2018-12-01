using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace CustomRaces
{
    public sealed class BlueprintContractResolver : DefaultContractResolver
    {
        private static readonly Type BlueprintScriptableObjectType = typeof(BlueprintScriptableObject);
        [CanBeNull]
        public Type RootBlueprintType
        {
            [UsedImplicitly]
            get => _rootBlueprintType;
            set
            {
                _rootBlueprintType = value;
            }
        }

        public BlueprintContractResolver()
        {
            RootBlueprintType = null;
        }

        public BlueprintContractResolver(Type rootType)
        {
            RootBlueprintType = rootType;
        }

        private static readonly HashSet<Type> ConverterBlacklist = new HashSet<Type>(new[] {
          typeof(Kingmaker.Game).Assembly
            .GetType("Kingmaker.EntitySystem.Persistence.JsonUtility.BlueprintConverter", false)
        });

        [CanBeNull]
        private Type _rootBlueprintType;

        /*
         * PrototypeLink is only used to check if blueprint is a companion
         * will need to fix if custom companions are wanted
         */
        private static readonly HashSet<FieldInfo> FieldBlacklist = new HashSet<FieldInfo>(new[] {
          typeof(PrototypeableObjectBase).GetField("PrototypeLink")
        });

        private static readonly BlueprintAssetIdConverter BlueprintAssetIdConverter
          = new BlueprintAssetIdConverter(true);

        private static readonly JsonConverter[] PreferredConverters = {
         new StringEnumConverter(true),
          new IsoDateTimeConverter(),
          new XmlNodeConverter(),
          new VersionConverter(),
          new RegexConverter(),
          //new BlueprintComponentConverter(true),
          new LocalizedStringConverter(true),
          new WeakResourceLinkConverter(true),
          new UnityJsonConverter(true),
          new GameObjectAssetIdConverter(true)
        };

        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (objectType == null)
                return null;

            if (BlueprintScriptableObjectType.IsAssignableFrom(objectType))
                if (objectType != RootBlueprintType)
                {
                    //Main.DebugLog($"Using {BlueprintAssetIdConverter.GetType()} for {objectType}");
                    return BlueprintAssetIdConverter;
                }
            var prefCnv = PreferredConverters.FirstOrDefault(cnv => cnv.CanConvert(objectType));
            if (prefCnv != null)
                return prefCnv;

            var converter = base.ResolveContractConverter(objectType);
            if (converter == null)
                return null;
            while (ConverterBlacklist.Contains(converter.GetType()))
            {
                objectType = objectType.BaseType;
                if (objectType == null)
                {
                    converter = null;
                    break;
                }
                converter = base.ResolveContractConverter(objectType);
                if (converter == null) break;
            }
            //Main.DebugLog($"Found converter {converter?.GetType().ToString() ?? "NULL"} for {objectType}");
            return null;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProp = base.CreateProperty(member, memberSerialization);

            void Skip()
            {
                jsonProp.ShouldSerialize = o => false;
                jsonProp.ShouldDeserialize = o => false;
            }

            void Allow()
            {
                jsonProp.ShouldSerialize = o => true;
                jsonProp.ShouldDeserialize = o => true;
            }
            if (member is FieldInfo field)
            {
                //Main.DebugLog($"Serializing field {field.ReflectedType}.{field.Name}");
                jsonProp.Readable = true;
                jsonProp.Writable = true;
                //Readonly field
                if (field.IsInitOnly)
                {
                    //Main.DebugLog($"Skipping readonly field {field.ReflectedType}.{field.Name}");
                    Skip();
                    return null;
                }
                if (FieldBlacklist.Contains(field))
                {
                    Skip();
                    return null;
                }
                // ReSharper disable once InvertIf
                if (field.FieldType.IsSubclassOf(BlueprintScriptableObjectType))
                {
                    jsonProp.Converter = BlueprintAssetIdConverter;
                    jsonProp.IsReference = false;
                    Allow();
                }
            }
            else if (member is PropertyInfo property)
            {
            }
            else
            {
                throw new NotImplementedException($"Member type {member.MemberType} not implemented");
            }

            return jsonProp;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).ToArray();
        }
        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            return JsonBlueprints.GetUnitySerializableMembers(objectType);
        }
    }
}