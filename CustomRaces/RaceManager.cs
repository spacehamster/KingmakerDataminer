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
        static List<BlueprintCharacterClass> characterClasses = new List<BlueprintCharacterClass>();
        static bool loaded = false;
        public static void Init()
        {
            var blueprints = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
            var goblinRace = (BlueprintRace)blueprints["9d168ca7100e9314385ce66852385451"];
            races.Add(goblinRace);
            races.Add(Drow.CreateRace());
            races.Add(Dhampir.CreateRace());
            races.Add(MeshTestRace.CreateRace());
            //AasimarFix.Apply();
            characterClasses.Add(Slayer.CreateClass());
            characterClasses.Add(Ninja.CreateClass());
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
