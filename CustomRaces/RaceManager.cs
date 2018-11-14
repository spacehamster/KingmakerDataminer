using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace CustomRaces
{
    public class RaceManager
    {
        static List<BlueprintRace> races = new List<BlueprintRace>();
        static public Dictionary<string, UnityEngine.Object> assets = new Dictionary<string, UnityEngine.Object>();
        static bool loaded = false;
        public static void Init()
        {
            var blueprints = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
            var goblinRace = (BlueprintRace)blueprints["9d168ca7100e9314385ce66852385451"];
            races.Add(goblinRace);
            races.Add(Drow.CreateRace());
            races.Add(Dhampir.CreateRace());
            races.Add(MeshTestRace.CreateRace());
            races.Add(SkeletonRace.CreateRace());
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
                    Main.DebugLog($"Already had a {race.name} in the race list");
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
    }
}
