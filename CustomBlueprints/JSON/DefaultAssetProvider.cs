using System;
using Kingmaker.Blueprints;
using UnityEngine;

namespace CustomBlueprints
{
    class DefaultAssetProvider : IAssetProvider
    {
        public void AddBlueprint(BlueprintScriptableObject blueprint, string assetId)
        {
            BlueprintUtil.AddBlueprint(blueprint, assetId);
        }

        public string AddResource<T>(T resource, string path) where T : UnityEngine.Object
        {
            return BlueprintUtil.AddResource<T>(resource, path);
        }

        public BlueprintScriptableObject GetBlueprint(Type type, string assetId)
        {
            return ResourcesLibrary.TryGetBlueprint(assetId);
        }

        public UnityEngine.Object GetResource(Type type, string assetId)
        {
            return ResourcesLibrary.TryGetResource<UnityEngine.Object>(assetId);
        }

        public UnityEngine.Object GetUnityObject(Type type, int instanceId)
        {
            var result = BlueprintUtil.FindObjectByInstanceId(instanceId, type);
            if (result == null)
            {
                throw new System.Exception($"Couldn't find object with InstanceId {instanceId}");
            }
            return result;
        }
    }
}