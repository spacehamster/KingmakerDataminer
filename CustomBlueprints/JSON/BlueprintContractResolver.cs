using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CustomBlueprints
{
    public sealed class BlueprintContractResolver : DefaultContractResolver
    {
        public BlueprintContractResolver()
        {

        }
        private static readonly HashSet<Type> ConverterBlacklist = new HashSet<Type>(new[] {
          typeof(Kingmaker.Game).Assembly
            .GetType("Kingmaker.EntitySystem.Persistence.JsonUtility.BlueprintConverter", false)
        });
        private readonly BlueprintAssetIdConverter BlueprintAssetIdConverter
          = new BlueprintAssetIdConverter();
        private readonly BlueprintConverter BlueprintConverter
            = new BlueprintConverter();
        private static StringEnumConverter stringEnumConverter = new StringEnumConverter(false);
        public List<JsonConverter> PreferredConverters = new List<JsonConverter>() {
          stringEnumConverter,
          new IsoDateTimeConverter(),
          new XmlNodeConverter(),
          new VersionConverter(),
          new RegexConverter(),
          new TagListConverter(),
          new ArrayConverter(),
          new TMPConverter(),
          new TMPFontConverter(),
          new UnityEventConverter(),
          new ScriptableObjectConverter(),
          new LocalizedStringConverter(),
          new WeakResourceLinkConverter(),
          new UnityJsonConverter(),
          new GameObjectAssetIdConverter(),
          new UnitCustomizationVariationConverter()
        };
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (objectType == null) return null;
            if (typeof(BlueprintScriptableObject).IsAssignableFrom(objectType))
            {
                if (BlueprintConverter.CanRead && BlueprintConverter.CanWrite) return BlueprintConverter;
                else return BlueprintAssetIdConverter;
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
            return null;
        }
        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);
            if (typeof(BlueprintScriptableObject).IsAssignableFrom(objectType))
            {
                contract.IsReference = false;
                contract.OnSerializedCallbacks.Add((o, context) =>
                {
                    contract.Converter = BlueprintConverter;
                });
                contract.OnSerializingCallbacks.Add((o, context) =>
                {
                    contract.Converter = BlueprintAssetIdConverter;
                });
            }
            return contract;
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
                jsonProp.Readable = true;
                jsonProp.Writable = true;
                if (JsonBlueprints.IsBlacklisted(field))
                {
                    Skip();
                    return null;
                }
                if (field.FieldType == typeof(long))
                {
                    var attribute = field.GetCustomAttribute<LongAsEnumAttribute>();
                    if (attribute != null)
                    {
                        jsonProp.ValueProvider = new LongAsEnumValueProvider(field, attribute.EnumType);
                        jsonProp.PropertyType = attribute.EnumType;
                        jsonProp.MemberConverter = stringEnumConverter;
                        jsonProp.Converter = stringEnumConverter;
                    }
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

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            return JsonBlueprints.GetUnitySerializableMembers(objectType).Distinct().ToList();
        }
    }
}