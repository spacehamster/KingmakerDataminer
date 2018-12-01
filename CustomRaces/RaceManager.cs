using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace CustomRaces
{
    public class RaceManager
    {
        static List<BlueprintRace> races = new List<BlueprintRace>();
        static List<BlueprintCharacterClass> characterClasses = new List<BlueprintCharacterClass>();
        static bool loaded = false;
        public static void Init()
        {
            var blueprints = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
            var goblinRace = (BlueprintRace)blueprints["9d168ca7100e9314385ce66852385451"];

            /*races.Add(goblinRace);
            races.Add(Drow.CreateRace());
            races.Add(Dhampir.CreateRace());
#if (DEBUG)
            races.Add(MeshTestRace.CreateRace());
#endif
            characterClasses.Add(Slayer.CreateClass());
#if(DEBUG)
            characterClasses.Add(Ninja.CreateClass());
#endif*/
        }
        static public void Reload()
        {
            var originalRaces = new List<BlueprintRace>();
            foreach(var race in Game.Instance.BlueprintRoot.Progression.CharacterRaces)
            {
                if (race.AssetGuid.EndsWith(RaceUtil.AssetSuffix)) continue;
                originalRaces.Add(race);
            }
            Game.Instance.BlueprintRoot.Progression.CharacterRaces = originalRaces.ToArray();
            var originalClasses = new List<BlueprintCharacterClass>();
            foreach (var characterClass in Game.Instance.BlueprintRoot.Progression.CharacterClasses)
            {
                if (characterClass.AssetGuid.EndsWith(RaceUtil.AssetSuffix)) continue;
                originalClasses.Add(characterClass);
            }
            Game.Instance.BlueprintRoot.Progression.CharacterClasses = originalClasses.ToArray();
            var blueprintsToRemove = ResourcesLibrary.LibraryObject.BlueprintsByAssetId.Where(
                (item) => item.Key.EndsWith("CustomFeature")).ToList();
            foreach(var kv in blueprintsToRemove)
            {
                ResourcesLibrary.LibraryObject.BlueprintsByAssetId.Remove(kv.Key);
            }
            Main.DebugLog($"Removed {blueprintsToRemove.Count} Blueprints");
            var loadedResources = ResourcesLibrary.LoadedResources;
            if (loadedResources == null) throw new Exception("No loaded resources");
            IDictionary resources = loadedResources as IDictionary;
            var resourcesToRemove = new List<string>();
            foreach(DictionaryEntry entry in resources)
            {
                if (entry.Key.ToString().EndsWith(RaceUtil.AssetSuffix)){
                    resourcesToRemove.Add(entry.Key);
                }
            }
            foreach(var key in resourcesToRemove)
            {
                resources.Remove(key);
            }
            Main.DebugLog($"Removing {resourcesToRemove.Count} Resources");
            races.Clear();
            characterClasses.Clear();
            Init();
            foreach (var race in races) AddRace(race);
            foreach (var characterClass in characterClasses) AddCharcterClass(characterClass);
        }
        static public void OnSceneManagerOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == SceneName.LoadingScreenUI ||
                scene.name == SceneName.MainMenu ||
                scene.name == SceneName.MainMenuBoard)
            {
                try
                {
                    if (!loaded) Init();
                    loaded = true;
                    foreach (var race in races) AddRace(race);
                    foreach (var characterClass in characterClasses) AddCharcterClass(characterClass);
                } catch(Exception e)
                {
                    Main.DebugLog(e.ToString() + "\n" + e.StackTrace);
                    throw e;
                }
            }
        }
        static public void AddRace(BlueprintRace race)
        {
            try
            {
                ref var charRaces = ref Game.Instance.BlueprintRoot.Progression.CharacterRaces;
                if (charRaces.Contains(race))
                {
                    return;
                }
                var l = charRaces.Length;
                Array.Resize(ref charRaces, l + 1);
                charRaces[l] = race;
                Main.DebugLog($"{race.name} added to the race list");
            }
            catch (Exception ex)
            {
                Main.DebugLog(ex.ToString());
                throw;
            }
        }
        static public void AddCharcterClass(BlueprintCharacterClass characterClass)
        {
            try
            {
                ref var charClasses = ref Game.Instance.BlueprintRoot.Progression.CharacterClasses;
                if (charClasses.Contains(characterClass))
                {
                    return;
                }
                var l = charClasses.Length;
                Array.Resize(ref charClasses, l + 1);
                charClasses[l] = characterClass;
                Main.DebugLog($"{characterClass.name} added to the class list");
            }
            catch (Exception ex)
            {
                Main.DebugLog(ex.ToString());
                throw;
            }
        }
    }
}
