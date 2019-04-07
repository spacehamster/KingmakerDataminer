using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Kingdom;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Harmony12;
using Kingmaker.Visual.Critters;
using UnityEngine.SceneManagement;

namespace CustomBlueprints
{
    public class AssetsDump
    {
        public static void DumpBlueprint(BlueprintScriptableObject blueprint)
        {
            JsonBlueprints.Dump(blueprint, $"Blueprints/{blueprint.GetType()}/{blueprint.name}.{blueprint.AssetGuid}.json");
        }
        public static void DumpBlueprints()
        {
            var seen = new HashSet<Type>();

            var blueprints = ResourcesLibrary.GetBlueprints<BlueprintScriptableObject>();
            foreach (var blueprint in blueprints)
            {
                if (!seen.Contains(blueprint.GetType()))
                {
                    seen.Add(blueprint.GetType());
                    DumpBlueprint(blueprint);
                }
            }
        }
        public static void DumpScriptableObjects()
        {
            Directory.CreateDirectory("ScriptableObjects");
            foreach (var obj in UnityEngine.Object.FindObjectsOfType<ScriptableObject>())
            {
                try
                {
                    if (obj is BlueprintScriptableObject blueprint &&
                        !ResourcesLibrary.LibraryObject.BlueprintsByAssetId.ContainsKey(blueprint.AssetGuid))
                    {
                        JsonBlueprints.Dump(blueprint, $"ScriptableObjects/{blueprint.GetType()}/{blueprint.name}.{blueprint.AssetGuid}.json");
                    }
                    else
                    {
                        JsonBlueprints.Dump(obj, $"ScriptableObjects/{obj.GetType()}/{obj.name}.{obj.GetInstanceID()}.json");
                    }
                }
                catch (Exception ex)
                {
                    File.WriteAllText($"ScriptableObjects/{obj.GetType()}/{obj.name}.{obj.GetInstanceID()}.txt", ex.ToString());
                }
            }
        }
        public static void DumpQuick()
        {
            var types = new HashSet<Type>()
            {
                typeof(BlueprintCharacterClass),
                typeof(BlueprintRaceVisualPreset),
                typeof(BlueprintRace),
                typeof(BlueprintArchetype),
                typeof(BlueprintProgression),
                typeof(BlueprintStatProgression),
                typeof(BlueprintFeature),
                typeof(BlueprintFeatureSelection),
                typeof(BlueprintSpellbook),
                typeof(BlueprintSpellList),
                typeof(BlueprintSpellsTable),
                typeof(BlueprintItemWeapon),
                typeof(BlueprintBuff)
            };
            foreach (var blueprint in ResourcesLibrary.GetBlueprints<BlueprintScriptableObject>())
            {
                if (types.Contains(blueprint.GetType())) DumpBlueprint(blueprint);
            }
        }
        public static void DumpAllBlueprints()
        {
            var blueprints = ResourcesLibrary.GetBlueprints<BlueprintScriptableObject>();
            Directory.CreateDirectory("Blueprints");
            using (var file = new StreamWriter("Blueprints/log.txt"))
            {
                foreach (var blueprint in blueprints)
                {
                    if (blueprint.AssetGuid.Length != 32) continue;
                    Main.DebugLog($"Dumping {blueprint.name} - {blueprint.AssetGuid}");
                    try
                    {
                        DumpBlueprint(blueprint);
                    }
                    catch (Exception ex)
                    {
                        file.WriteLine($"Error dumping {blueprint.name}:{blueprint.AssetGuid}:{blueprint.GetType().FullName}, {ex.ToString()}");
                    }
                }
            }
        }
        static void DumpResource(UnityEngine.Object resource, string assetId)
        {
            Directory.CreateDirectory($"Blueprints/{resource.GetType()}");
            JsonBlueprints.Dump(resource, $"Blueprints/{resource.GetType()}/{resource.name}.{assetId}.json");
        }
        public static void DumpEquipmentEntities()
        {
            foreach (var kv in ResourcesLibrary.LibraryObject.ResourceNamesByAssetId)
            {
                var resource = ResourcesLibrary.TryGetResource<EquipmentEntity>(kv.Key);
                if (resource == null) continue;
                DumpResource(resource, kv.Key);
                ResourcesLibrary.CleanupLoadedCache();
            }
        }
        public static void DumpUnitViews()
        {
            foreach (var kv in ResourcesLibrary.LibraryObject.ResourceNamesByAssetId)
            {
                var resource = ResourcesLibrary.TryGetResource<UnitEntityView>(kv.Key);
                if (resource == null) continue;
                DumpResource(resource, kv.Key);
                ResourcesLibrary.CleanupLoadedCache();
            }
        }
        public static void DumpList()
        {
            var resourceTypes = new Type[]
            {
                typeof(EquipmentEntity),
                typeof(Familiar),
                typeof(UnitEntityView),
                typeof(ProjectileView)
                //Note: PrefabLink : WeakResourceLink<GameObject> exists
            };
            Directory.CreateDirectory($"Blueprints/");
            var blueprints = ResourcesLibrary.GetBlueprints<BlueprintScriptableObject>().ToList();
            var blueprintsByAssetId = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
            Main.DebugLog($"BlueprintsByAssetId contains  {blueprintsByAssetId.Count} blueprints");
            Main.DebugLog($"Dumping {blueprints.Count} blueprints");
            using (var file = new StreamWriter("Blueprints/Blueprints.txt"))
            {
                file.WriteLine($"name\tAssetId\tType");
                foreach (var blueprint in blueprints)
                {
                    file.WriteLine($"{blueprint.name}\t{blueprint.AssetGuid}\t{blueprint.GetType()}");
                }
            }
            var resourcePathsByAssetId = ResourcesLibrary.LibraryObject.ResourceNamesByAssetId;
            Main.DebugLog($"ResourcePathsByAssetId contains  {blueprintsByAssetId.Count} resources");
            using (var file = new StreamWriter("Blueprints/Resources.txt"))
            {
                file.WriteLine($"Name\tAssetId\tType\tBaseType\tInstanceId");
                foreach (var kv in ResourcesLibrary.LibraryObject.ResourceNamesByAssetId)
                {
                    var resource = ResourcesLibrary.TryGetResource<UnityEngine.Object>(kv.Key);
                    if (resource != null)
                    {
                        var baseType = resource.GetType().IsAssignableFrom(typeof(UnityEngine.GameObject)) ? "GameObject" :
                                         resource.GetType().IsAssignableFrom(typeof(UnityEngine.ScriptableObject)) ? "ScriptableObject" :
                                         resource.GetType().IsAssignableFrom(typeof(UnityEngine.Component)) ? "Component" :
                                         "Object";
                        var go = resource as GameObject;
                        var typeName = resource?.GetType().Name ?? "NULL";
                        if (go != null)
                        {
                            foreach (var type in resourceTypes)
                            {
                                if (go.GetComponent(type) != null)
                                {
                                    typeName = type.Name;
                                }
                            }
                        }
                        file.WriteLine($"{kv.Value}\t{kv.Key}\t{typeName}\t{baseType}\t{resource?.GetInstanceID()}");
                        ResourcesLibrary.CleanupLoadedCache();
                    }
                }
            }
        }
        public static void DumpAssetBundles()
        {
            Directory.CreateDirectory($"Blueprints/");
            var bundles = Directory.GetFiles("Kingmaker_Data/StreamingAssets/Bundles")
                    .Select(f => Path.GetFileName(f));

            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles().ToDictionary(b => b.name);
            var file = new StreamWriter("Blueprints/AssetBundles.txt");
            file.WriteLine($"Loaded Bundles");
            foreach (var kv in loadedBundles)
            {
                file.WriteLine(kv.Key + $" IsNull {kv.Value == null}");
            }
            file.WriteLine("\nResourceBundles");
            foreach (var bundleNameExt in bundles)
            {
                if (!bundleNameExt.StartsWith("resource") || bundleNameExt.EndsWith("manifest")) continue;
                var bundleName = Path.GetFileNameWithoutExtension(bundleNameExt);
                string bundlePath = PathUtils.BundlePath(bundleName);
                var bundle = loadedBundles.ContainsKey(bundleName) ?
                    loadedBundles[bundleName] :
                    AssetBundle.LoadFromFile(bundlePath);
                file.WriteLine(bundleName);
                if (bundle == null)
                {
                    file.WriteLine($"  NULL, IsLoaded {loadedBundles.ContainsKey(bundlePath)}");
                    continue;
                }
                foreach (var name in bundle.GetAllAssetNames())
                {
                    file.WriteLine($"  Name: {name}");
                    var asset = bundle.LoadAsset(name);
                    file.WriteLine($"    Asset: {asset.name}, {asset.GetType().Name}");
                    foreach (var subAsset in bundle.LoadAssetWithSubAssets(name))
                    {
                        file.WriteLine($"    SubAsset: {subAsset}, {subAsset.GetType().Name}");
                    }
                }
                foreach (var scenePath in bundle.GetAllScenePaths())
                {
                    file.WriteLine($"  ScenePath: {scenePath}");
                }
                if (!loadedBundles.ContainsKey(bundle.name))
                {
                    bundle.Unload(true);
                }
            }
        }
        public static void DumpUI()
        {
            JsonBlueprints.DumpResource(Game.Instance, "UI/Game.json");
            JsonBlueprints.DumpResource(Game.Instance.UI, "UI/Game.UI.json");
            JsonBlueprints.DumpResource(Game.Instance.BlueprintRoot.UIRoot, "UI/Game.BlueprintRoot.UIRoot.json");
            JsonBlueprints.DumpResource(Game.Instance.DialogController, "UI/Game.DialogController.json");
            var ui = Game.Instance.UI;
            foreach (var field in ui.GetType().GetFields())
            {
                try
                {
                    var value = field.GetValue(ui);
                    if (value == null)
                    {
                        Main.DebugLog($"Null field {field.Name}");
                        continue;
                    }
                    JsonBlueprints.DumpResource(value, $"UI/UI.{value.GetType().FullName}.json");
                }
                catch (Exception ex)
                {
                    Main.DebugLog($"Error dumping UI field {field.Name}");
                }
            }
            foreach (var prop in ui.GetType().GetProperties())
            {
                try
                {
                    var value = prop.GetValue(ui);
                    if (value == null)
                    {
                        Main.DebugLog($"Null property {prop.Name}");
                        continue;
                    }
                    JsonBlueprints.DumpResource(value, $"UI/UI.{value.GetType().FullName}.json");
                }
                catch (Exception ex)
                {
                    Main.DebugLog($"Error dumping UI property {prop.Name}");
                }
            }
        }
        public static void DumpKingdom()
        {
            JsonBlueprints.Dump(KingdomState.Instance, "Kingdom");
        }
        public static void DumpSceneList()
        {
            string result = "";
            for(int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                var scene = SceneManager.GetSceneByBuildIndex(i);
                result += $"{i}\t{scenePath}\t{scene.name}";
            }
            Directory.CreateDirectory("Dump");
            File.WriteAllText("Dump/SceneList.txt", result);
        }
    }
}
