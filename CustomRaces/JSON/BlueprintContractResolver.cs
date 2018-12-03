using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
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
          new LocalizedStringConverter(true),
          new WeakResourceLinkConverter(true),
          new UnityJsonConverter(true),
          new GameObjectAssetIdConverter(true)
        };
        void OnDeserializing(object o, StreamingContext context)
        {
            //After construction, before initialization
            Main.DebugLog("OnDeserializing BlueprintProgression");
        }
        void OnDeserialized(object o, StreamingContext context)
        {
            //After construction, after initialization
            Main.DebugLog("OnDeserialized BlueprintProgression");
        }
        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);
            if(objectType == typeof(BlueprintProgression))
            {
                contract.OnDeserializingCallbacks.Add(OnDeserializing);
                contract.OnDeserializedCallbacks.Add(OnDeserialized);
            }
            return contract;
        }
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (objectType == null)
                return null;

            if (BlueprintScriptableObjectType.IsAssignableFrom(objectType))
                if (objectType != RootBlueprintType)
                {
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
                jsonProp.Readable = true;
                jsonProp.Writable = true;
                //Readonly field
                if (field.IsInitOnly)
                {
                    Skip();
                    return null;
                }
                if (FieldBlacklist.Contains(field))
                {
                    Skip();
                    return null;
                }
                if (field.FieldType.IsSubclassOf(BlueprintScriptableObjectType))
                {
                    //MemberConverter required to deserialize see 
                    //https://stackoverflow.com/questions/24946362/custom-jsonconverter-is-ignored-for-deserialization-when-using-custom-contract-r
                    jsonProp.MemberConverter = BlueprintAssetIdConverter;
                    jsonProp.Converter = BlueprintAssetIdConverter;
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