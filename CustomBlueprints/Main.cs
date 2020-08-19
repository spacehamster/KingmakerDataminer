using UnityEngine;
using Harmony12;
using UnityModManagerNet;
using System.Reflection;
using System;
using UnityEngine.SceneManagement;
using Kingmaker.Blueprints;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Blueprints.CharGen;
using System.IO;
using Kingmaker;

namespace CustomBlueprints
{
#if DEBUG
    [EnableReloading]
#endif
    public class Main
    {
        public static ILogger logger;
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugLog(string msg)
        {
            if(logger != null) logger.Log(msg);
        }
        public static bool enabled;
        public static Settings settings;
        public static BlueprintManager blueprintManager;
        public static string ModPath = null;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
                //var harmony = HarmonyInstance.Create(modEntry.Info.Id);
                //harmony.PatchAll(Assembly.GetExecutingAssembly());
                modEntry.OnToggle = OnToggle;
                modEntry.OnGUI = OnGUI;
                modEntry.OnSaveGUI = OnSaveGUI;
#if DEBUG
                modEntry.OnUnload = Unload;
#endif
                ModPath = modEntry.Path;
                logger = new UMMLogger(modEntry.Logger);

                SceneManager.sceneLoaded += BlueprintManager.OnSceneManagerOnSceneLoaded;
            }
            catch (Exception e){
                modEntry.Logger.Log(e.ToString() +"\n" + e.StackTrace);
            }
            return true;
        }
        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            //HarmonyInstance.Create(modEntry.Info.Id).UnpatchAll();
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        static UnityEngine.Object FindObject2(int instanceId)
        {
            return null; //can't find FindObjectFromInstanceID 

        }
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            try
            {
                if (!enabled) return;
#if (DEBUG)
                GUILayout.Label($"Game Version: {GameVersion.GetVersion()}");
                if (GUILayout.Button("DumpAssets"))
                {
                    AssetsDump.DumpAssets();
                }
                if (GUILayout.Button("DumpClassRaceBlueprints"))
                {
                    AssetsDump.DumpQuick();
                }
                if (GUILayout.Button("DumpSampleOfBlueprints")){
                    AssetsDump.DumpBlueprints();
                }
                if (GUILayout.Button("DumpAllBlueprints"))
                {
                    AssetsDump.DumpAllBlueprints();
                }
                if (GUILayout.Button("DumpAllBlueprintsVerbose"))
                {
                    AssetsDump.DumpAllBlueprintsVerbose();
                }
                if (GUILayout.Button("DumpFlags"))
                {
                    var blueprints = ResourcesLibrary.GetBlueprints<BlueprintUnlockableFlag>();
                    Directory.CreateDirectory("Blueprints");
                    using (var file = new StreamWriter("Blueprints/log.txt"))
                    {
                        foreach (var blueprint in blueprints)
                        {
                            if (blueprint.AssetGuid.Length != 32) continue;
                            Main.DebugLog($"Dumping {blueprint.name} - {blueprint.AssetGuid}");
                            try
                            {
                                AssetsDump.DumpBlueprint(blueprint);
                            }
                            catch (Exception ex)
                            {
                                file.WriteLine($"Error dumping {blueprint.name}:{blueprint.AssetGuid}:{blueprint.GetType().FullName}, {ex.ToString()}");
                            }
                        }
                    }
                }
                if (GUILayout.Button("DumpEquipmentEntities"))
                {
                    AssetsDump.DumpEquipmentEntities();
                }
                if (GUILayout.Button("DumpUnitViews"))
                {
                    AssetsDump.DumpUnitViews();
                }
                if (GUILayout.Button("DumpList"))
                {
                    AssetsDump.DumpList();
                }
                if (GUILayout.Button("DumpScriptableObjects"))
                {
                    AssetsDump.DumpScriptableObjects();
                }
                if (GUILayout.Button("DumpAssetBundles"))
                {
                    AssetsDump.DumpAssetBundles();
                }
                if (GUILayout.Button("DumpUI"))
                {
                    AssetsDump.DumpUI();
                }
                if (GUILayout.Button("DumpSceneList"))
                {
                    AssetsDump.DumpSceneList();
                }
                if (GUILayout.Button("DumpKingdom"))
                {
                    AssetsDump.DumpKingdom();
                }
                if (GUILayout.Button("DumpView"))
                {
                    var view = ResourcesLibrary.TryGetResource<GameObject>("adf003833b2463543a065d5160c7e8f1");
                    var character = view.GetComponent<Character>();
                    JsonBlueprints.Dump(character, "adf003833b2463543a065d5160c7e8f1");
                }
                if (GUILayout.Button("TestLoad"))
                {
                    var vp = JsonBlueprints.Load<BlueprintRaceVisualPreset>("mods/customraces/data/TestPreset.json");
                    DebugLog("Loaded " + vp.name);
                }
#endif
            } catch(Exception e)
            {
                DebugLog(e.ToString() + " " + e.StackTrace);
            }
        }

    }
}
