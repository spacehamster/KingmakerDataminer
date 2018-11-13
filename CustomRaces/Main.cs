using UnityEngine;
using Harmony12;
using UnityModManagerNet;
using Kingmaker.Blueprints;
using System.Reflection;
using System;
using Debug = System.Diagnostics.Debug;
using System.Diagnostics;
using System.Linq;
using Kingmaker.Visual.CharacterSystem;
using System.Collections.Generic;
using Kingmaker.Blueprints.Classes;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.ResourceLinks;
using Kingmaker.Blueprints.Root;
using static Kingmaker.Visual.CharacterSystem.EquipmentEntity;
using static CustomRaces.Settings;
using System.Threading;
using UnityEngine.SceneManagement;

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
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            try
            {
                if (!enabled) return;
                if (GUILayout.Button("LogRace"))
                {
                    var player = Game.Instance.Player.MainCharacter.Value;
                    var descriptor = player.Descriptor;
                    var race = descriptor.Progression.Race;
                    DebugLog("MainCharacter Race is " + race.name + " " + race.AssetGuid);
                }
                int newTorso = (int)GUILayout.HorizontalSlider(torso, -1, MeshTestRace.testAssets.Length - 1, GUILayout.Width(300));
                GUILayout.Label("Torso: " + newTorso);
                if(torso != newTorso)
                {
                    torso = newTorso;
                    MeshTestRace.ChooseTorso(torso);
                }
            } catch(Exception e)
            {
                DebugLog(e.ToString() + " " + e.StackTrace);
            }
        }

    }
}
