using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomBlueprints
{
    class AssetsDump
    {
        public static void DumpBlueprints()
        {
            var seen = new HashSet<Type>();

            var blueprints = ResourcesLibrary.GetBlueprints<BlueprintScriptableObject>();
            foreach (var blueprint in blueprints)
            {
                if (!seen.Contains(blueprint.GetType()))
                {
                    seen.Add(blueprint.GetType());
                    JsonBlueprints.Dump(blueprint);
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
                if (types.Contains(blueprint.GetType())) JsonBlueprints.Dump(blueprint);
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
                    try
                    {
                        JsonBlueprints.Dump(blueprint);
                    }
                    catch (Exception ex)
                    {
                        file.WriteLine($"Error dumping {blueprint.name}:{blueprint.AssetGuid}:{blueprint.GetType().FullName}, {ex.ToString()}");
                    }
                }
            }
        }
        public static void DumpEquipmentEntities()
        {
            foreach (var kv in ResourcesLibrary.LibraryObject.ResourceNamesByAssetId)
            {
                var resource = ResourcesLibrary.TryGetResource<EquipmentEntity>(kv.Key);
                if (resource == null) continue;
                JsonBlueprints.Dump(resource, kv.Key);
            }
        }
        public static void DumpUnitViews()
        {
            foreach (var kv in ResourcesLibrary.LibraryObject.ResourceNamesByAssetId)
            {
                var resource = ResourcesLibrary.TryGetResource<UnitEntityView>(kv.Key);
                if (resource == null) continue;
                JsonBlueprints.Dump(resource, kv.Key);
            }
        }
        public static void DumpList()
        {
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
                foreach (var kv in ResourcesLibrary.LibraryObject.ResourceNamesByAssetId)
                {
                    file.WriteLine($"name\tAssetId\tType\nResourcenName\nInstanceId");
                    var resource = ResourcesLibrary.TryGetResource<UnityEngine.Object>(kv.Key);
                    if (resource != null)
                    {
                        var baseType = resource.GetType().IsAssignableFrom(typeof(UnityEngine.GameObject)) ? "GameObject" :
                                         resource.GetType().IsAssignableFrom(typeof(UnityEngine.ScriptableObject)) ? "ScriptableObject" :
                                         resource.GetType().IsAssignableFrom(typeof(UnityEngine.Component)) ? "Component" :
                                         "Object";
                    }
                    file.WriteLine($"{resource?.name ?? "NULL"}\t{kv.Key}\t{resource?.GetType()?.Name ?? "NULL"}\t{kv.Value}\t{resource?.GetInstanceID()}");
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
                if(bundle == null)
                {
                    file.WriteLine($"  NULL, IsLoaded {loadedBundles.ContainsKey(bundlePath)}");
                    continue;
                }
                foreach(var name in bundle.GetAllAssetNames())
                {
                    file.WriteLine($"  Name: {name}");
                    var asset = bundle.LoadAsset(name);
                    file.WriteLine($"    Asset: {asset.name}, {asset.GetType().Name}");
                    foreach (var subAsset in bundle.LoadAssetWithSubAssets(name))
                    {
                        file.WriteLine($"    SubAsset: {subAsset}, {subAsset.GetType().Name}");
                    }
                }
                foreach(var scenePath in bundle.GetAllScenePaths())
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
            JsonBlueprints.Dump(Game.Instance, "UI");
            JsonBlueprints.Dump(Game.Instance.UI, "UI");
            JsonBlueprints.Dump(Game.Instance.BlueprintRoot.UIRoot, "UI");
            JsonBlueprints.Dump(Game.Instance.DialogController, "UI");
        }
    }
}
