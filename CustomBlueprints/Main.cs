using UnityEngine;
using Harmony12;
using UnityModManagerNet;
using System.Reflection;
using System;
using UnityEngine.SceneManagement;
using Kingmaker.Blueprints;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Blueprints.CharGen;

namespace CustomBlueprints
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
        public static BlueprintManager raceManager;
        public static string ModPath = null;
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
                ModPath = modEntry.Path;
                logger = modEntry.Logger;
                SceneManager.sceneLoaded += BlueprintManager.OnSceneManagerOnSceneLoaded;
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
                    //var info = BlueprintInfo.Load();
                    //DebugLog("Loaded " + info.Classes[0].name);
                    var vp = JsonBlueprints.Load<BlueprintRaceVisualPreset>("mods/customraces/data/TestPreset.json");
                    DebugLog("Loaded " + vp.name);
                }
                /*
                 * UnityEngine.Networking.NetworkTransport.GetAssetId(go) //returns ""
                 * internal static extern bool Object.DoesObjectWithInstanceIDExist(int instanceID); //returns true
                 * internal static extern Object Object.FindObjectFromInstanceID(int instanceID); // returns CR_Hair_VioletDark_U_HM
                 * Resources.FindObjectsOfTypeAll<Texture2D>() // returns CR_Hair_VioletDark_U_HM after it has been loaded with Resource.Load
                 */
                if (GUILayout.Button("FindObject"))
                {
                    var go = BlueprintUtil.FindObjectByInstanceId<GameObject>(270194);
                    DebugLog("FindByID " + go == null ? "NULL" : go.name); //OH_LongswordThieves

                    var sprite = BlueprintUtil.FindObjectByInstanceId<Sprite>(45820);
                    DebugLog(sprite == null ? "NULL" : sprite.name); //OH_LongswordThieves

                    var texture1 = BlueprintUtil.FindObjectByInstanceId<Texture2D>(552466);
                    DebugLog(texture1 == null ? "NULL" : texture1.name); //CR_Hair_VioletDark_U_HM

                    var humanHair = ResourcesLibrary.TryGetResource<EquipmentEntity>("a9558cfc0705d4e48af7ecd2ebd75411"); //EE_Hair_HairLongWavy_M_HM

                    var texture2 = BlueprintUtil.FindObjectByInstanceId<Texture2D>(552466);
                    DebugLog(texture2 == null ? "NULL" : texture2.name); //CR_Hair_VioletDark_U_HM
                }
                if (GUILayout.Button("FindObject2"))
                {

                    var doesExist =  Traverse.Create<UnityEngine.Object>().Method("DoesObjectWithInstanceIDExist", new object[] { 552466 }).GetValue<bool>();
                    DebugLog($"Does resource exist first {doesExist}");
                    var tex1 = Traverse.Create<UnityEngine.Object>().Method("FindObjectFromInstanceID", new object[] { 552466 }).GetValue<UnityEngine.Object>();
                    DebugLog(tex1 == null ? "NULL" : tex1.name); //CR_Hair_VioletDark_U_HM

                    var humanHair = ResourcesLibrary.TryGetResource<EquipmentEntity>("a9558cfc0705d4e48af7ecd2ebd75411"); //EE_Hair_HairLongWavy_M_HM

                    doesExist = Traverse.Create<UnityEngine.Object>().Method("DoesObjectWithInstanceIDExist", new object[] { 552466 }).GetValue<bool>();
                    DebugLog($"Does resource exist second {doesExist}");
                    var tex2 = Traverse.Create<UnityEngine.Object>().Method("FindObjectFromInstanceID", new object[] { 552466 }).GetValue<UnityEngine.Object>();
                    DebugLog(tex2 == null ? "NULL" : tex2.name); //CR_Hair_VioletDark_U_HM


                    var go = (GameObject)BlueprintUtil.FindObjectByInstanceId<GameObject>(270194);
                    DebugLog("FindByID " + go == null ? "NULL" : go.name); //OH_LongswordThieves

                    var assetId = UnityEngine.Networking.NetworkTransport.GetAssetId(go);
                    if (assetId == null) assetId = "NULL";
                    if (assetId == "") assetId = "Empty";
                    DebugLog($"AssetId: {assetId}");

                }

                if (GUILayout.Button("Reload"))
                {
                    BlueprintManager.Reload();
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
