using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CustomRaces
{
    public sealed class BlueprintContractResolver : DefaultContractResolver
    {
        private static readonly Type BlueprintScriptableObjectType = typeof(BlueprintScriptableObject);

        private static readonly HashSet<string> MemberNameBlacklist =
          new HashSet<string>(new string[] { "BlueprintsByAssetId", "LibraryObject", "BundleLoader", "Enumerator", "Current" });

        [CanBeNull]
        public Type RootBlueprintType
        {
            [UsedImplicitly]
            get => _rootBlueprintType;
            set
            {
                _rootBlueprintType = value;
                if (value == null)
                    return;
                if (!_rootBlueprintType.IsInstanceOfType(_rootBlueprint))
                    _rootBlueprint = null;
            }
        }

        [CanBeNull]
        public object RootBlueprint
        {
            [UsedImplicitly]
            get => _rootBlueprint;
            set
            {
                _rootBlueprint = value;
                if (value == null)
                    return;
                if (!(_rootBlueprintType?.IsInstanceOfType(value) ?? false))
                    _rootBlueprintType = value.GetType();
            }
        }

        public BlueprintContractResolver()
        {
            RootBlueprint = null;
            RootBlueprintType = null;
        }

        public BlueprintContractResolver(BlueprintScriptableObject rootBlueprint)
        {
            RootBlueprint = rootBlueprint;
            RootBlueprintType = rootBlueprint.GetType();
        }

        private static readonly HashSet<Type> ConverterBlacklist = new HashSet<Type>(new[] {
      typeof(Kingmaker.Game).Assembly
        .GetType("Kingmaker.EntitySystem.Persistence.JsonUtility.BlueprintConverter", false)
    });

        [CanBeNull]
        private Type _rootBlueprintType;

        [CanBeNull]
        private object _rootBlueprint;


        private static readonly BlueprintAssetIdConverter BlueprintAssetIdConverter
          = new BlueprintAssetIdConverter(true);

        private static readonly GameObjectAssetIdConverter GameObjectAssetIdConverter
          = new GameObjectAssetIdConverter(true);

        private static readonly JsonConverter[] PreferredConverters = {
      new StringEnumConverter(true),
      new IsoDateTimeConverter(),
      new XmlNodeConverter(),
      new VersionConverter(),
      new RegexConverter(),
      new UnityJsonConverter(true),
      GameObjectAssetIdConverter
    };

        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (objectType == null)
                return null;

            if (BlueprintScriptableObjectType.IsAssignableFrom(objectType))
              if (objectType != RootBlueprintType)
                return BlueprintAssetIdConverter;

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
                    return null;
                converter = base.ResolveContractConverter(objectType);
                if (converter == null)
                    return null;
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

            if (MemberNameBlacklist.Contains(member.Name))
            {
                Skip();
                return jsonProp;
            }

            if (!jsonProp.Writable)
            {
                Skip();
                return null;
            }

            if (member is FieldInfo field)
            {
                if (field.IsLiteral)
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
                if (!property.CanWrite)
                {
                    Skip();
                    return null;
                }

                var getterBody = property.GetMethod.GetMethodBody();
                if (getterBody == null
                  || getterBody.LocalVariables.Count > 0
                  || getterBody.GetILAsByteArray().Length > 12)
                {
                    Skip();
                    return null;
                }

                if (property.SetMethod?.GetMethodBody() == null)
                {
                    Skip();
                    return null;
                }

                // ReSharper disable once InvertIf
                if (property.PropertyType.IsSubclassOf(BlueprintScriptableObjectType))
                {
                    jsonProp.Converter = BlueprintAssetIdConverter;
                    jsonProp.IsReference = false;
                    Allow();
                }
            }
            else
            {
                throw new NotImplementedException($"Member type {member.MemberType} not implemented");
            }

            return jsonProp;
        }


        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).Where(p => p.Writable).ToArray();
        }
    }
}