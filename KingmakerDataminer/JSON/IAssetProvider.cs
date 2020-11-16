using Kingmaker.Blueprints;
using System;

namespace CustomBlueprints
{
    public interface IAssetProvider
    {
        void AddBlueprint(BlueprintScriptableObject blueprint, string assetId);
        BlueprintScriptableObject GetBlueprint(Type type, string assetId);
        UnityEngine.Object GetResource(Type type, string assetId);
        UnityEngine.Object GetUnityObject(Type type, int instanceId);
        string AddResource<T>(T resource, string path) where T : UnityEngine.Object;
    }
}