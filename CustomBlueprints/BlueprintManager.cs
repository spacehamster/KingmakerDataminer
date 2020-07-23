using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace CustomBlueprints
{
    public class BlueprintManager
    {
        static List<BlueprintRace> races = new List<BlueprintRace>();
        static List<BlueprintCharacterClass> characterClasses = new List<BlueprintCharacterClass>();
        static List<BlueprintFeature> feats = new List<BlueprintFeature>();
        static BlueprintInfo info = null;
        static bool loaded = false;
        public static void Init()
        {
            var blueprints = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
            LoadStrings();
            info = BlueprintInfo.Load();
            races.AddRange(info.Races);
            characterClasses.AddRange(info.Classes);
            feats.AddRange(info.Feats);
        }
        public static void LoadStrings()
        {
            Dictionary<string, string> strings = null;
            var currentLocale = LocalizationManager.CurrentLocale;
            if (File.Exists($"{Main.ModPath}/data/localization/{currentLocale}.json"))
            {
                strings = JsonBlueprints.Load<Dictionary<string, string>>($"{Main.ModPath}/data/localization/{currentLocale}.json");
            } else if(File.Exists($"{Main.ModPath}/data/localization/enGB.json"))
            {
                Main.DebugLog($"Could not find locale {currentLocale}");
                strings = JsonBlueprints.Load<Dictionary<string, string>>($"{Main.ModPath}/data/localization/enGB.json");
            } else
            {
                Main.DebugLog($"Could not find any localization file");
            }
            if (strings == null) return;
            foreach(var kv in strings)
            {
                if (LocalizationManager.CurrentPack.Strings.ContainsKey(kv.Key))
                {
                    Main.DebugLog($"Duplicate localization string key {kv.Key}");
                } else
                {
                    LocalizationManager.CurrentPack.Strings[kv.Key] = kv.Value;
                }
            }
        }
        static public void OnSceneManagerOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == SceneName.LoadingScreenUI ||
                scene.name == SceneName.MainMenu ||
                scene.name == SceneName.MainMenuBoard)
            {
                /*try
                {
                    if (!loaded) Init();
                    loaded = true;
                    foreach (var race in races) AddRace(race);
                    foreach (var characterClass in characterClasses) AddCharcterClass(characterClass);
                    foreach (var feature in feats) AddFeature(feature);
                } catch(Exception e)
                {
                    Main.DebugLog(e.ToString() + "\n" + e.StackTrace);
                    loaded = true;
                    throw e;
                }*/
            }
        }
        static public void AddFeature(BlueprintFeature feature)
        {
            var basicFeatSelection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("247a4068296e8be42890143f451b4b45");
            try
            {
                ref var allFeatures = ref basicFeatSelection.AllFeatures;
                if (allFeatures.Contains(feature))
                {
                    return;
                }
                var l = allFeatures.Length;
                Array.Resize(ref allFeatures, l + 1);
                allFeatures[l] = feature;
                Main.DebugLog($"{feature.name} of type {feature.GetType()} added to the class list");
            }
            catch (Exception ex)
            {
                Main.DebugLog(ex.ToString());
                throw;
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
