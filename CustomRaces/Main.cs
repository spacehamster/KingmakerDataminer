using UnityEngine;
using Harmony12;
using UnityModManagerNet;
using System.Reflection;
using System;
using Kingmaker;
using UnityEngine.SceneManagement;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Visual.CharacterSystem;
using Newtonsoft.Json;

namespace CustomRaces
{

    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger logger;
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugLog(string msg)
        {
            if(logger != null) logger.Log(msg);
        }
        public static bool enabled;
        public static Settings settings;
        static int torso = -1;
        public static RaceManager raceManager;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
                var harmony = HarmonyInstance.Create(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                modEntry.OnToggle = OnToggle;
                modEntry.OnGUI = OnGUI;
                modEntry.OnSaveGUI = OnSaveGUI;
                modEntry.Logger.Log("Loaded CustomRaces");
                logger = modEntry.Logger;
                SceneManager.sceneLoaded += RaceManager.OnSceneManagerOnSceneLoaded;
            }
            catch (Exception e){
                modEntry.Logger.Log(e.ToString() +"\n" + e.StackTrace);
            }
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
                if (GUILayout.Button("DumpBlueprintsQuick"))
                {
                    AssetsDump.DumpQuick();
                }
                if (GUILayout.Button("DumpBlueprints")){
                    AssetsDump.DumpBlueprints();
                }
                if (GUILayout.Button("DumpAllBlueprints"))
                {
                    AssetsDump.DumpAllBlueprints();
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
                if (GUILayout.Button("TestLoad"))
                {
                    //var bp = JsonBlueprints.Load<BlueprintCharacterClass>("mods/customraces/data/slayerclass.json");
                    //DebugLog("Loaded " + (bp?.name ?? "NULL"));
                    var info = BlueprintInfo.Load();
                    DebugLog("Loaded " + info.Classes[0].name);
                }
                if (GUILayout.Button("FindObject"))
                {
                    var go = RaceUtil.FindObjectByInstanceId<GameObject>(270194);
                    DebugLog("FindByID " + go == null ? "NULL" : go.name); //OH_LongswordThieves

                    var sprite = RaceUtil.FindObjectByInstanceId<Sprite>(45820);
                    DebugLog(sprite == null ? "NULL" : sprite.name); //OH_LongswordThieves
                }
                if (GUILayout.Button("Reload"))
                {
                    RaceManager.Reload();
                }
                int newTorso = (int)GUILayout.HorizontalSlider(torso, -1, MeshTestRace.testAssets.Length - 1, GUILayout.Width(300));
                GUILayout.Label("Torso: " + newTorso);
                if(torso != newTorso)
                {
                    torso = newTorso;
                    MeshTestRace.ChooseTorso(torso);
                }
#endif
            } catch(Exception e)
            {
                DebugLog(e.ToString() + " " + e.StackTrace);
            }
        }

    }
}
